using UnityEngine;

public class CSGenerator : MonoBehaviour
{

	public ComputeShader terrainShader;

	[Header("Terrain Settings")]
	public float surfaceLevel;
	public float surfaceScale;
	public float surfaceMagnitude;

	public float warpScale;
	public float warpMagnitude;
	
	public float caveScale;
	public float caveMagnitude;

	public Vector3 noiseOffset = Vector3.zero;
	public Vector3 noiseScale = Vector3.one;

	public float derivativeStep = 0.0001f;

	[Header("Deformation Settings")]
	public int chunkOperationLimit = 256;

	Matrix4x4 rot1;
	Matrix4x4 rot2;
	Matrix4x4 rot3;
	Matrix4x4 rot4;

	int _generatorKernel, _surfaceGeneratorKernel;
	uint _threadSizeX;
	uint _threadSizeY;
	uint _threadSizeZ;

	ComputeBuffer csgBuffer;

    void Awake()
    {
      	Setup();  
    }

	void OnDestroy()
	{
		csgBuffer.Release();
	}

	void Setup()
	{
		_generatorKernel = terrainShader.FindKernel("CSGenerator");

		terrainShader.GetKernelThreadGroupSizes(_generatorKernel,
				out _threadSizeX, out _threadSizeY, out _threadSizeZ);

		rot1 = Matrix4x4.Rotate(Random.rotation);
		rot2 = Matrix4x4.Rotate(Random.rotation);
		rot3 = Matrix4x4.Rotate(Random.rotation);
		rot4 = Matrix4x4.Rotate(Random.rotation);

		csgBuffer = new ComputeBuffer(chunkOperationLimit, 24);
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
		terrainShader.SetFloat("caveScale", caveScale);
		terrainShader.SetFloat("caveMagnitude", caveMagnitude);
		terrainShader.SetFloat("derivativeStep", derivativeStep);
		terrainShader.SetFloats("noiseOffset", new float[] { noiseOffset.x, noiseOffset.y, noiseOffset.z });
		terrainShader.SetFloats("noiseScale", new float[] { noiseScale.x, noiseScale.y, noiseScale.z });
	}

	public void Generate(ComputeBuffer isoDists, ComputeBuffer isoNormals, Vector3 chunk, int isoSize, float isoScale)
	{

		SetUniforms(isoSize, isoScale);

		Vector3Int ts = new Vector3Int(
				Mathf.CeilToInt(isoSize / _threadSizeX), 
				Mathf.CeilToInt(isoSize / _threadSizeY), 
				Mathf.CeilToInt(isoSize / _threadSizeZ));

		terrainShader.SetFloats("chunkOffset", new float[] { chunk.x, chunk.y, chunk.z });

		terrainShader.SetBuffer(_generatorKernel, "isoDists", isoDists);
		terrainShader.SetBuffer(_generatorKernel, "isoNormals", isoNormals);
		terrainShader.SetBuffer(_generatorKernel, "operations", csgBuffer);

		CSG[] ops = new CSG[2];
		ops[0] = new CSG(new Vector3(-15, 250,-20), 10f, OpShape.Sphere, OpType.Union);
		ops[1] = new CSG(new Vector3(-15, 250,-10), 10f, OpShape.Sphere, OpType.Subtraction);

		terrainShader.SetInt("opCount", 2);
		csgBuffer.SetData(ops, 0, 0, 2);

		terrainShader.Dispatch(_generatorKernel, ts.x, ts.y, ts.z);
	}
}
