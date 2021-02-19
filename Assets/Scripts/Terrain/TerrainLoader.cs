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
	public int lodSize = 2048;
	public int lodRedrawDistance = 4;
	public int lodHoleSize = 4;

	[Header("World Settings")]
	public float volumeScale = 1.0f;
	public int volumeSize = 64;

	public Material defaultMaterial;
	public Terrain distantTerrain;

	Vector2Int prevLod = new Vector2Int(-999, -999);

	float LodWidth { get => lodSize; }
	float LodHeight { get => terrainGenerator.surfaceMagnitude; }

	CSGenerator terrainGenerator;
	CSContourGenerator contourGenerator;

	List<Chunk> chunks;
	Queue<Chunk> unloadedChunks;
	Dictionary<Vector3Int, Chunk> loadedChunks;

	bool[,] makeHoleStencil;
	bool[,] closeHoleStencil;

	Vector2Int lastStencil;

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

		// Ensure LOD fits into compute buffer
		int r = FloorPower2(Mathf.RoundToInt(Mathf.Sqrt(Mathf.Pow(volumeSize, 3))));

		distantTerrain.terrainData.size = new Vector3(w, h, w);
		distantTerrain.terrainData.heightmapResolution = r;
		distantTerrain.transform.Translate(new Vector3(-w / 2, 0, -w / 2));

		lastStencil = new Vector2Int(lodHoleSize, lodHoleSize);

		SetupStencils();
		ClearHoles();
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
					-0.1f, 
					viewChunk.y * scaledVolumeSize - w / 2);

			distantTerrain.transform.SetPositionAndRotation(terrainOffset, Quaternion.identity);
		}

		Vector3 lodRelPos = viewPos - distantTerrain.transform.position;

		int res = distantTerrain.terrainData.holesResolution;
		int xStencil = Mathf.RoundToInt(lodRelPos.x / lodSize * res) - lodHoleSize / 2;
		int yStencil = Mathf.RoundToInt(lodRelPos.z / lodSize * res) - lodHoleSize / 2;

		distantTerrain.terrainData.SetHoles(lastStencil.x, lastStencil.y, closeHoleStencil);
		distantTerrain.terrainData.SetHoles(xStencil, yStencil, makeHoleStencil);
		
		lastStencil.x = xStencil;
		lastStencil.y = yStencil;
	}

	void SetupStencils()
    {
		closeHoleStencil = new bool[lodHoleSize, lodHoleSize];
		makeHoleStencil = new bool[lodHoleSize, lodHoleSize];

		int halfSize = lodHoleSize / 2;
		var a = new Vector2Int(halfSize, halfSize);

		int sqrHole = halfSize * halfSize;

		for (int i = 0; i < lodHoleSize * lodHoleSize; i++)
		{
			int x = i % lodHoleSize;
			int y = i / lodHoleSize;

			var b = new Vector2Int(x, y);
			int dist = (b - a).sqrMagnitude;

			closeHoleStencil[x, y] = true;
			makeHoleStencil[x, y] = dist > sqrHole / 2;
		}
	}

	void ClearHoles()
    {
		int holes = distantTerrain.terrainData.holesResolution;
		bool[,] arr = new bool[holes, holes];

		for (int j = 0; j < holes * holes; j++)
		{
			int x = j % holes;
			int y = j / holes;
			arr[x, y] = true;
		}

		distantTerrain.terrainData.SetHoles(0, 0, arr);
	}

	void SetHole(Vector3 pos, bool isHole)
    {
		Vector3 lodRelPos = pos - distantTerrain.transform.position;

		int res = distantTerrain.terrainData.holesResolution;
		int xStencil = Mathf.RoundToInt(lodRelPos.x / lodSize * res) - lodHoleSize / 2;
		int yStencil = Mathf.RoundToInt(lodRelPos.z / lodSize * res) - lodHoleSize / 2;

		Debug.Log($"Hole at {lodRelPos} / {xStencil}, {yStencil} = {isHole}");

		distantTerrain.terrainData.SetHolesDelayLOD(xStencil, yStencil, isHole ? makeHoleStencil : closeHoleStencil);
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
