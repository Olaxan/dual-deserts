using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainModifier : MonoBehaviour
{

	public OctLoaderTest loader;

	public float rayLength = 100.0f;

	[Header("Operation Settings")]

	[Range(0.1f, 32.0f)]
	public float radius = 10.0f;

	public OpShape shape = OpShape.Box;
	public OpType type = OpType.Union;

	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast(ray, out hit, rayLength))
			{
				if (hit.collider != null)
				{
					CSG op = new CSG(hit.point, radius, shape, type);
					loader.AddOperation(op);
				}
			}

		}
	}
}
