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

			if (d < point.Dist)
			{
				point.Dist = d;
				point.Normal = vec.normalized;
			}
		});
	}

	public static void RemoveSphere(Array3<IsoPoint> field, Vector3 center, float rad)
	{
		field.ForEach3( (Vector3Int pos) => 
		{
			var point = field[pos];

			Vector3 vec = pos - center;
			float d = vec.magnitude - rad;

			if (-d > point.Dist)
			{
				point.Dist = -d;
				point.Normal = -vec.normalized;
			}
		});
	}
}
