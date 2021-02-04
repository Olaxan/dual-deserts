using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class CSContourGenerator : MonoBehaviour
{

	public ComputeShader shader;

	[Header("Voxel Settings")]
	public Vector3Int size;
	public float centerBias;
	public float maxCornerDistance;

	Mesh contour;
	MeshFilter meshFilter;

	ComputeBuffer isoBuffer;
	ComputeBuffer triangleBuffer;
	ComputeBuffer argsBuffer;
	int _kernelDirect;
	int _kernelIndirect;
	int[] args;
	uint _threadSizeX;
	uint _threadSizeY;
	uint _threadSizeZ;

    // Start is called before the first frame update
    void Start()
    {
		Setup();
		SetupBuffers();
    	Generate();    
		ReleaseBuffers();
    }

	//void OnEnable()
	//{
	//	SetupBuffers();
	//}

	void Setup()
	{
		meshFilter = gameObject.GetComponent<MeshFilter>();
		if (meshFilter == null)
			meshFilter = gameObject.AddComponent<MeshFilter>();

		contour = meshFilter.sharedMesh;

		if (contour == null)
		{
			contour = new Mesh();
			contour.name = "Contour";
			meshFilter.sharedMesh = contour;
		}

		_kernelDirect = shader.FindKernel("CSMakeVerticesDirect");
		_kernelIndirect = shader.FindKernel("CSMakeTrianglesIndirect");

		shader.GetKernelThreadGroupSizes(_kernelDirect, 
				out _threadSizeX, out _threadSizeY, out _threadSizeZ);
	}

	void SetupBuffers()
	{
		Vector3Int voxelSize = size - Vector3Int.one;
		int voxelCount = voxelSize.x * voxelSize.y * voxelSize.z;
		int triCount = voxelCount * 3 * 2;

		ReleaseBuffers();

		// Create buffer for isosurface on shader !!! POSSIBLE PACK ISSUES !!!
		isoBuffer = new ComputeBuffer(voxelCount, 4 + 3 * 4);

		// Provide thread arguments for indirect execution
		argsBuffer = new ComputeBuffer(1, 16, ComputeBufferType.IndirectArguments);
		args = new int[4];
		args[0] = (int)_threadSizeX;
		args[1] = (int)_threadSizeY;
		args[2] = (int)_threadSizeZ;
		args[3] = 0;
		argsBuffer.SetData(args);

	}

	void ReleaseBuffers()
	{
		if (triangleBuffer != null)
			triangleBuffer.Release();

		if (isoBuffer != null)
			isoBuffer.Release();

		triangleBuffer = null;
		isoBuffer = null;
	}

	void Generate()
	{
		shader.SetInts("sizeAxes", new int[] { size.x, size.y, size.z });
		shader.Dispatch(_kernelDirect, 
				Mathf.CeilToInt(size.x / _threadSizeX), 
				Mathf.CeilToInt(size.y / _threadSizeY), 
				Mathf.CeilToInt(size.z / _threadSizeZ)
				);

	}
}
