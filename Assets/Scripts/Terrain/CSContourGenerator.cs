﻿using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CSGenerator))]
public class CSContourGenerator : MonoBehaviour
{

	public ComputeShader contourGenerator;

	[Header("Voxel Settings")]
	public float centerBias;
	public float maxCornerDistance;
	public float clampRange;

	public int chunksPerFrame = 1;

	CSGenerator terrainGenerator;

	Vector3Int size;
	Vector3 scale;

	ComputeBuffer isoBuffer;
	ComputeBuffer indexBuffer;
	ComputeBuffer vertexBuffer;
	ComputeBuffer normalBuffer;
	ComputeBuffer quadBuffer;
	ComputeBuffer quadCountBuffer;
	ComputeBuffer vertexCountBuffer;

	Queue<Chunk> buildQueue;

	int _vertexKernel;
	int _triangleKernel;
	uint _threadSizeX;
	uint _threadSizeY;
	uint _threadSizeZ;
	Vector3Int bufferSize;

	Vector3Int VoxelSize { get => size - Vector3Int.one; }

	void Update()
	{
		for (int i = 0; i < chunksPerFrame; i++)
		{
			if (buildQueue.Count > 0)
			{
				Chunk chunk = buildQueue.Dequeue();
				chunk.contour.Clear();
				GenerateChunk(chunk);
				chunk.gameObject.SetActive(true);
			}
			else return;
		}
	}

	void OnDestroy()
	{
		ReleaseBuffers();
	}

	public void Setup(Vector3Int isoSize, Vector3 isoScale)
	{
		terrainGenerator = gameObject.GetComponent<CSGenerator>();
		if (terrainGenerator == null)
			terrainGenerator = gameObject.AddComponent<CSGenerator>();

		_vertexKernel = contourGenerator.FindKernel("CSMakeVertices");
		_triangleKernel = contourGenerator.FindKernel("CSMakeTriangles");

		contourGenerator.GetKernelThreadGroupSizes(_vertexKernel, 
				out _threadSizeX, out _threadSizeY, out _threadSizeZ);

		buildQueue = new Queue<Chunk>();

		size = isoSize;
		scale = isoScale;

		SetupBuffers();

	}

	public void RequestRemesh(Chunk chunk)
	{
		chunk.gameObject.SetActive(false);
		buildQueue.Enqueue(chunk);
	}

	void GenerateChunk(Chunk chunk)
	{

		if (bufferSize != size)
			SetupBuffers();

		int pointCount = size.x * size.y * size.z;
		int indexCount = VoxelSize.x * VoxelSize.y * VoxelSize.z;

		Vector3Int ts = new Vector3Int(
				Mathf.CeilToInt(size.x / _threadSizeX), 
				Mathf.CeilToInt(size.y / _threadSizeY), 
				Mathf.CeilToInt(size.z / _threadSizeZ));

		terrainGenerator.Generate(isoBuffer, chunk.position, size, scale);

		contourGenerator.SetInts("isoSize", new int[] { size.x, size.y, size.z });
		contourGenerator.SetFloats("scale", new float[] { scale.x, scale.y, scale.z });
		contourGenerator.SetFloat("maxCornerDistance", maxCornerDistance);
		contourGenerator.SetFloat("centerBias", centerBias);
		contourGenerator.SetFloat("clampRange", clampRange);

		quadBuffer.SetCounterValue(0);
		vertexBuffer.SetCounterValue(0);
		indexBuffer.SetCounterValue(0);

		contourGenerator.Dispatch(_vertexKernel, ts.x, ts.y, ts.z);
		contourGenerator.Dispatch(_triangleKernel, ts.x, ts.y, ts.z);

		ComputeBuffer.CopyCount(quadBuffer, quadCountBuffer, 0);
		int[] quadCountArray = { 0 };
		quadCountBuffer.GetData(quadCountArray);
		int quadCount = quadCountArray[0];
		
		ComputeBuffer.CopyCount(vertexBuffer, vertexCountBuffer, 0);
		int[] vertexCountArray = { 0 };
		vertexCountBuffer.GetData(vertexCountArray);
		int vertexCount = vertexCountArray[0];

		Vector3[] vertices = new Vector3[vertexCount];
		Vector3[] normals = new Vector3[vertexCount];
		int[] triangles = new int[2 * 3 * quadCount];

		quadBuffer.GetData(triangles);
		vertexBuffer.GetData(vertices);
		normalBuffer.GetData(normals);

		Mesh contour = chunk.contour;

		if (contour == null)
			return;

		contour.Clear();
		contour.vertices = vertices;
		contour.triangles = triangles;
		contour.normals = normals;

		chunk.UpdateCollider();
	}

	void SetupBuffers()
	{
		int pointCount = size.x * size.y * size.z;
		int indexCount = VoxelSize.x * VoxelSize.y * VoxelSize.z;
		int quadCount = indexCount * 3 * 2;

		bufferSize = size;

		ReleaseBuffers();

		isoBuffer = new ComputeBuffer(pointCount, 4 * sizeof(float));
		indexBuffer = new ComputeBuffer(indexCount, sizeof(int));
		vertexBuffer = new ComputeBuffer(indexCount, 3 * sizeof(float), ComputeBufferType.Counter);
		normalBuffer = new ComputeBuffer(indexCount, 3 * sizeof(float));
		quadBuffer = new ComputeBuffer(indexCount * 3, 2 * 3 * sizeof(int), ComputeBufferType.Append);
		quadCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
		vertexCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

		contourGenerator.SetBuffer(_vertexKernel, "iso", isoBuffer);
		contourGenerator.SetBuffer(_vertexKernel, "indices", indexBuffer);
		contourGenerator.SetBuffer(_vertexKernel, "vertices", vertexBuffer);
		contourGenerator.SetBuffer(_vertexKernel, "normals", normalBuffer);
		contourGenerator.SetBuffer(_vertexKernel, "quads", quadBuffer);

		contourGenerator.SetBuffer(_triangleKernel, "iso", isoBuffer);
		contourGenerator.SetBuffer(_triangleKernel, "indices", indexBuffer);
		contourGenerator.SetBuffer(_triangleKernel, "quads", quadBuffer);
	}

	void ReleaseBuffers()
	{
		if (isoBuffer != null)
			isoBuffer.Release();

		if (indexBuffer != null)
			indexBuffer.Release();

		if (vertexBuffer != null)
			vertexBuffer.Release();

		if (normalBuffer != null)
			normalBuffer.Release();

		if (quadBuffer != null)
			quadBuffer.Release();

		if (quadCountBuffer != null)
			quadCountBuffer.Release();

		if (vertexCountBuffer != null)
			vertexCountBuffer.Release();

		isoBuffer = null;
		indexBuffer = null;
		vertexBuffer = null;
		normalBuffer = null;
		quadBuffer = null;
		quadCountBuffer = null;
		vertexCountBuffer = null;
	}
}
