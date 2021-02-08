using UnityEngine;

public class Generator 
{
	public static void AddSphere(Array3<IsoPoint> field, Vector3 center, float rad)
	{
		field.ForEach3( (Vector3Int pos) => 
		{
			var point = field[pos];

			Vector3 vec = pos - center;
			float d = vec.magnitude - rad;

			if (d < point.dist)
				field[pos] = new IsoPoint(d, vec.normalized);

		});
	}

	public static void RemoveSphere(Array3<IsoPoint> field, Vector3 center, float rad)
	{
		field.ForEach3( (Vector3Int pos) => 
		{
			var point = field[pos];

			Vector3 vec = pos - center;
			float d = vec.magnitude - rad;

			if (-d > point.dist)
				field[pos] = new IsoPoint(-d, -vec.normalized);

		});
	}

	public static void AddCylinder(Array3<IsoPoint> field, Vector2 center, float rad)
	{
		field.ForEach3( (Vector3Int pos) =>
		{
			var point = field[pos];
			var vec = new Vector2(pos.x, pos.y) - center;
			float d = vec.magnitude - rad;

			if (d < point.dist)
				field[pos] = new IsoPoint(d, vec.normalized);

		});
	}

	public static void RemoveCylinder(Array3<IsoPoint> field, Vector2 center, float rad)
	{
		field.ForEach3((Vector3Int pos) =>
		{
			var point = field[pos];
			var vec = new Vector2(pos.x, pos.y) - center;
			float d = vec.magnitude - rad;

			if (-d > point.dist)
				field[pos] = new IsoPoint(-d, -vec.normalized);

		});
	}
}
