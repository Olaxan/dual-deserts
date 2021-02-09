﻿struct IsoPoint
{
	float dist;
	float3 normal;
};

RWStructuredBuffer<IsoPoint> iso;

int3 isoSize;

int getVolumeIndex(int3 id)
{
	return ((id.z * isoSize.y) + id.y) * isoSize.x + id.x;
}

bool isEdge(int3 id)
{
	return (id.x >= isoSize.x - 1 || id.y >= isoSize.y - 1 || id.z >= isoSize.z - 1);
}

bool isBorder(int3 id)
{
	return (id.x * id.y * id.z == 0) || isEdge(id);
}