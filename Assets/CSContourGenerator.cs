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
	ComputeBuffer vertexBuffer;
	ComputeBuffer quadBuffer;
	ComputeBuffer quadCountBuffer;
	ComputeBuffer vertexCountBuffer;

	int _kernelDirect;
	int _kernelIndirect;
	uint _threadSizeX;
	uint _threadSizeY;
	uint _threadSizeZ;

	Array3<IsoPoint> iso;

    // Start is called before the first frame update
    void Start()
    {
		Setup();

		iso = new Array3<IsoPoint>(size);
		iso.ForEach3( (Vector3Int pos) => { iso[pos] = new IsoPoint(float.MaxValue, Vector3.zero); } );
		var center = (Vector3)size * 0.5f + new Vector3(0, 0, size.z) * 2.0f / 16.0f;
		float rad = size.x * 3.5f / 16.0f;

		Generator.AddSphere(iso, center, rad);

		SetupBuffers();
    	Generate(iso);    
		
    }

	//void OnEnable()
	//{
	//	SetupBuffers();
	//}

	void OnDestroy()
	{
		ReleaseBuffers();
	}

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
		int pointCount = size.x * size.y * size.z;
		int indexCount = voxelSize.x * voxelSize.y * voxelSize.z;
		int quadCount = indexCount * 3 * 2;

		ReleaseBuffers();

		// Create buffer for isosurface on shader !!! POSSIBLE PACK ISSUES !!!
		isoBuffer = new ComputeBuffer(pointCount, 4 * sizeof(float));
		indexBuffer = new ComputeBuffer(indexCount, sizeof(int));
		vertexBuffer = new ComputeBuffer(indexCount, 3 * sizeof(float), ComputeBufferType.Counter);
		quadBuffer = new ComputeBuffer(indexCount * 3, 2 * 3 * sizeof(int), ComputeBufferType.Append);
		quadCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
		vertexCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

		shader.SetBuffer(_kernelDirect, "iso", isoBuffer);
		shader.SetBuffer(_kernelDirect, "indices", indexBuffer);
		shader.SetBuffer(_kernelDirect, "vertices", vertexBuffer);
		shader.SetBuffer(_kernelDirect, "quads", quadBuffer);

		shader.SetBuffer(_kernelIndirect, "iso", isoBuffer);
		shader.SetBuffer(_kernelIndirect, "indices", indexBuffer);
		shader.SetBuffer(_kernelIndirect, "quads", quadBuffer);
	}

	void ReleaseBuffers()
	{
		if (isoBuffer != null)
			isoBuffer.Release();

		if (indexBuffer != null)
			indexBuffer.Release();

		if (vertexBuffer != null)
			vertexBuffer.Release();

		if (quadBuffer != null)
			quadBuffer.Release();

		if (quadCountBuffer != null)
			quadCountBuffer.Release();

		if (vertexCountBuffer != null)
			vertexCountBuffer.Release();

		isoBuffer = null;
		indexBuffer = null;
		vertexBuffer = null;
		quadBuffer = null;
		quadCountBuffer = null;
		vertexCountBuffer = null;
	}

	void Generate(Array3<IsoPoint> iso)
	{

		Vector3Int voxelSize = size - Vector3Int.one;
		int pointCount = size.x * size.y * size.z;
		int indexCount = voxelSize.x * voxelSize.y * voxelSize.z;

		var t0 = Time.realtimeSinceStartup;

		Vector3Int ts = new Vector3Int(
				Mathf.CeilToInt(size.x / _threadSizeX), 
				Mathf.CeilToInt(size.y / _threadSizeY), 
				Mathf.CeilToInt(size.z / _threadSizeZ));

		shader.SetInts("sizeAxes", new int[] { size.x, size.y, size.z });
		shader.SetFloat("maxCornerDistance", maxCornerDistance);
		shader.SetFloat("centerBias", centerBias);

		isoBuffer.SetData(iso.Data);

		quadBuffer.SetCounterValue(0);
		vertexBuffer.SetCounterValue(0);
		indexBuffer.SetCounterValue(0);

		shader.Dispatch(_kernelDirect, ts.x, ts.y, ts.z);
		shader.Dispatch(_kernelIndirect, ts.x, ts.y, ts.z);

		ComputeBuffer.CopyCount(quadBuffer, quadCountBuffer, 0);
		int[] quadCountArray = { 0 };
		quadCountBuffer.GetData(quadCountArray);
		int quadCount = quadCountArray[0];
		
		ComputeBuffer.CopyCount(vertexBuffer, vertexCountBuffer, 0);
		int[] vertexCountArray = { 0 };
		vertexCountBuffer.GetData(vertexCountArray);
		int vertexCount = vertexCountArray[0];

		Vector3[] vertices = new Vector3[vertexCount];
		int[] triangles = new int[2 * 3 * quadCount];

		quadBuffer.GetData(triangles);
		vertexBuffer.GetData(vertices);

		var t1 = Time.realtimeSinceStartup;
		Debug.Log(string.Format("CS: Created {0} vertices, {1} triangles, {2} indices in {3} seconds", 
			vertexCount, quadCount * 2, indexCount, t1 - t0));

		contour.vertices = vertices;
		contour.triangles = triangles;
		contour.RecalculateNormals();
	}
}
