using UnityEngine;

public class IsoPoint
{
	public float Dist { get; set; } = 0;
	public Vector3 Normal { get; set; } = Vector3.zero;

	public IsoPoint()
	{
		Dist = 0;
		Normal = Vector3.zero;
	}

	public IsoPoint(float dist, Vector3 normal)
	{
		this.Dist = dist;
		this.Normal = normal;
	}
}
