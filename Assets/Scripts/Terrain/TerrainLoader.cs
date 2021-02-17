using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CSContourGenerator))]
public class TerrainLoader : MonoBehaviour
{

	public Transform viewer;

	public int viewDistance = 3;
	public int viewDepth = 1;
	public Vector3 worldScale = Vector3.one;
	public Vector3Int volumeSize = new Vector3Int(16, 16, 16);

	public Material defaultMaterial;
	public Terrain distantTerrain;

	CSContourGenerator contourGenerator;

	List<Chunk> chunks;
	Queue<Chunk> unloadedChunks;
	Dictionary<Vector3Int, Chunk> loadedChunks;

    void Start()
    {
       Setup(); 
	   UpdateChunks();
    }

	void Update()
	{
		UpdateChunks();	
	}

	void Setup()
	{
		chunks = new List<Chunk>();
		unloadedChunks = new Queue<Chunk>();
		loadedChunks = new Dictionary<Vector3Int, Chunk>();

		contourGenerator = gameObject.GetComponent<CSContourGenerator>();		
		contourGenerator.Setup(volumeSize, worldScale);
	}

	Chunk AddChunk()
	{
		GameObject chunkObj = new GameObject();
		chunkObj.transform.parent = gameObject.transform;
		Chunk chunkComp = chunkObj.AddComponent<Chunk>();
		chunkComp.defaultMaterial = defaultMaterial;
		chunkComp.Setup();

		Debug.Log($"Adding chunk to pool (count = {chunks.Count})");

		return chunkComp;
	}

	void UpdateChunks()
	{
		
		Vector3 viewPos = viewer.position;
		Vector3Int adjustedVolumeSize = (volumeSize - Vector3Int.one * 2);
		Vector3 scaleSize = Vector3.Scale(adjustedVolumeSize, worldScale);

		Bounds volumeBounds = new Bounds(Vector3.zero, scaleSize);
		Plane[] camPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);

		Vector3Int viewChunk = new Vector3Int(
				Mathf.RoundToInt(viewPos.x / scaleSize.x),
				Mathf.RoundToInt(viewPos.y / scaleSize.y),
				Mathf.RoundToInt(viewPos.z / scaleSize.z));

		int sqrDist = viewDistance * viewDistance;

		for (int i = chunks.Count - 1; i >= 0; i--)
		{
			Chunk chunk = chunks[i];

			Vector3Int originPos = chunk.position - viewChunk;
			if (originPos.sqrMagnitude > sqrDist)
			{
				loadedChunks.Remove(chunk.position);
				unloadedChunks.Enqueue(chunk);
				chunks.RemoveAt(i);
			}
		}

		for (int x = -viewDistance; x <= viewDistance; x++)
		{
			for (int y = -viewDepth; y <= viewDepth; y++)
			{
				for (int z = -viewDistance; z <= viewDistance; z++)
				{
					Vector3Int pos = new Vector3Int(x, y, z);
					Vector3Int offsetPos = pos + viewChunk;
					int posSqrDist = pos.sqrMagnitude;

					if (loadedChunks.ContainsKey(offsetPos))
						continue;

					if (posSqrDist > sqrDist)
						continue;

					Vector3 chunkOffset = Vector3.Scale(offsetPos, scaleSize);
					volumeBounds.center = chunkOffset;

					if (!CheckVisible(camPlanes, volumeBounds) && posSqrDist > 1)
						continue;

					Chunk newChunk;
					if (unloadedChunks.Count > 0)
						newChunk = unloadedChunks.Dequeue();
					else
						newChunk = AddChunk();

					
					newChunk.Refresh(offsetPos, chunkOffset);
					loadedChunks.Add(offsetPos, newChunk);
					chunks.Add(newChunk);
					contourGenerator.RequestRemesh(newChunk, posSqrDist);

				}
			}
		}
	}

	bool CheckVisible(Plane[] frustum, Bounds bounds)
	{
		return GeometryUtility.TestPlanesAABB(frustum, bounds);
	}

	public void UpdateAll()
	{
		Vector3 viewPos = viewer.position;
		Vector3Int adjustedVolumeSize = (volumeSize - Vector3Int.one * 2);
		Vector3 scaleSize = Vector3.Scale(adjustedVolumeSize, worldScale);
		Vector3Int viewChunk = new Vector3Int(
				Mathf.RoundToInt(viewPos.x / scaleSize.x),
				Mathf.RoundToInt(viewPos.y / scaleSize.y),
				Mathf.RoundToInt(viewPos.z / scaleSize.z));

		foreach (Chunk chunk in chunks)
		{
			contourGenerator.RequestRemesh(chunk, (chunk.position - viewChunk).sqrMagnitude);
		}
	}

	public void UpdateDistantTerrain()
    {
		Vector3 viewPos = viewer.position;
		Vector3Int adjustedVolumeSize = (volumeSize - Vector3Int.one * 2);
		Vector3 scaleSize = Vector3.Scale(adjustedVolumeSize, worldScale);
		Vector2Int viewChunk = new Vector2Int(
				Mathf.RoundToInt(viewPos.x / scaleSize.x),
				Mathf.RoundToInt(viewPos.z / scaleSize.z));
		
		contourGenerator.SurfaceRemesh(distantTerrain.terrainData, viewChunk);
	}
}
