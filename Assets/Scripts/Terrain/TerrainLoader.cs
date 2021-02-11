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

	public int staticChunks = 4;
	public Material defaultMaterial;
	Vector3Int oldChunk = Vector3Int.zero;

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
			Debug.Log($"Moved to {viewChunk}");
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
		}


		//for (int x = 0; x < staticChunks; x++)
		//{
		//	for (int y = -staticChunks; y < staticChunks; y++)
		//	{
		//		for (int z = 0; z < staticChunks; z++)
		//		{
		//			Vector3Int p = new Vector3Int(x, y, z);

		//			if (loadedChunks.ContainsKey(p))
		//				continue;

		//			Chunk newChunk;
		//			if (unloadedChunks.Count > 0)
		//				newChunk = unloadedChunks.Dequeue();
		//			else
		//				newChunk = AddChunk();

		//			Vector3 chunkOffset = p * (volumeSize - Vector3Int.one * 2);
		//			newChunk.Refresh(p, Vector3.Scale(chunkOffset, worldScale));
		//			loadedChunks.Add(p, newChunk);
		//			chunks.Add(newChunk);
		//			contourGenerator.GenerateChunk(newChunk);
		//		}
		//	}
		//}
	}

	public void UpdateAll()
	{
		foreach (Chunk chunk in chunks)
			contourGenerator.GenerateChunk(chunk);
	}
}
