#ifndef _VOLUME_CGINC_
#define _VOLUME_CGINC_


RWStructuredBuffer<float> isoDists;
RWStructuredBuffer<float3> isoNormals;

int3 isoSize;

int getVolumeIndex(int3 id)
{
	return ((id.z * isoSize.y) + id.y) * isoSize.x + id.x;
}

bool checkEdge(int3 id, int range)
{
	return (id.x >= isoSize.x - range 
		|| id.y >= isoSize.y - range 
		|| id.z >= isoSize.z - range);
}

bool isBorder(int3 id)
{
	return (id.x * id.y * id.z == 0) || checkEdge(id, 1);
}

#endif