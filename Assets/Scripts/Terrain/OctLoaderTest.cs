using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CSContourGenerator))]
public class OctLoaderTest : MonoBehaviour
{

	public int adjacency = 1;
	public float lodVolumeSize = 1024;
	public float volumeSize = 64;

	public int tick = 16;	
	int tickCounter = 0;

	PointOctree<int> world;
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
       if (tickCounter++ >= tick)
	   {
		   RebuildTree();
		   tickCounter = 0;
	   }
    }

	void Setup()
	{
		loadedChunks = new Dictionary<Vector3, float>();
		unloadedChunks = new Queue<Chunk>();

		contourGenerator = gameObject.GetComponent<CSContourGenerator>();
		contourGenerator.Setup(Mathf.RoundToInt(volumeSize), 1.0f);
	}

	void RebuildTree()
	{
		float halfVolumeSize = lodVolumeSize / 2.0f;

		Vector3 tp = transform.position;

		Vector3 gridPos = new Vector3(
			Mathf.Round(tp.x / halfVolumeSize),
			Mathf.Round(tp.y / halfVolumeSize),
			Mathf.Round(tp.z / halfVolumeSize)
				) * halfVolumeSize;

		world = new PointOctree<int>(lodVolumeSize, gridPos, volumeSize);

		int adjSqr = adjacency * adjacency;
		int adjCube = adjacency * adjSqr;

		for (int i = 0; i < adjCube; i++)
		{
			int x = i / (adjSqr);
			int w = i % (adjSqr);
			int y = w / adjacency;
			int z = w % adjacency;

			Vector3 adj = tp + new Vector3(x, y, z) * volumeSize;
			adj -= Vector3.one * (adjacency - 1) * volumeSize / 2;
			world.Add(i, adj);
		}

		var chunkNodes = world.GetAllLeafNodes();

		int c = 0;
		foreach (var chunkNode in chunkNodes)
		{
			if (loadedChunks.ContainsKey(chunkNode.Center))
				continue;

			loadedChunks[chunkNode.Center] = chunkNode.SideLength;
			c++;
		}

		Debug.Log($"{c} chunks need rebuilding ({chunkNodes.Count} total)");
	}

	void OnDrawGizmos()
	{
		if (Application.isPlaying)
		{
			world.DrawAllBounds();
			world.DrawAllObjects();
		}
	}
}
