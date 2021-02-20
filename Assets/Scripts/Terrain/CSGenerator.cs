using UnityEngine;

public class CSGenerator : MonoBehaviour
{

	public ComputeShader terrainShader;

	[Header("Terrain Settings")]
	public float surfaceLevel;
	public float surfaceScale;
	public float surfaceMagnitude;

	public float warpScale = 0.004f;
	public float warpMagnitude = 8;

	public Vector3 noiseOffset;
	public Vector3 noiseScale;

	public float derivativeStep = 0.0001f;

	Matrix4x4 rot1;
	Matrix4x4 rot2;
	Matrix4x4 rot3;
	Matrix4x4 rot4;

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

		rot1 = Matrix4x4.Rotate(Random.rotation);
		rot2 = Matrix4x4.Rotate(Random.rotation);
		rot3 = Matrix4x4.Rotate(Random.rotation);
		rot4 = Matrix4x4.Rotate(Random.rotation);
	}

	void SetUniforms(int size, float scale)
    {
		terrainShader.SetInt("isoSize", size);
		terrainShader.SetFloat("isoScale", scale);

		terrainShader.SetMatrix("octaveMat0", rot1);
		terrainShader.SetMatrix("octaveMat1", rot2);
		terrainShader.SetMatrix("octaveMat2", rot3);
		terrainShader.SetMatrix("octaveMat3", rot4);

		terrainShader.SetFloat("surfaceLevel", surfaceLevel);
		terrainShader.SetFloat("surfaceScale", surfaceScale);
		terrainShader.SetFloat("surfaceMagnitude", surfaceMagnitude);
		terrainShader.SetFloat("warpScale", warpScale);
		terrainShader.SetFloat("warpMagnitude", warpMagnitude);
		terrainShader.SetFloat("derivativeStep", derivativeStep);
		terrainShader.SetFloats("noiseOffset", new float[] { noiseOffset.x, noiseOffset.y, noiseOffset.z });
		terrainShader.SetFloats("noiseScale", new float[] { noiseScale.x, noiseScale.y, noiseScale.z });
	}

	public void Generate(ComputeBuffer isoDists, ComputeBuffer isoNormals, Vector3Int chunk, int isoSize, float isoScale)
	{
		SetUniforms(isoSize, isoScale);

		Vector3Int ts = new Vector3Int(
				Mathf.CeilToInt(isoSize / _threadSizeX), 
				Mathf.CeilToInt(isoSize / _threadSizeY), 
				Mathf.CeilToInt(isoSize / _threadSizeZ));

		terrainShader.SetInts("chunkOffset", new int[] { chunk.x, chunk.y, chunk.z });

		terrainShader.SetBuffer(_generatorKernel, "isoDists", isoDists);
		terrainShader.SetBuffer(_generatorKernel, "isoNormals", isoNormals);

		terrainShader.Dispatch(_generatorKernel, ts.x, ts.y, ts.z);
	}

	public void GenerateSurface(ComputeBuffer isoDists, Vector2Int chunk, int lodSize, int lodRes, int isoSize, float isoScale)
	{
		SetUniforms(isoSize, isoScale);

		int ts = Mathf.CeilToInt(lodRes / _threadSizeX);

		terrainShader.SetInts("chunkOffset", new int[] { chunk.x, 0, chunk.y });
		terrainShader.SetInt("isoSize", isoSize);
		terrainShader.SetInt("lodSize", lodSize);
		terrainShader.SetInt("lodRes", lodRes);
		terrainShader.SetInt("lodChunksPerAxis", lodSize / isoSize);

		terrainShader.SetBuffer(_surfaceGeneratorKernel, "isoDists", isoDists);

		terrainShader.Dispatch(_surfaceGeneratorKernel, ts, ts, 1);
	}
}
