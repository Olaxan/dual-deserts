using System.Collections;
using System.Collections.Generic;
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
	List<Chunk> chunks;
	Dictionary<Vector3, float> loadedChunks;
	Queue<Chunk> unloadedChunks;

	CSContourGenerator contourGenerator;

    void Start()
    {
		Setup();
		RebuildTree();
    }

    void Update()
    {
    }

	void Setup()
	{
		chunks = new List<Chunk>();
		loadedChunks = new Dictionary<Vector3, float>();
		unloadedChunks = new Queue<Chunk>();

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

	void RebuildTree()
	{
		float halfVolumeSize = lodVolumeSize / 2.0f;

		Vector3 tp = viewer.transform.position;

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

		var chunkNodes = world.GetAllLeafNodes();

		int c = 0;
		foreach (var chunkNode in chunkNodes)
		{
			if (loadedChunks.ContainsKey(chunkNode.Center))
				continue;

			c++;

			Chunk newChunk;
			if (unloadedChunks.Count > 0)
				newChunk = unloadedChunks.Dequeue();
			else 
				newChunk = AddChunk();

			float dist = (tp - chunkNode.Center).sqrMagnitude;

			newChunk.Refresh(chunkNode.Center, chunkNode.SideLength);
			loadedChunks[chunkNode.Center] = chunkNode.SideLength;
			chunks.Add(newChunk);
			contourGenerator.RequestRemesh(newChunk, Mathf.RoundToInt(dist));
		}

		Debug.Log($"{c} chunks need rebuilding ({chunkNodes.Count} total)");
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
