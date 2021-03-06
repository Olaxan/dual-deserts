using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ContourGeneratorScript : MonoBehaviour
{

	public Vector3Int size;
	public float maxCornerDistance;
	public float pushSize;

	CSGenerator terrainGenerator;

	Mesh contour;
	MeshFilter meshFilter;
	MeshRenderer meshRenderer;
	MeshCollider meshCollider;

	class VoxelMesh
	{
		public VoxelMesh(Vector3Int size)
		{
			int count = size.x * size.y * size.z;
			vertices = new List<Vector3>(count);
			triangles = new List<int>(count);
			voxels = new Array3<int>(size - new Vector3Int(1, 1, 1));
			voxels.Fill(-1);
		}

		public List<Vector3> vertices;
		public List<int> triangles;
		public Array3<int> voxels;
	};

	static Vector3Int[] corners =  
	{
		new Vector3Int(0,0,0), 
		new	Vector3Int(0,0,1), 
		new	Vector3Int(0,1,0), 
		new	Vector3Int(0,1,1),
		new	Vector3Int(1,0,0), 
		new	Vector3Int(1,0,1), 
		new	Vector3Int(1,1,0), 
		new	Vector3Int(1,1,1)
	};

	static Vector3[] axes = 
	{
		new Vector3(1,0,0),
		new Vector3(0,1,0),
		new Vector3(0,0,1)
	};

	static (int, int)[] edges = 
	{
		(0,1),
		(0,2),
		(0,4),
		(1,3),
		(1,5),
		(2,3),
		(2,6),
		(3,7),
		(4,5),
		(4,6),
		(5,7),
		(6,7),
	};

	static int numCorners = 8;

    // Start is called before the first frame update
    void Start()
    {

		Setup();

		var iso = new Array3<IsoPoint>(size);
		var mesh = new VoxelMesh(size);

		//var buf = terrainGenerator.CreateIsoBuffer(size);
		//terrainGenerator.Generate(buf, Vector3Int.zero, size, Vector3.one);
		//IsoPoint[] isoArr = new IsoPoint[size.x * size.y * size.z];
		//buf.GetData(isoArr);
		//iso.Data = isoArr;
		//buf.Release();

		Debug.Log(iso.Count);

		var t0 = Time.realtimeSinceStartup;

		BuildVertices(iso, mesh);
		BuildTriangles(iso, mesh);

		var t1 = Time.realtimeSinceStartup;

		Debug.Log(string.Format("Created {0} vertices, {1} triangles, {2} indices in {3} seconds", 
					mesh.vertices.Count, mesh.triangles.Count / 3, mesh.voxels.Count, t1 - t0)); 

		contour.vertices = mesh.vertices.ToArray(); // fix -- we shouldn't need to convert
		contour.triangles = mesh.triangles.ToArray();

		contour.RecalculateNormals();
    }

	void Setup()
	{

		meshFilter = GetComponent<MeshFilter>();
		if (meshFilter == null)
			meshFilter = gameObject.AddComponent<MeshFilter>();

		meshRenderer = GetComponent<MeshRenderer>();
		if (meshRenderer == null)
			meshRenderer = gameObject.AddComponent<MeshRenderer>();

		terrainGenerator = gameObject.GetComponent<CSGenerator>();
		if (terrainGenerator == null)
			terrainGenerator = gameObject.AddComponent<CSGenerator>();

		//meshCollider = GetComponent<MeshCollider>();

		contour = meshFilter.sharedMesh;
		if (contour == null)
		{
			contour = new Mesh();
			contour.name = "Contour";
			contour.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
			meshFilter.sharedMesh = contour;
		}
	}

    // Update is called once per frame
    void Update()
    {
        
    }

	void BuildVertices(Array3<IsoPoint> iso, VoxelMesh mesh)
	{
		List<IsoPoint> points = new List<IsoPoint>();
		List<Vector3> normals = new List<Vector3>();
		List<float> dists = new List<float>();

		mesh.voxels.ForEach3( (Vector3Int pos) =>
		{
			bool[] inside = new bool[numCorners];
			int numInside = 0;

			for (int ci = 0; ci < numCorners; ci++)
			{
				inside[ci] = (iso[pos + corners[ci]].dist <= 0);
				if (inside[ci])
					numInside++;
			}

			if (numInside == 0 || numInside == numCorners)
				return;

			bool[] crossingCorners = new bool[numCorners];

			foreach (var edge in edges)
			{
				if (inside[edge.Item1] != inside[edge.Item2])
				{
					crossingCorners[edge.Item1] = true;
					crossingCorners[edge.Item2] = true;
				}
			}

			points.Clear();
			normals.Clear();
			dists.Clear();

			for (int ci = 0; ci < numCorners; ci++)
			{
				Vector3Int pos_next = pos + corners[ci];
				IsoPoint point = iso[pos_next];

				if (!crossingCorners[ci])
					continue;

				if (Mathf.Abs(point.dist) > maxCornerDistance)
					continue;

				var p = new IsoPoint(Vector3.Dot(point.normal, pos_next) - point.dist, point.normal);
				points.Add(p);
			}

			Vector3 voxelCenter = pos + new Vector3(0.5f, 0.5f, 0.5f);

			foreach (Vector3 axis in axes)
			{
				Vector3 n = pushSize * axis;
				points.Add(new IsoPoint(Vector3.Dot(n, voxelCenter), n));
			}

			foreach (IsoPoint p in points)
			{
				normals.Add(p.normal);
				dists.Add(p.dist);
			}

			Vector3 vertex;
			if (Solver.LeastSquares(normals, dists, out vertex))
			{
				vertex = new Vector3(
						Mathf.Clamp(vertex.x, pos.x, pos.x + 1),
						Mathf.Clamp(vertex.y, pos.y, pos.y + 1),
						Mathf.Clamp(vertex.z, pos.z, pos.z + 1)
					);
			}
			else
				vertex = voxelCenter;


			// clamp vertex within own cell

			mesh.voxels[pos] = mesh.vertices.Count;
			mesh.vertices.Add(vertex);

		});
	}

	static (int, int)[] farEdges = 
	{
		(3, 7), (5, 7), (6, 7)
	};

	// ugh
	static Vector3Int d0 = new Vector3Int(0,0,1);
	static Vector3Int d1 = new Vector3Int(0,1,0);
	static Vector3Int d2 = new Vector3Int(0,1,1);
	static Vector3Int d3 = new Vector3Int(1,0,0);
	static Vector3Int d4 = new Vector3Int(1,0,1);
	static Vector3Int d5 = new Vector3Int(1,1,0);

	void BuildTriangles(Array3<IsoPoint> iso, VoxelMesh mesh)
	{

		mesh.voxels.ForEach3(mesh.voxels.Size - Vector3Int.one, (Vector3Int pos) => 
		{
			var v0 = mesh.voxels[pos];

			if (v0 < 0)
				return;

			var inside = new bool[numCorners];
			
			for (int ci = 0; ci < numCorners; ci++)
				inside[ci] = (iso[pos + corners[ci]].dist <= 0);

			for (int ai = 0; ai < 3; ai++)
			{
				var edge = farEdges[ai];

				if (inside[edge.Item1] == inside[edge.Item2])
					continue;

				int v1, v2, v3;

				switch (ai)
				{
					case 0:
						v1 = mesh.voxels[pos + d0];
						v2 = mesh.voxels[pos + d1];
						v3 = mesh.voxels[pos + d2];
						break;
					case 1:
						v1 = mesh.voxels[pos + d0];
						v2 = mesh.voxels[pos + d3];
						v3 = mesh.voxels[pos + d4];
						break;
					default:
						v1 = mesh.voxels[pos + d1];
						v2 = mesh.voxels[pos + d3];
						v3 = mesh.voxels[pos + d5];
						break;
				}

				if (v1 < 0 || v2 < 0 || v3 < 0)
				{
					Debug.LogWarning("Incorrect index data in model");
					continue;
				}

				if (inside[edge.Item1] == (ai == 1))
					mesh.triangles.AddRange(new int[] { v0, v1, v3, v0, v3, v2 });
				else
					mesh.triangles.AddRange(new int[] { v0, v3, v1, v0, v2, v3 });
			}
		});
	}
}
