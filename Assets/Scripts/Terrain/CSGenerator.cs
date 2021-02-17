using UnityEngine;

public class CSGenerator : MonoBehaviour
{

	public ComputeShader terrainShader;

	[Header("Terrain Settings")]
	public float surfaceLevel;
	public float surfaceScale;
	public float surfaceMagnitude;

	public float caveScale;

	public Vector3 noiseOffset;
	public Vector3 noiseScale;

	public float derivativeStep = 0.0001f;

	int _generatorKernel, _surfaceGeneratorKernel;
	uint _threadSizeX;
	uint _threadSizeY;
	uint _threadSizeZ;

    void Awake()
    {
      	Setup();  
    }

	void Setup()
	{
		_generatorKernel = terrainShader.FindKernel("CSGenerator");
		_surfaceGeneratorKernel = terrainShader.FindKernel("CSSurfaceGenerator");

		terrainShader.GetKernelThreadGroupSizes(_generatorKernel,
				out _threadSizeX, out _threadSizeY, out _threadSizeZ);
	}

	void SetUniforms()
    {
		terrainShader.SetFloat("surfaceLevel", surfaceLevel);
		terrainShader.SetFloat("surfaceScale", surfaceScale);
		terrainShader.SetFloat("surfaceMagnitude", surfaceMagnitude);
		terrainShader.SetFloat("caveScale", caveScale);
		terrainShader.SetFloat("derivativeStep", derivativeStep);
		terrainShader.SetFloats("noiseOffset", new float[] { noiseOffset.x, noiseOffset.y, noiseOffset.z });
		terrainShader.SetFloats("noiseScale", new float[] { noiseScale.x, noiseScale.y, noiseScale.z });
	}

	public void Generate(ComputeBuffer isoDists, ComputeBuffer isoNormals, Vector3Int chunk, Vector3Int size)
	{
		SetUniforms();

		Vector3Int ts = new Vector3Int(
				Mathf.CeilToInt(size.x / _threadSizeX), 
				Mathf.CeilToInt(size.y / _threadSizeY), 
				Mathf.CeilToInt(size.z / _threadSizeZ));

		terrainShader.SetInts("chunkOffset", new int[] { chunk.x, chunk.y, chunk.z });
		terrainShader.SetInts("isoSize", new int[] { size.x, size.y, size.z });

		terrainShader.SetBuffer(_generatorKernel, "isoDists", isoDists);
		terrainShader.SetBuffer(_generatorKernel, "isoNormals", isoNormals);

		terrainShader.Dispatch(_generatorKernel, ts.x, ts.y, ts.z);
	}

	public void GenerateSurface(ComputeBuffer isoDists, Vector2Int chunk, int size, int lodChunks)
	{
		SetUniforms();

		int ts = Mathf.CeilToInt(size / _threadSizeX);

		terrainShader.SetInts("chunkOffset", new int[] { chunk.x, 0, chunk.y });
		terrainShader.SetInts("isoSize", new int[] { size, 1, size });
		terrainShader.SetInt("lodChunks", lodChunks);

		terrainShader.SetBuffer(_surfaceGeneratorKernel, "isoDists", isoDists);

		terrainShader.Dispatch(_surfaceGeneratorKernel, ts, ts, 1);
	}
}
