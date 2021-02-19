﻿using UnityEngine;

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

	void SetUniforms(int size, float scale)
    {
		terrainShader.SetInt("isoSize", size);
		terrainShader.SetFloat("isoScale", scale);

		terrainShader.SetFloat("surfaceLevel", surfaceLevel);
		terrainShader.SetFloat("surfaceScale", surfaceScale);
		terrainShader.SetFloat("surfaceMagnitude", surfaceMagnitude);
		terrainShader.SetFloat("caveScale", caveScale);
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
