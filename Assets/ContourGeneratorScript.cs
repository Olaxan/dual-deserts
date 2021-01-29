using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class ContourGeneratorScript : MonoBehaviour
{

	public Vector3Int size;
	public float maxCornerDistance;
	public float pushSize;

	Mesh contour;
	MeshFilter meshFilter;
	MeshRenderer meshRenderer;
	MeshCollider meshCollider;

	Array3<IsoPoint> iso;
	Array3<int> indices;

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

	static Vector2Int[] edges = 
	{
		new Vector2Int(0,1),
		new Vector2Int(0,2),
		new Vector2Int(0,4),
		new Vector2Int(1,3),
		new Vector2Int(1,5),
		new Vector2Int(2,3),
		new Vector2Int(2,6),
		new Vector2Int(3,7),
		new Vector2Int(4,5),
		new Vector2Int(4,6),
		new Vector2Int(5,7),
		new Vector2Int(6,7),
	};

	static int numCorners = 8;
	static int numEdges = 3 * 4;

    // Start is called before the first frame update
    void Start()
    {
		contour = new Mesh();
		meshFilter = GetComponent<MeshFilter>();
		meshRenderer = GetComponent<MeshRenderer>();
		meshCollider = GetComponent<MeshCollider>();

		iso = new Array3<IsoPoint>(size);
		indices = new Array3<int>(size);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	void BuildVertices()
	{
		List<IsoPoint> points = new List<IsoPoint>();
		List<Vector3> normals = new List<Vector3>();
		List<float> dists = new List<float>();

		indices.ForEach3(indices.Size, (Vector3Int pos) =>
		{
			bool[] inside = new bool[numCorners];
			int numInside = 0;

			for (int ci = 0; ci < numCorners; ci++)
			{
				inside[ci] = (iso[pos + corners[ci]].Dist <= 0);
				if (inside[ci])
					numInside++;
			}

			if (numInside == 0 || numInside == numCorners)
				return;

			bool[] crossingCorners = new bool[numCorners];

			foreach (Vector2Int edge in edges)
			{
				if (inside[edge.x] != inside[edge.y])
				{
					crossingCorners[edge.x] = true;
					crossingCorners[edge.y] = true;
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

				if (Mathf.Abs(point.Dist) > maxCornerDistance)
					continue;

				Vector3 voxelCenter = pos + new Vector3(0.5f, 0.5f, 0.5f);

				foreach (Vector3 axis in axes)
				{
					Vector3 n = pushSize * axis;
					points.Add(new IsoPoint(Vector3.Dot(n, voxelCenter), n));
				}

				Vector3 vertex = new Vector3(0, 0, 0); // Solve();

			}
		});
	}
}
