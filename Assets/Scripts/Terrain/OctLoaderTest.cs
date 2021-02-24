using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(CSContourGenerator))]
public class OctLoaderTest : MonoBehaviour
{

	public int adjacency = 1;
	public int lodVolumeSize = 1024;
	public int lodTresholdMult = 4;
	public int logicalVolumeSize = 64;
	public int voxelSize = 64;

	public bool drawBounds = false;

	public GameObject viewer;
	public Material defaultMaterial;

	PointOctreeInt<int> world;
	HashSet<PointOctreeIntNode<int>> loadedNodes;
	Dictionary<Vector3Int, Chunk> loadedChunks;
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
		loadedChunks = new Dictionary<Vector3Int, Chunk>();
		unloadedChunks = new Queue<Chunk>();
		loadedNodes = new HashSet<PointOctreeIntNode<int>>();

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

		int halfSize = logicalVolumeSize / 2;

		Vector3Int chunk = new Vector3Int(
				Mathf.FloorToInt(tp.x / (logicalVolumeSize / lodTresholdMult)),
				Mathf.FloorToInt(tp.y / (logicalVolumeSize / lodTresholdMult)),
				Mathf.FloorToInt(tp.z / (logicalVolumeSize / lodTresholdMult))
				);

		if (chunk != lastPos)
		{
			Debug.Log($"Moved to {chunk}");
			RebuildTree(tp);
			lastPos = chunk;

			gameObject.name = $"OctLoader {transform.childCount} children ({unloadedChunks.Count} queued)";
		}
	}

	void RebuildTree(Vector3 tp)
	{
		int halfVolumeSize = lodVolumeSize / 2;

		Vector3Int gridPos = new Vector3Int(
			Mathf.RoundToInt(tp.x / halfVolumeSize),
			Mathf.RoundToInt(tp.y / halfVolumeSize),
			Mathf.RoundToInt(tp.z / halfVolumeSize)
				) * halfVolumeSize;

		world = new PointOctreeInt<int>(lodVolumeSize, gridPos, logicalVolumeSize);

		int adjSqr = adjacency * adjacency;
		int adjCube = adjacency * adjSqr;

		for (int i = 0; i < adjCube; i++)
		{
			int x = i / (adjSqr);
			int w = i % (adjSqr);
			int y = w / adjacency;
			int z = w % adjacency;

			Vector3Int adj = Vector3Int.RoundToInt(tp) + new Vector3Int(x, y, z) * logicalVolumeSize;
			adj -= Vector3Int.one * (adjacency - 1) * logicalVolumeSize / 2;
			world.Add(i, adj);
		}

		HashSet<PointOctreeIntNode<int>> newNodes = world.GetAllLeafNodes();

		var comparer = new PointOctreeIntNodeEqualityComparer<int>();

		var chunksToLoad = newNodes.Except(loadedNodes, comparer);
		var chunksToUnload = loadedNodes.Except(newNodes, comparer);

		loadedNodes = newNodes;

		Debug.Log($"Loading {chunksToLoad.Count()} chunks of {newNodes.Count} total...");
		foreach (var chunkNode in chunksToLoad)
		{
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
		
		Debug.Log($"Unloading {chunksToUnload.Count()} chunks...");
		foreach (var chunkNode in chunksToUnload)
		{
			if (loadedChunks.ContainsKey(chunkNode.Center))
			{
				Chunk unloadChunk = loadedChunks[chunkNode.Center];
				loadedChunks.Remove(chunkNode.Center);

				RecycleChunk(unloadChunk);
			}
			else Debug.Log($"{chunkNode.Center} not loaded!");
		}

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
