using System;
using UnityEngine;

public class Array3<T>
{
	T[] data;
	Vector3Int size;

	public Array3()
	{
		size = Vector3Int.zero;		
		data = new T[0];
	}

	public Array3(Vector3Int size)
	{
		this.size = size;
		data = new T[size.x * size.y * size.z];
	}

	public T this[int key]
	{
		get => data[key];
		set => data[key] = value;
	}

	public T this[Vector3Int pos]
	{
		get => data[((pos.z * size.y) + pos.y) * size.x + pos.x];
		set => data[((pos.z * size.y) + pos.y) * size.x + pos.x] = value;
	}

	public Vector3Int Size { get => size; }
	public int Count { get => size.x * size.y * size.z; }
	public T[] Data { get => data; set => data = value; }

	public void Fill(T value)
	{
		for (int i = 0; i < Count; i++)
			data[i] = value;
	}

	public void ForEach3(Vector3Int min, Vector3Int max, Action<Vector3Int> func)
	{
		for (int z = min.z; z < max.z; z++)	
		{
			for (int y = min.y; y < max.y; y++)	
			{
				for (int x = min.x; x < max.x; x++)	
				{
					func(new Vector3Int(x, y, z));
				}
			}
		}
	}

	public void ForEach3(Vector3Int max, Action<Vector3Int> func)
	{
		ForEach3(new Vector3Int(0, 0, 0), max, func);
	}

	public void ForEach3(Action<Vector3Int> func)
	{
		ForEach3(this.size, func);
	}
}
