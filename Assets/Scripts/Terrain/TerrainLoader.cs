using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CSContourGenerator))]
public class TerrainLoader : MonoBehaviour
{

	public Transform viewer;
	public int updateFrequency = 16;

	[Header("Draw Distance Settings")]
	public int viewDistance = 3;
	public int viewDepth = 1;

	[Header("World Settings")]
	public float volumeScale = 1.0f;
	public int volumeSize = 64;

	public Material defaultMaterial;

	CSContourGenerator contourGenerator;

	List<Chunk> chunks;
	Queue<Chunk> unloadedChunks;
	Dictionary<Vector3Int, Chunk> loadedChunks;

	int updateCounter = 0;

	void Start()
    {
       Setup(); 
	   UpdateChunks();
    }

	void Update()
	{
		if (updateCounter++ % updateFrequency == 0)
        {
			UpdateChunks();
			updateCounter = 0;
        }
	}

	void Setup()
	{
		chunks = new List<Chunk>();
		unloadedChunks = new Queue<Chunk>();
		loadedChunks = new Dictionary<Vector3Int, Chunk>();

		contourGenerator = gameObject.GetComponent<CSContourGenerator>();
		contourGenerator.Setup(volumeSize, volumeScale);
	}

	Chunk AddChunk()
	{
		GameObject chunkObj = new GameObject();
		chunkObj.transform.parent = gameObject.transform;
		Chunk chunkComp = chunkObj.AddComponent<Chunk>();
		chunkComp.defaultMaterial = defaultMaterial;
		chunkComp.Setup();

		return chunkComp;
	}

	void UpdateChunks()
	{
		
		Vector3 viewPos = viewer.position;
		int adjustedVolumeSize = volumeSize - 2;
		float scaledVolumeSize = adjustedVolumeSize * volumeScale;

		Bounds volumeBounds = new Bounds(Vector3.zero, Vector3.one * scaledVolumeSize);
		Plane[] camPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);

		Vector3Int viewChunk = new Vector3Int(
				Mathf.RoundToInt(viewPos.x / scaledVolumeSize),
				Mathf.RoundToInt(viewPos.y / scaledVolumeSize),
				Mathf.RoundToInt(viewPos.z / scaledVolumeSize));

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

					Vector3 chunkOffset =  (Vector3)offsetPos * scaledVolumeSize;
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
		int adjustedVolumeSize = volumeSize - 2;
		float scaledVolumeSize = adjustedVolumeSize * volumeScale;

		Vector3Int viewChunk = new Vector3Int(
				Mathf.RoundToInt(viewPos.x / scaledVolumeSize),
				Mathf.RoundToInt(viewPos.y / scaledVolumeSize),
				Mathf.RoundToInt(viewPos.z / scaledVolumeSize));

		foreach (Chunk chunk in chunks)
		{
			contourGenerator.RequestRemesh(chunk, (chunk.position - viewChunk).sqrMagnitude);
		}
	}
}
