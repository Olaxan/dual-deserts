using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MEC;


[RequireComponent(typeof(CSContourGenerator))]
public class OctLoaderTest : MonoBehaviour
{

	public int tickRate = 16;

	[Header("Voxel Settings")]
	public int voxelSize = 64;

	[Header("LOD Settings")]
	public int lodVolumeSize = 1024;
	public int lodLogicalVolumeSize = 64;
	public int lodFadeOutFrames = 30;
	public float lodOverlap = 1f;
	public bool drawBounds = false;

	[Header("CSG Settings")]
	public int csgLogicalVolumeSize = 64;

	[Header("Material Settings")]
	public Material defaultMaterial;

	TerrainOctree world;
	public List<TerrainObject> worldObjects;

	Dictionary<Vector3Int, Chunk> loadedChunks;
	Queue<Chunk> unloadedChunks;

	MultiMap<Vector3Int, CSG> operations;

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
		operations = new MultiMap<Vector3Int, CSG>();

		contourGenerator = gameObject.GetComponent<CSContourGenerator>();
		contourGenerator.Setup(voxelSize);
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
			float overlap = lodOverlap * (node.SideLength / lodLogicalVolumeSize - 1);
			newChunk.Refresh(node.Center, node.SideLength + overlap);

			var opLoc = GetCSGVolume(node.Center);
			var ops = operations[opLoc];

			contourGenerator.RequestRemesh(newChunk, ops, node.SideLength);
		}
	}

	void UnloadChunks(HashSet<TerrainOctreeNode> nodes)
	{
		Chunk chunk;
		foreach (var node in nodes)
		{
			if (loadedChunks.TryGetValue(node.Center, out chunk))
			{
				loadedChunks.Remove(node.Center);
				RecycleChunk(chunk);
				//Timing.RunCoroutine(_FadeAndRecycle(chunk));
			}
			else
				Debug.Log($"No chunk {node.Center} / {node.SideLength}!");

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

	Vector3Int GetCSGVolume(Vector3 pos)
	{
		return new Vector3Int(
				Mathf.FloorToInt(pos.x / csgLogicalVolumeSize),
				Mathf.FloorToInt(pos.y / csgLogicalVolumeSize),
				Mathf.FloorToInt(pos.z / csgLogicalVolumeSize));
	
	}

	Vector3Int GetLODVolume(Vector3 pos, int level)
	{
		int halfVol = lodLogicalVolumeSize / 2 * (level + 1);

		return new Vector3Int(
				Mathf.FloorToInt(pos.x / lodLogicalVolumeSize) * lodLogicalVolumeSize + halfVol,
				Mathf.FloorToInt(pos.y / lodLogicalVolumeSize) * lodLogicalVolumeSize + halfVol,
				Mathf.FloorToInt(pos.z / lodLogicalVolumeSize) * lodLogicalVolumeSize + halfVol);
	}

	public void AddOperation(CSG operation)
	{
		var opPos = GetCSGVolume(operation.position);
		var lodPos = GetLODVolume(operation.position, 0);

		operations.Add(opPos, operation);
		
		Debug.Log($"Operation {operation.type}, {operation.shape} at {lodPos} ({opPos})");

		Chunk chunk;
		if (loadedChunks.TryGetValue(lodPos, out chunk))
		{
			contourGenerator.RequestRemesh(chunk, operations[opPos], 0);
		}
	}

	public void UpdateChunks()
    {
		foreach (var chunk in loadedChunks.Values)
		{
			var opPos = GetCSGVolume(chunk.WorldPos);
			var csg = operations[opPos];
			contourGenerator.RequestRemesh(chunk, csg, -1);
		}
    }

	void OnDrawGizmos()
	{
		if (Application.isPlaying && drawBounds)
		{
			world.DrawAllBounds();
		}
	}
}
