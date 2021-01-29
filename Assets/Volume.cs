using UnityEngine;


public class IsoPoint
{
	public float Dist { get; set; }
	public Vector3 Normal { get; set; }

	public IsoPoint(float dist, Vector3 normal)
	{
		this.Dist = dist;
		this.Normal = normal;
	}
}
