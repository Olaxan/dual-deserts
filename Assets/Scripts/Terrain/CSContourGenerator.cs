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

	int size;

	ComputeBuffer isoDistBuffer;
	ComputeBuffer isoNormalBuffer;

	ComputeBuffer indexBuffer;
	ComputeBuffer vertexBuffer;
	ComputeBuffer normalBuffer;
	ComputeBuffer quadBuffer;

	ComputeBuffer quadCountBuffer;
	ComputeBuffer vertexCountBuffer;

	PriorityQueue<(Chunk, List<CSG>), int> buildQueue;

	int _vertexKernel;
	int _triangleKernel;
	uint _threadSizeX;
	uint _threadSizeY;
	uint _threadSizeZ;
	int bufferSize;

	int VoxelSize { get => size - 1; }

	void Update()
	{
		for (int i = 0; i < Mathf.Min(chunksPerFrame, buildQueue.Count); i++)
		{
			var pair  = buildQueue.Dequeue();
			Chunk chunk = pair.Item1;
			List<CSG> operations = pair.Item2;

			chunk.contour.Clear();
			Remesh(chunk, operations);
			chunk.gameObject.SetActive(true);
			chunk.Opacity = 1f;
		}
	}

	void OnDestroy()
	{
		ReleaseBuffers();
	}

	public void Setup(int isoSize)
	{
		terrainGenerator = gameObject.GetComponent<CSGenerator>();
		if (terrainGenerator == null)
			terrainGenerator = gameObject.AddComponent<CSGenerator>();

		_vertexKernel = contourGenerator.FindKernel("CSMakeVertices");
		_triangleKernel = contourGenerator.FindKernel("CSMakeTriangles");

		contourGenerator.GetKernelThreadGroupSizes(_vertexKernel, 
				out _threadSizeX, out _threadSizeY, out _threadSizeZ);

		buildQueue = new PriorityQueue<(Chunk, List<CSG>), int>();

		size = isoSize;

		SetupBuffers();
	}

	public void RemeshAsync(Chunk chunk, List<CSG> operations, int priority)
	{
		chunk.gameObject.SetActive(false);
		buildQueue.Enqueue((chunk, operations), priority);
	}

	public void Remesh(Chunk chunk, List<CSG> operations)
	{

		if (bufferSize != size)
			SetupBuffers();

		int pointCount = size * size * size;
		int indexCount = VoxelSize * VoxelSize * VoxelSize;

		float chunkScale = chunk.Size / (float)(size - 2);

		Vector3Int ts = new Vector3Int(
				Mathf.CeilToInt(size / _threadSizeX), 
				Mathf.CeilToInt(size / _threadSizeY), 
				Mathf.CeilToInt(size / _threadSizeZ));

		terrainGenerator.SetOperations(operations);
		terrainGenerator.SetBuffers(isoDistBuffer, isoNormalBuffer);
		terrainGenerator.Generate(chunk.WorldPos, size, chunkScale);

		contourGenerator.SetInt("isoSize", size); 
		contourGenerator.SetFloat("isoScale", chunkScale);
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
		int pointCount = size * size * size;
		int indexCount = VoxelSize * VoxelSize * VoxelSize;
		int quadCount = indexCount * 3 * 2;

		bufferSize = size;

		ReleaseBuffers();

		isoDistBuffer = new ComputeBuffer(pointCount, sizeof(float));
		isoNormalBuffer = new ComputeBuffer(pointCount, 3 * sizeof(float));

		indexBuffer = new ComputeBuffer(indexCount, sizeof(int));
		vertexBuffer = new ComputeBuffer(indexCount, 3 * sizeof(float), ComputeBufferType.Counter);
		normalBuffer = new ComputeBuffer(indexCount, 3 * sizeof(float));
		quadBuffer = new ComputeBuffer(indexCount * 3, 2 * 3 * sizeof(int), ComputeBufferType.Append);

		quadCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
		vertexCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

		contourGenerator.SetBuffer(_vertexKernel, "isoDists", isoDistBuffer);
		contourGenerator.SetBuffer(_vertexKernel, "isoNormals", isoNormalBuffer);
		contourGenerator.SetBuffer(_vertexKernel, "indices", indexBuffer);
		contourGenerator.SetBuffer(_vertexKernel, "vertices", vertexBuffer);
		contourGenerator.SetBuffer(_vertexKernel, "normals", normalBuffer);
		contourGenerator.SetBuffer(_vertexKernel, "quads", quadBuffer);

		contourGenerator.SetBuffer(_triangleKernel, "isoDists", isoDistBuffer);
		contourGenerator.SetBuffer(_triangleKernel, "isoNormals", isoNormalBuffer);
		contourGenerator.SetBuffer(_triangleKernel, "indices", indexBuffer);
		contourGenerator.SetBuffer(_triangleKernel, "quads", quadBuffer);
	}

	void ReleaseBuffers()
	{
		if (isoDistBuffer != null)
			isoDistBuffer.Release();
		
		if (isoNormalBuffer != null)
			isoNormalBuffer.Release();

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

		isoDistBuffer = null;
		isoNormalBuffer = null;
		indexBuffer = null;
		vertexBuffer = null;
		normalBuffer = null;
		quadBuffer = null;
		quadCountBuffer = null;
		vertexCountBuffer = null;
	}
}
