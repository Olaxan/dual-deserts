using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{

	public Mesh contour;
	public Material defaultMaterial;
	public Vector3Int position;

	MeshFilter meshFilter;
	MeshRenderer meshRenderer;
	MeshCollider meshCollider;

	public void Setup()
	{
		meshFilter = GetComponent<MeshFilter>();
		meshRenderer = GetComponent<MeshRenderer>();
		meshCollider = GetComponent<MeshCollider>();

		if (meshFilter == null)
			meshFilter = gameObject.AddComponent<MeshFilter>();

		if (meshRenderer == null)
			meshRenderer = gameObject.AddComponent<MeshRenderer>();

		if (meshCollider == null)
			meshCollider = gameObject.AddComponent<MeshCollider>();

		contour = meshFilter.sharedMesh;

		if (contour == null)
		{
			contour = new Mesh();
			meshFilter.sharedMesh = contour;
		}

		if (meshCollider.sharedMesh == null)
		{
			meshCollider.sharedMesh = contour;

			meshCollider.enabled = false;
			meshCollider.enabled = true;
		}

		meshRenderer.material = defaultMaterial;

	}

	public void Refresh(Vector3Int localPos, Vector3 worldPos)
	{
		gameObject.name = localPos.ToString();
		position = localPos;
		gameObject.transform.position = worldPos;
	}
}

