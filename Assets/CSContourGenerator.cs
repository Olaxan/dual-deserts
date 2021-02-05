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
		int pointCount = size.x * size.y * size.z;
		int indexCount = voxelSize.x * voxelSize.y * voxelSize.z;
		int quadCount = indexCount * 3 * 2;

		ReleaseBuffers();

		// Create buffer for isosurface on shader !!! POSSIBLE PACK ISSUES !!!
		isoBuffer = new ComputeBuffer(pointCount, 4 * sizeof(float));
		indexBuffer = new ComputeBuffer(indexCount, sizeof(int), ComputeBufferType.Counter);
		vertexBuffer = new ComputeBuffer(indexCount, 3 * sizeof(float), ComputeBufferType.Append);
		quadBuffer = new ComputeBuffer(indexCount, 2 * 3 * sizeof(int), ComputeBufferType.Append);
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

		Vector3Int ts = new Vector3Int(
				Mathf.CeilToInt(size.x / _threadSizeX - 1), 
				Mathf.CeilToInt(size.y / _threadSizeY - 1), 
				Mathf.CeilToInt(size.z / _threadSizeZ - 1));

		shader.SetInts("sizeAxes", new int[] { size.x, size.y, size.z });
		shader.SetFloat("maxCornerDistance", maxCornerDistance);
		shader.SetFloat("centerBias", centerBias);

		isoBuffer.SetData(iso.Data);

		quadBuffer.SetCounterValue(0);
		vertexBuffer.SetCounterValue(0);
		indexBuffer.SetCounterValue(0);

		shader.Dispatch(_kernelDirect, ts.x, ts.y, ts.z);
		shader.Dispatch(_kernelIndirect, ts.x, ts.y, ts.z);

		Vector3[] vertices = new Vector3[vertexBuffer.count];

		ComputeBuffer.CopyCount(quadBuffer, quadCountBuffer, 0);
		int[] quadCountArray = { 0 };
		quadCountBuffer.GetData(quadCountArray);
		int quadCount = quadCountArray[0];
		
		ComputeBuffer.CopyCount(vertexBuffer, vertexCountBuffer, 0);
		int[] vertexCountArray = { 0 };
		vertexCountBuffer.GetData(vertexCountArray);
		int vertexCount = vertexCountArray[0];

		int[] triangles = new int[2 * 3 * quadCount];
		Vector3[] verts = new Vector3[vertexCount];

		Debug.Log(string.Format("CS: Created {0} vertices, {1} triangles", vertexCount, quadCount / (3 * 2)));

		quadBuffer.GetData(triangles);
		vertexBuffer.GetData(verts);

		float chkSum = 0;
		for (int i = 0; i < vertexCount; i++)
			chkSum += (verts[i].x + verts[i].y + verts[i].z);	

		Debug.Log(string.Format("CS: Checksum: {0}", chkSum));

		//contour.vertices = verts;
		//contour.triangles = triangles;
		//contour.RecalculateNormals();
	}
}
