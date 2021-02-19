#ifndef _VOLUME_CGINC_
#define _VOLUME_CGINC_


RWStructuredBuffer<float> isoDists;
RWStructuredBuffer<float3> isoNormals;

uint isoSize;
float isoScale;

uint getVolumeIndex(uint3 id)
{
	return ((id.z * isoSize) + id.y) * isoSize + id.x;
}

bool checkEdge(uint3 id, uint range)
{
	return (id.x >= isoSize - range 
		|| id.y >= isoSize - range 
		|| id.z >= isoSize - range);
}

bool isBorder(uint3 id)
{
	return (id.x * id.y * id.z == 0) || checkEdge(id, 1);
}

#endif
