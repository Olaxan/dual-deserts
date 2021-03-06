﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{

	public Mesh contour;
	public Material defaultMaterial;

	public Vector3 WorldPos { get => transform.position; }
	public Vector3Int GridPos { get; private set; }
	public float Size { get; private set; }

	public float Opacity 
	{ 
		get => meshRenderer.material.GetFloat("_Opacity"); 
		set => meshRenderer.material.SetFloat("_Opacity", value); 
	}

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
			UpdateCollider();
		}

		meshRenderer.material = defaultMaterial;

	}

	public void Refresh(Vector3Int gridPos, float size)
	{
		gameObject.name = $"{gridPos.ToString()} / {size}";
		gameObject.transform.position = gridPos - Vector3.one * size / 2.0f;
		Size = size;
		GridPos = gridPos;
	}

	public void UpdateCollider()
	{
		meshCollider.enabled = false;
		meshCollider.enabled = true;
	}
}

