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
	ComputeBuffer indexBuffer;
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

		var iso = new Array3<IsoPoint>(size);
		iso.ForEach3( (Vector3Int pos) => { iso[pos] = new IsoPoint(float.MaxValue, Vector3.zero); } );

		var center = (Vector3)size * 0.5f + new Vector3(0, 0, size.z) * 2.0f / 16.0f;
		float rad = size.x * 3.5f / 16.0f;

		Generator.AddSphere(iso, center, rad);
		Generator.RemoveCylinder(iso, new Vector2(center.x, center.y), rad / 3);

		SetupBuffers();
    	Generate(iso);    
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
		int pointCount = size.x * size.y * size.z;
		int triCount = voxelCount * 3 * 2;

		ReleaseBuffers();

		// Create buffer for isosurface on shader !!! POSSIBLE PACK ISSUES !!!
		isoBuffer = new ComputeBuffer(voxelCount, 4 * sizeof(float));

		// Provide thread arguments for indirect execution
		argsBuffer = new ComputeBuffer(1, 16, ComputeBufferType.IndirectArguments);
		args = new int[4];
		args[0] = (int)_threadSizeX;
		args[1] = (int)_threadSizeY;
		args[2] = (int)_threadSizeZ;
		args[3] = 0;
		argsBuffer.SetData(args);

		indexBuffer = new ComputeBuffer(pointCount, sizeof(int), ComputeBufferType.Counter);

		triangleBuffer = new ComputeBuffer(voxelCount * 2, 3 * sizeof(int), ComputeBufferType.Append);
	}

	void ReleaseBuffers()
	{
		if (isoBuffer != null)
			isoBuffer.Release();

		if (indexBuffer != null)
			indexBuffer.Release();

		if (triangleBuffer != null)
			triangleBuffer.Release();

		if (argsBuffer != null)
			argsBuffer.Release();


		isoBuffer = null;
		indexBuffer = null;
		triangleBuffer = null;
		argsBuffer = null;
	}

	void Generate(Array3<IsoPoint> iso)
	{
		shader.SetInts("sizeAxes", new int[] { size.x, size.y, size.z });
		shader.SetFloat("maxCornerDistance", maxCornerDistance);
		shader.SetFloat("centerBias", centerBias);

		isoBuffer.SetData(iso.Data);

		triangleBuffer.SetCounterValue(0);
		indexBuffer.SetCounterValue(0);

		//shader.Dispatch(_kernelDirect, 
		//		Mathf.CeilToInt(size.x / _threadSizeX), 
		//		Mathf.CeilToInt(size.y / _threadSizeY), 
		//		Mathf.CeilToInt(size.z / _threadSizeZ)
		//		);

	}
}
