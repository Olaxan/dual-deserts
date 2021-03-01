using UnityEngine;

enum OpShape
{
	Box = 0,
	Sphere = 1,
	Octahedron = 2
}

enum OpType
{
	Union = 0,
	Subtraction = 1,
	Difference = 2
}

struct CSG
{
	Vector3 position;
	float radius;
	OpShape shape;
	OpType type;

	public CSG(Vector3 pos, float r, OpShape shape, OpType type)
	{
		this.position = pos;
		this.radius = r;
		this.shape = shape;
		this.type = type;
	}
};
