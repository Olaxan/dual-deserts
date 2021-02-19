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
	public int lodChunks = 32;
	public int lodRedrawDistance = 4;

	[Header("World Settings")]
	public float volumeScale = 1.0f;
	public int volumeSize = 64;

	public Material defaultMaterial;
	public Terrain distantTerrain;

	Vector2Int prevLod = new Vector2Int(-999, -999);

	float LodWidth { get => lodChunks * volumeSize * volumeScale * 2; }
	float LodHeight { get => terrainGenerator.surfaceMagnitude * volumeScale * 2; }

	CSGenerator terrainGenerator;
	CSContourGenerator contourGenerator;

	List<Chunk> chunks;
	Queue<Chunk> unloadedChunks;
	Dictionary<Vector3Int, Chunk> loadedChunks;

	int updateCounter = 0;

	public static int CeilPower2(int x)
	{
		if (x < 2)
		{
			return 1;
		}
		return (int)Mathf.Pow(2, (int)Mathf.Log(x - 1, 2) + 1);
	}

	public static int FloorPower2(int x)
	{
		if (x < 1)
		{
			return 1;
		}
		return (int)Mathf.Pow(2, (int)Mathf.Log(x, 2));
	}

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
			UpdateDistantTerrain();
			updateCounter = 0;
        }
	}

	void Setup()
	{
		chunks = new List<Chunk>();
		unloadedChunks = new Queue<Chunk>();
		loadedChunks = new Dictionary<Vector3Int, Chunk>();

		contourGenerator = gameObject.GetComponent<CSContourGenerator>();
		terrainGenerator = gameObject.GetComponent<CSGenerator>();

		contourGenerator.Setup(volumeSize, volumeScale);

		float w = LodWidth;
		float h = LodHeight;
		int r = FloorPower2(Mathf.RoundToInt(Mathf.Sqrt(Mathf.Pow(volumeSize, 3))));

		Debug.Log(r);

		distantTerrain.terrainData.size = new Vector3(w, h, w);
		distantTerrain.terrainData.heightmapResolution = r;
		distantTerrain.transform.Translate(new Vector3(-w / 2, 0, -w / 2));
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

	public void UpdateDistantTerrain(bool force = false)
	{
		Vector3 viewPos = viewer.position;
		int adjustedVolumeSize = volumeSize - 2;
		float scaledVolumeSize = adjustedVolumeSize * volumeScale;

		Vector2Int viewChunk = new Vector2Int(
				Mathf.RoundToInt(viewPos.x / scaledVolumeSize),
				Mathf.RoundToInt(viewPos.z / scaledVolumeSize));

		int lodRedrawSqr = lodRedrawDistance * lodRedrawDistance;

		if ((viewChunk - prevLod).sqrMagnitude >= lodRedrawSqr || force)
        {
			prevLod = viewChunk;

			float w = LodWidth;

			contourGenerator.SurfaceRemesh(distantTerrain.terrainData, viewChunk);

			Vector3 terrainOffset = new Vector3(
					viewChunk.x * scaledVolumeSize - w / 2, 
					0, 
					viewChunk.y * scaledVolumeSize - w / 2);

			distantTerrain.transform.SetPositionAndRotation(terrainOffset, Quaternion.identity);
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
