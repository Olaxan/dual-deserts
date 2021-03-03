﻿using System.Collections;
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
	public float csgLodRadius = 5f;

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

			var ops = operations[node.Center];

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

	List<Vector3Int> GetNeighbourPositions(Vector3Int p0, int size)
	{
		var positions = new List<Vector3Int>(26);

		for (int i = 0; i < 27; i++)
		{
			int x = i / 9;
			int w = i % 9;
			int y = w / 3;
			int z = w % 3;

			Vector3Int p = new Vector3Int(
					p0.x + size * (x - 1),
					p0.y + size * (y - 1),
					p0.z + size * (z - 1));

			if (p != p0)
				positions.Add(p);
		}

		return positions;
	}
	
	List<Chunk> GetNeighbours(Chunk chunk)
	{
		Vector3Int p0 = chunk.GridPos;
		int s = (int)chunk.Size;

		Chunk c;
		List<Chunk> chunks = new List<Chunk>();

		foreach (var pos in GetNeighbourPositions(chunk.GridPos, (int)chunk.Size))
		{
			if (loadedChunks.TryGetValue(pos, out c))
				chunks.Add(c);
		}

		return chunks;
	}

	float CheckCSGRadius(CSG operation, Vector3Int pos, int size)
	{
		Vector3 s = Vector3.one * size / 2f; // /2?
		Vector3 pc = operation.position - pos;
		pc.x = Mathf.Abs(pc.x);
		pc.y = Mathf.Abs(pc.y);
		pc.z = Mathf.Abs(pc.z);
		pc -= s;

		float m = Mathf.Max(pc.x, pc.y, pc.z);

		return m;
	}

	float CheckCSGRadius(CSG operation, Chunk chunk)
	{
		return CheckCSGRadius(operation, chunk.GridPos, (int)chunk.Size);
	}

	bool ExceedsChunk(CSG operation, Chunk chunk)
	{
		return operation.radius > Mathf.Abs(CheckCSGRadius(operation, chunk));
	}

	Vector3Int GetLODVolume(Vector3 pos, int level)
	{
		int lodLevelSize = lodLogicalVolumeSize * (level + 1);
		int halfSize = lodLevelSize / 2;

		return new Vector3Int(
				Mathf.FloorToInt(pos.x / lodLevelSize) * lodLevelSize + halfSize,
				Mathf.FloorToInt(pos.y / lodLevelSize) * lodLevelSize + halfSize,
				Mathf.FloorToInt(pos.z / lodLevelSize) * lodLevelSize + halfSize);
	}

	public void AddOperation(CSG operation)
	{
		
		int lodLevel = Mathf.CeilToInt(operation.radius / csgLodRadius);

		Debug.Log($"Operation {operation.type}, {operation.shape} at {operation.position} (lodLevel = {lodLevel})");

		for (int i = 0; i < lodLevel; i++)
		{
			var pos = GetLODVolume(operation.position, i);

			operations.Add(pos, operation);

			Chunk chunk;
			if (loadedChunks.TryGetValue(pos, out chunk))
				contourGenerator.RequestRemesh(chunk, operations[pos], -1);

			foreach (var p in GetNeighbourPositions(pos, lodLogicalVolumeSize))
			{
				if (CheckCSGRadius(operation, p, lodLogicalVolumeSize) < operation.radius)
				{
					operations.Add(p, operation);

					Debug.Log($"Operation overflow: {p}, c = {operations[p].Count}");

					if (loadedChunks.TryGetValue(p, out chunk))
						contourGenerator.RequestRemesh(chunk, operations[p], 0);
				}
			}
		}
	}

	public void UpdateChunks()
    {
		foreach (var chunk in loadedChunks.Values)
		{
			var csg = operations[chunk.GridPos];
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
