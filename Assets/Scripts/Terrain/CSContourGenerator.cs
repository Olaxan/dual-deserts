using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(CSGenerator))]
public class CSContourGenerator : MonoBehaviour
{

	public ComputeShader contourGenerator;

	[Header("Voxel Settings")]
	public Vector3Int size;
	public float centerBias;
	public float maxCornerDistance;
	public float clampRange;

	[Header("Debug")]
	public bool autoUpdate;

	Mesh contour;
	MeshFilter meshFilter;

	CSGenerator terrainGenerator;

	ComputeBuffer isoBuffer;
	ComputeBuffer indexBuffer;
	ComputeBuffer vertexBuffer;
	ComputeBuffer quadBuffer;
	ComputeBuffer quadCountBuffer;
	ComputeBuffer vertexCountBuffer;

	int _vertexKernel;
	int _triangleKernel;
	uint _threadSizeX;
	uint _threadSizeY;
	uint _threadSizeZ;
	Vector3Int bufferSize;

	Vector3Int VoxelSize { get => size - Vector3Int.one; }

    void Awake()
    {
		Setup();
		SetupBuffers();
    }

	void Start()
	{
    	GenerateChunk();    
	}

	void Update()
    {
		if (autoUpdate)
			GenerateChunk();
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

		terrainGenerator = gameObject.GetComponent<CSGenerator>();
		if (terrainGenerator == null)
			terrainGenerator = gameObject.AddComponent<CSGenerator>();

		contour = meshFilter.sharedMesh;

		if (contour == null)
		{
			contour = new Mesh();
			contour.name = "Contour";
			meshFilter.sharedMesh = contour;
		}

		_vertexKernel = contourGenerator.FindKernel("CSMakeVertices");
		_triangleKernel = contourGenerator.FindKernel("CSMakeTriangles");

		contourGenerator.GetKernelThreadGroupSizes(_vertexKernel, 
				out _threadSizeX, out _threadSizeY, out _threadSizeZ);

	}

	void SetupBuffers()
	{
		Vector3Int voxelSize = size - Vector3Int.one;
		int pointCount = size.x * size.y * size.z;
		int indexCount = voxelSize.x * voxelSize.y * voxelSize.z;
		int quadCount = indexCount * 3 * 2;

		bufferSize = size;

		ReleaseBuffers();

		isoBuffer = new ComputeBuffer(pointCount, 4 * sizeof(float));
		indexBuffer = new ComputeBuffer(indexCount, sizeof(int));
		vertexBuffer = new ComputeBuffer(indexCount, 3 * sizeof(float), ComputeBufferType.Counter);
		quadBuffer = new ComputeBuffer(indexCount * 3, 2 * 3 * sizeof(int), ComputeBufferType.Append);
		quadCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
		vertexCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

		contourGenerator.SetBuffer(_vertexKernel, "iso", isoBuffer);
		contourGenerator.SetBuffer(_vertexKernel, "indices", indexBuffer);
		contourGenerator.SetBuffer(_vertexKernel, "vertices", vertexBuffer);
		contourGenerator.SetBuffer(_vertexKernel, "quads", quadBuffer);

		contourGenerator.SetBuffer(_triangleKernel, "iso", isoBuffer);
		contourGenerator.SetBuffer(_triangleKernel, "indices", indexBuffer);
		contourGenerator.SetBuffer(_triangleKernel, "quads", quadBuffer);
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

	public void GenerateChunk()
	{

		if (bufferSize != size)
			SetupBuffers();

		int pointCount = size.x * size.y * size.z;
		int indexCount = VoxelSize.x * VoxelSize.y * VoxelSize.z;

		Vector3Int ts = new Vector3Int(
				Mathf.CeilToInt(size.x / _threadSizeX), 
				Mathf.CeilToInt(size.y / _threadSizeY), 
				Mathf.CeilToInt(size.z / _threadSizeZ));

		terrainGenerator.Generate(isoBuffer, size, new Vector3Int(0,0,0));

		contourGenerator.SetInts("isoSize", new int[] { size.x, size.y, size.z });
		contourGenerator.SetFloat("maxCornerDistance", maxCornerDistance);
		contourGenerator.SetFloat("centerBias", centerBias);
		contourGenerator.SetFloat("clampRange", clampRange);

		quadBuffer.SetCounterValue(0);
		vertexBuffer.SetCounterValue(0);
		indexBuffer.SetCounterValue(0);

		contourGenerator.Dispatch(_vertexKernel, ts.x, ts.y, ts.z);
		contourGenerator.Dispatch(_triangleKernel, ts.x, ts.y, ts.z);

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

		contour.Clear();
		contour.vertices = vertices;
		contour.triangles = triangles;
		contour.RecalculateNormals();
		contour.RecalculateTangents();
	}
}
