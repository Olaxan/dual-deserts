using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(CSContourGenerator))]
public class OctLoaderTest : MonoBehaviour
{

	public int adjacency = 1;
	public float lodVolumeSize = 1024;
	public float logicalVolumeSize = 64;
	public int voxelSize = 64;

	public bool drawBounds = false;

	public GameObject viewer;
	public Material defaultMaterial;

	PointOctree<int> world;
	HashSet<PointOctreeNode<int>> loadedNodes;
	Dictionary<Vector3, Chunk> loadedChunks;
	Queue<Chunk> unloadedChunks;

	Vector3Int lastPos;

	CSContourGenerator contourGenerator;

    void Start()
    {
		Setup();
    }

    void Update()
    {
		ChunkStep();	
    }

	void Setup()
	{
		loadedChunks = new Dictionary<Vector3, Chunk>();
		unloadedChunks = new Queue<Chunk>();
		loadedNodes = new HashSet<PointOctreeNode<int>>();

		lastPos = Vector3Int.one * 999;

		contourGenerator = gameObject.GetComponent<CSContourGenerator>();
		contourGenerator.Setup(voxelSize, 1.0f);
	}

	Chunk AddChunk()
	{
		GameObject obj = new GameObject();
		obj.transform.parent = gameObject.transform;
		Chunk comp = obj.AddComponent<Chunk>();
		comp.defaultMaterial = defaultMaterial;
		comp.Setup();

		return comp;
	}

	void RecycleChunk(Chunk chunk)
	{
		chunk.gameObject.SetActive(false);
		unloadedChunks.Enqueue(chunk);
	}

	void ChunkStep()
	{
		Vector3 tp = viewer.transform.position;

		Vector3Int chunk = new Vector3Int(
				Mathf.RoundToInt(tp.x / logicalVolumeSize),
				Mathf.RoundToInt(tp.y / logicalVolumeSize),
				Mathf.RoundToInt(tp.z / logicalVolumeSize)
				);

		if (chunk != lastPos)
		{
			RebuildTree(tp);
			lastPos = chunk;
		}
	}

	void RebuildTree(Vector3 tp)
	{
		float halfVolumeSize = lodVolumeSize / 2.0f;

		Vector3 gridPos = new Vector3(
			Mathf.Round(tp.x / halfVolumeSize),
			Mathf.Round(tp.y / halfVolumeSize),
			Mathf.Round(tp.z / halfVolumeSize)
				) * halfVolumeSize;

		world = new PointOctree<int>(lodVolumeSize, gridPos, logicalVolumeSize);

		int adjSqr = adjacency * adjacency;
		int adjCube = adjacency * adjSqr;

		for (int i = 0; i < adjCube; i++)
		{
			int x = i / (adjSqr);
			int w = i % (adjSqr);
			int y = w / adjacency;
			int z = w % adjacency;

			Vector3 adj = tp + new Vector3(x, y, z) * logicalVolumeSize;
			adj -= Vector3.one * (adjacency - 1) * logicalVolumeSize / 2;
			world.Add(i, adj);
		}

		HashSet<PointOctreeNode<int>> newNodes = world.GetAllLeafNodes();

		var chunksToLoad = newNodes.Except(loadedNodes);
		var chunksToUnload = loadedNodes.Except(newNodes);

		int c = 0;
		foreach (var chunkNode in chunksToLoad)
		{
			c++;

			Chunk newChunk;
			if (unloadedChunks.Count > 0)
				newChunk = unloadedChunks.Dequeue();
			else 
				newChunk = AddChunk();

			float dist = (tp - chunkNode.Center).sqrMagnitude;

			loadedChunks[chunkNode.Center] = newChunk;
			newChunk.Refresh(chunkNode.Center, chunkNode.SideLength);
			contourGenerator.RequestRemesh(newChunk, Mathf.RoundToInt(dist));
		}
		
		foreach (var chunkNode in chunksToUnload)
		{
			Chunk unloadChunk = loadedChunks[chunkNode.Center];
			loadedChunks.Remove(chunkNode.Center);

			RecycleChunk(unloadChunk);
		}

		loadedNodes = newNodes;
	}

	public void UpdateChunks()
    {
		foreach (var chunk in loadedChunks.Values)
			contourGenerator.RequestRemesh(chunk, 0);
    }

	void OnDrawGizmos()
	{
		if (Application.isPlaying && drawBounds)
		{
			world.DrawAllBounds();
			world.DrawAllObjects();
		}
	}
}
