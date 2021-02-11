using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CSContourGenerator))]
public class TerrainLoader : MonoBehaviour
{

	public Transform viewer;

	public int viewDistance = 3;
	public Vector3 worldScale = Vector3.one;
	public Vector3Int volumeSize = new Vector3Int(16, 16, 16);

	public Material defaultMaterial;
	Vector3Int oldChunk;

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
		Vector3 scaleSize = Vector3.Scale(volumeSize, worldScale);

		Vector3Int viewChunk = new Vector3Int(
				Mathf.RoundToInt(viewPos.x / scaleSize.x),
				Mathf.RoundToInt(viewPos.y / scaleSize.y),
				Mathf.RoundToInt(viewPos.z / scaleSize.z));

		int sqrDist = viewDistance * viewDistance;

		if (viewChunk != oldChunk)
		{
			oldChunk = viewChunk;

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

			for (int x = -viewDistance; x < viewDistance; x++)
			{
				for (int y = -viewDistance; y < viewDistance; y++)
				{
					for (int z = -viewDistance; z < viewDistance; z++)
					{
						Vector3Int pos = new Vector3Int(x, y, z);
						Vector3Int offsetPos = pos + viewChunk;

						if (loadedChunks.ContainsKey(offsetPos))
							continue;

						if (pos.sqrMagnitude > sqrDist)
							continue;

						Chunk newChunk;
						if (unloadedChunks.Count > 0)
							newChunk = unloadedChunks.Dequeue();
						else
							newChunk = AddChunk();

						Vector3 chunkOffset = offsetPos * (volumeSize - Vector3Int.one * 2);
						newChunk.Refresh(offsetPos, Vector3.Scale(chunkOffset, worldScale));
						loadedChunks.Add(offsetPos, newChunk);
						chunks.Add(newChunk);
						contourGenerator.RequestRemesh(newChunk);

					}
				}
			}
		}
	}

	public void UpdateAll()
	{
		foreach (Chunk chunk in chunks)
			contourGenerator.RequestRemesh(chunk);
	}
}
