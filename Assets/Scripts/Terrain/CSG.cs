using UnityEngine;

public enum OpShape
{
	Box = 0,
	Sphere = 1,
	Octahedron = 2
}

public enum OpType
{
	Union = 0,
	Subtraction = 1,
	Difference = 2
}

public struct CSG
{
	public Vector3 position;
	public float radius;
	public OpShape shape;
	public OpType type;

	public CSG(Vector3 pos, float r, OpShape shape, OpType type)
	{
		this.position = pos;
		this.radius = r;
		this.shape = shape;
		this.type = type;
	}
};
