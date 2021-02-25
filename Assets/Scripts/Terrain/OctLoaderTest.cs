using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MEC;


[RequireComponent(typeof(CSContourGenerator))]
public class OctLoaderTest : MonoBehaviour
{

	public int tickRate = 16;

	public int voxelSize = 64;

	public int lodVolumeSize = 1024;
	public int lodLogicalVolumeSize = 64;
	public int lodFadeOutFrames = 30;

	public bool drawBounds = false;

	public Material defaultMaterial;

	TerrainOctree world;
	public List<TerrainObject> worldObjects;

	Dictionary<Vector3Int, Chunk> loadedChunks;
	Queue<Chunk> unloadedChunks;

	int tick = 0;
	int tickRemeshDelay = 0;

	CSContourGenerator contourGenerator;

    void Start()
    {
		Setup();
		PrewarmTree();
    }

    void Update()
    {
		if (tick++ >= Mathf.Max(tickRate, tickRemeshDelay))
		{
			EvaluateTree();
		}
    }

	void Setup()
	{
		loadedChunks = new Dictionary<Vector3Int, Chunk>();
		unloadedChunks = new Queue<Chunk>();
		world = new TerrainOctree(lodVolumeSize, Vector3Int.zero, lodLogicalVolumeSize);

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

	IEnumerator<float> _FadeAndRecycle(Chunk chunk)
    {
		for (int i = 0; i < lodFadeOutFrames; i++)
        {
			chunk.Opacity = 1f - (float)i / lodFadeOutFrames;
			yield return Timing.WaitForOneFrame;
        }

		RecycleChunk(chunk);
    }

	void RecycleChunk(Chunk chunk)
	{
		chunk.gameObject.SetActive(false);
		unloadedChunks.Enqueue(chunk);
	}

	void LoadChunks(HashSet<TerrainOctreeNode> nodes)
	{
		foreach (var node in nodes)
		{
			Chunk newChunk = (unloadedChunks.Count > 0) ? unloadedChunks.Dequeue() : AddChunk();
			loadedChunks.Add(node.Center, newChunk);
			newChunk.Refresh(node.Center, node.SideLength);

			contourGenerator.RequestRemesh(newChunk, node.SideLength);
		}
	}

	void UnloadChunks(HashSet<TerrainOctreeNode> nodes)
	{
		foreach (var node in nodes)
		{
			if (!loadedChunks.ContainsKey(node.Center))
			{
				Debug.Log($"No chunk {node.Center} / {node.SideLength}!");
				continue;
			}

			Chunk chunk = loadedChunks[node.Center];
			loadedChunks.Remove(node.Center);
			//RecycleChunk(chunk);
			Timing.RunCoroutine(_FadeAndRecycle(chunk));
		}
	}

	void PrewarmTree()
	{
		var unloadedNodes = new HashSet<TerrainOctreeNode>();
		var newNodes = new HashSet<TerrainOctreeNode>();

		world.Evaluate(worldObjects, newNodes, unloadedNodes);

		LoadChunks(newNodes);

		tickRemeshDelay = newNodes.Count;

		gameObject.name = $"OctLoader {transform.childCount} children ({unloadedChunks.Count} queued)";
	}

	void EvaluateTree()
	{
		var unloadedNodes = new HashSet<TerrainOctreeNode>();
		var newNodes = new HashSet<TerrainOctreeNode>();

		world.Evaluate(worldObjects, newNodes, unloadedNodes);

		UnloadChunks(unloadedNodes);
		LoadChunks(newNodes);

		tickRemeshDelay = newNodes.Count;

		gameObject.name = $"OctLoader {transform.childCount} children ({unloadedChunks.Count} queued)";
	}

	int piss = 0;
	public void FadeChunk()
    {
		Chunk c = loadedChunks.ElementAt(piss++).Value;
		Timing.RunCoroutine(_FadeAndRecycle(c));
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
		}
	}
}
