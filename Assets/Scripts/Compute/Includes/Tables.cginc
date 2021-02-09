static const int numCorners = 8;
static const int numAxes = 3;
static const int numEdges = 12;
static const int numFarEdges = 3;
static const int numNeighbours = 6;
static const int numPoints = numCorners + numAxes;

static uint3 corners[numCorners] = 
{
	uint3(0,0,0),
	uint3(0,0,1),
	uint3(0,1,0),
	uint3(0,1,1),
	uint3(1,0,0),
	uint3(1,0,1),
	uint3(1,1,0),
	uint3(1,1,1)
};

static int3 axes[numAxes] = 
{
	int3(1,0,0),
	int3(0,1,0),
	int3(0,0,1)
};

static uint2 edges[numEdges] = 
{
	uint2(0,1),
	uint2(0,2),
	uint2(0,4),
	uint2(1,3),
	uint2(1,5),
	uint2(2,3),
	uint2(2,6),
	uint2(3,7),
	uint2(4,5),
	uint2(4,6),
	uint2(5,7),
	uint2(6,7)
};

static uint2 farEdges[numFarEdges] = 
{
	uint2(3,7),
	uint2(5,7),
	uint2(6,7)
};

static int3 neighbours[numNeighbours] = 
{
	int3(0,0,1),
	int3(0,1,0),
	int3(0,1,1),
	int3(1,0,0),
	int3(1,0,1),
	int3(1,1,0)
};
