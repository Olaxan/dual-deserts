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

	ComputeBuffer triangleBuffer;
	ComputeBuffer isoBuffer;

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
	}

	void SetupBuffers()
	{
		Vector3Int voxelSize = size - Vector3Int.one;
		int voxelCount = voxelSize.x * voxelSize.y * voxelSize.z;
		int triCount = voxelCount * 3 * 2;

		ReleaseBuffers();
		//triangleBuffer(triCount, 
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
	}
}
