using UnityEngine;

public class CSGenerator : MonoBehaviour
{

	public ComputeShader terrainShader;

	[Header("Terrain Settings")]
	public float noiseScale;
	public float noiseMagnitude;
	public Vector3 noiseOffset;

	[Header("Debug")]
	public Vector3 scroll;

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

	public void Generate(ComputeBuffer isoBuffer, Vector3Int size, Vector3Int chunk)
	{
		noiseOffset += scroll * Time.deltaTime;

		terrainShader.SetInts("isoSize", new int[] { size.x, size.y, size.z });	
		terrainShader.SetInts("chunkOffset", new int[] { chunk.x, chunk.y, chunk.z });

		terrainShader.SetFloat("noiseScale", noiseScale);
		terrainShader.SetFloat("noiseMagnitude", noiseMagnitude);
		terrainShader.SetFloats("noiseOffset", new float[] { noiseOffset.x, noiseOffset.y, noiseOffset.z });

		terrainShader.SetBuffer(_generatorKernel, "iso", isoBuffer);

		Vector3Int ts = new Vector3Int(
				Mathf.CeilToInt(size.x / _threadSizeX), 
				Mathf.CeilToInt(size.y / _threadSizeY), 
				Mathf.CeilToInt(size.z / _threadSizeZ));

		terrainShader.Dispatch(_generatorKernel, ts.x, ts.y, ts.z);
	}
}
