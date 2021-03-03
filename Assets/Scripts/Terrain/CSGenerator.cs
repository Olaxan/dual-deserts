using System.Collections.Generic;
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
	public int csgOperationLimit = 256;

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

		csgBuffer = new ComputeBuffer(csgOperationLimit, 24);
		terrainShader.SetBuffer(_generatorKernel, "operations", csgBuffer);
	}

	public void SetUniforms()
    {
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

	public void SetBuffers(ComputeBuffer isoDists, ComputeBuffer isoNormals)
	{
		terrainShader.SetBuffer(_generatorKernel, "isoDists", isoDists);
		terrainShader.SetBuffer(_generatorKernel, "isoNormals", isoNormals);
	}

	public void SetOperations(List<CSG> operations)
	{
		int c = Mathf.Min(csgOperationLimit, operations.Count);

		csgBuffer.SetData(operations, 0, 0, c);
		terrainShader.SetInt("opCount", c);
	}

	public void Generate(Vector3 chunk, int isoSize, float isoScale)
	{

		SetUniforms();

		Vector3Int ts = new Vector3Int(
				Mathf.CeilToInt(isoSize / _threadSizeX), 
				Mathf.CeilToInt(isoSize / _threadSizeY), 
				Mathf.CeilToInt(isoSize / _threadSizeZ));

		terrainShader.SetInt("isoSize", isoSize);
		terrainShader.SetFloat("isoScale", isoScale);
		terrainShader.SetFloats("chunkOffset", new float[] { chunk.x, chunk.y, chunk.z });

		terrainShader.Dispatch(_generatorKernel, ts.x, ts.y, ts.z);
	}
}
