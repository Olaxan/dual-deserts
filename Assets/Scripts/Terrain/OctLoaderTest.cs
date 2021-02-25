using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[RequireComponent(typeof(CSContourGenerator))]
public class OctLoaderTest : MonoBehaviour
{

	public int voxelSize = 64;

	public int lodVolumeSize = 1024;
	public int lodLogicalVolumeSize = 64;

	public bool drawBounds = false;

	public Material defaultMaterial;

	TerrainOctree world;
	public TerrainObject viewer;
	public List<TerrainObject> worldObjects;

	Queue<Chunk> unloadedChunks;

	Vector3Int lastPos;

	CSContourGenerator contourGenerator;

    void Start()
    {
		Setup();
		EvaluateTree();	
    }

    void Update()
    {
    }

	void Setup()
	{
		unloadedChunks = new Queue<Chunk>();
		world = new TerrainOctree(lodVolumeSize, Vector3Int.zero, lodLogicalVolumeSize);

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

	void EvaluateTree()
	{
		Vector3 tp = viewer.terrainObject.transform.position;

		int halfSize = lodLogicalVolumeSize / 2;
		int halfVolumeSize = lodVolumeSize / 2;

		Vector3Int chunk = new Vector3Int(
				Mathf.FloorToInt(tp.x / lodLogicalVolumeSize),
				Mathf.FloorToInt(tp.y / lodLogicalVolumeSize),
				Mathf.FloorToInt(tp.z / lodLogicalVolumeSize)
				);

		Vector3Int gridPos = new Vector3Int(
			Mathf.RoundToInt(tp.x / halfVolumeSize),
			Mathf.RoundToInt(tp.y / halfVolumeSize),
			Mathf.RoundToInt(tp.z / halfVolumeSize)
				) * halfVolumeSize;

		if (chunk != lastPos)
		{
			Debug.Log($"Moved to {chunk}");
			lastPos = chunk;
			world.Evaluate(viewer, worldObjects);
			gameObject.name = $"OctLoader {transform.childCount} children ({unloadedChunks.Count} queued)";
		}
	}

	public void UpdateChunks()
    {
		//foreach (var chunk in loadedChunks.Values)
		//	contourGenerator.RequestRemesh(chunk, 0);
		EvaluateTree();	
    }

	void OnDrawGizmos()
	{
		if (Application.isPlaying && drawBounds)
		{
			world.DrawAllBounds();
		}
	}
}
