using UnityEngine;

public class CSGenerator : MonoBehaviour
{

	public ComputeShader terrainShader;

	[Header("Terrain Settings")]
	public float surfaceLevel;
	public float surfaceScale;
	public float surfaceMagnitude;
	public float surfaceDistanceMult;

	public float caveScale;
	public float caveDistanceMult;

	public Vector3 noiseOffset;
	public Vector3 noiseScale;

	int _generatorKernel;
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
		terrainShader.GetKernelThreadGroupSizes(_generatorKernel,
				out _threadSizeX, out _threadSizeY, out _threadSizeZ);
	}

	public ComputeBuffer CreateIsoBuffer(Vector3Int size)
	{
		return new ComputeBuffer(size.x * size.y * size.z, 4 * sizeof(float));
	}

	public void Generate(ComputeBuffer isoBuffer, Vector3Int size, Vector3Int chunk)
	{
		terrainShader.SetInts("isoSize", new int[] { size.x, size.y, size.z });	
		terrainShader.SetInts("chunkOffset", new int[] { chunk.x, chunk.y, chunk.z });

		terrainShader.SetFloat("surfaceLevel", surfaceLevel);
		terrainShader.SetFloat("surfaceScale", surfaceScale);
		terrainShader.SetFloat("surfaceMagnitude", surfaceMagnitude);
		terrainShader.SetFloat("surfaceDistanceMult", surfaceDistanceMult);

		terrainShader.SetFloat("caveScale", caveScale);
		terrainShader.SetFloat("caveDistanceMult", caveDistanceMult);

		terrainShader.SetFloats("noiseScale", new float[] { noiseScale.x, noiseScale.y, noiseScale.z });
		terrainShader.SetFloats("noiseOffset", new float[] { noiseOffset.x, noiseOffset.y, noiseOffset.z });

		terrainShader.SetBuffer(_generatorKernel, "iso", isoBuffer);

		Vector3Int ts = new Vector3Int(
				Mathf.CeilToInt(size.x / _threadSizeX), 
				Mathf.CeilToInt(size.y / _threadSizeY), 
				Mathf.CeilToInt(size.z / _threadSizeZ));

		terrainShader.Dispatch(_generatorKernel, ts.x, ts.y, ts.z);
	}
}
