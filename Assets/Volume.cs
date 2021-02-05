using UnityEngine;

public struct IsoPoint
{
	public float dist;
	public Vector3 normal;

	public IsoPoint(float dist, Vector3 normal)
	{
		this.dist = dist;
		this.normal = normal;
	}
}
