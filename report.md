# Volumetric Terrain with Dual Contouring and Octree Based LOD

Fredrik Lind  
2021-03-22

# ABSTRACT

# INTRODUCTION

# THEORY
Terrain in video games is typically represented using heightmaps,
where elevation is stored as a floating point number between 0 and 1.
While this approach is fine for most applications, it has several limitations.
As it is only possible to store one single elevation value for any given point on the terrain, 
   you cannot represent caves, overhangs, or other landscape features where the terrain
   folds in on itself.
To some extent you can reproduce these features by masking out holes in the terrain and hand-placing
pre-made geometry to match. This approach requires extra work by both artists and level designers.
It is also unfeasable to use for procedurally generated terrain.

## Volume Data
An alternative solution is to use volumetric terrain.
The terrain surface, in this context, is typically referred to as the "isosurface."

In volumetric terrain, the elevation data is replaced by three-dimensional volume data.
This data can look different depending on the desired look and feel of the terrain.
For simple, blocky terrain, it is sufficient to store a boolean value for each "voxel" in the terrain,
	describing whether or not this block is filled.

Another common approach is to store density values. 
In essence, these values describe by how much any given voxel is filled.
As this data is more precise, it is possible to interpolate between voxels, and produce smooth terrain.

The Dual Contouring algorithm, used in this paper, relies on Hermite Data.
This data describes the points where the isosurface intersects any given voxel,
	 as well as their normal vectors (away from the surface).
	 
## Generating a Volume
Procedural terrain is often generated using layered noise of varying frequencies and amplitudes.
By gradually increasing the frequency of the noise lookups, while simultaneously reducing the amplitudes, you can achieve realistic-looking results.
The high-frequency noise resembles rocks and imperfections, while the low-frequencies give rise to continents and mountains.

For a traditional elevation map, it is sufficient to sample a 2D noise texture with points along the horizontal world axes as UV coordinates.
When dealing with 3D volumes, this same approach still works, by simply replacing the noise texture with a texture array (3D texture), 
	then use samples along the vertical world axis for texture indices in the array.
	
Creating actually interesting procedural terrain is an art-form in and of itself, and will not be explained in detail in this paper.

## Triangulation
In order to render volume data, there are a few methods to choose from.
Raytracing or raymarching will allow for rendering the data directly,
	without first creating a mesh.
However, it is not very performant on a large scale, and comes with a few tradeoffs.
As graphics processors are highly optimized to render triangles, 
   creating a polygonized approximation of the volume is probably a better option.
   This will also make it easier to perform collision checking.
There are many different algorithms that accomplish this, and the choice might be dependant on the format of the input data.

### Blocks
This is the simplest method of all, with no smoothing or interpolation to talk of.
If your input data is boolean in nature -- where a voxel is either on or off -- then this is probably your best option.
The resulting terrain will be jagged and terraced, resembling large stacked blocks.
As it is very easy to calculate the vertices for any given voxel, since they are cubical in nature, this method is very fast.
It is possible to run an optimizing algorithm, which will "merge" adjacent blocks together, reducing the polygon count.

### Marching Cubes
This algorithm, proposed at the SIGGRAPH '87 conference by XXX and XXX, was originally intented to visualize medical scan data.
The method uses a per-voxel density value to determine which voxels are inside the isosurface, comparing it to a specified treshold value.
It works by identifying a small number of unique triangle configurations, from which (when rotated and mirrored) it is possible to represent any isosurface with fair accuracy.
Marching Cubes uses large lookup tables to determine triangulation, which makes it very efficient, and well suited for real-time applications.
Optionally, one may perform linear interpolation between voxels to make the resulting surface smoother.

One caveat to this method is that, due to a lack of information in the input data, sharp edges on the isosurface are typically not retained.

### Dual Contouring
This is the method that was chosen for the application presented in this paper, primarily due to its accuracy.
Its ability to represent sharp edges was necessary to produce realistic sand dunes, as well as pyramids, rocks, and other things you might find in a desert.
Dual Contouring traditionally uses Hermite data to describe the volume being rendered. This format describes the intersection points between voxels, as well as the normals of the isosurface.
Alternatively, it is possible to store the volume as a Signed Distance Field (SDF), and calculate the intersection points and normals analytically.
An SDF describes, for any given voxel, the distance to the nearest point on the isosurface, as well as the direction (its surface normal).
The advantage of this is that SDF:s are well-researched and has many interesting properties for volume manipulation, such as easy subtraction and addition.
"Adding" a shape to another when using SDF:s is for example simply a matter of taking the minimum value of the two fields for any given voxel.

Calculating the normals can be done in two ways:
Either by deriving the generating functions in advance, or analytically by sampling the generator multiple times with small steps in each direction, calculating the normal based on the change in inclination. As the generator function easily grows to be complex, the latter can become fairly costly in the long run.

## GPU Acceleration

Graphics processors are actually very well suited for dealing with 3D volumes, as they closely resemble 3D textures.
A lot of the logic and optimizations that GPU:s provide can therefore be used to speed up both field generation and meshing.

There exists a specialized type of GPU shader called a "Compute Shader," used to perform complex calculation in parallel on the GPU.
A compute shader may contain one or more kernels, which are essentially program entry points.
Each kernel can specify how many threads should be assigned to a work group.
A multiple of 32 makes sense here, 64 being a good default -- as this corresponds well with the size of a warp/wavefront on AMD and NVidia GPU:s.
The group size is divided into three components: x, y, and z.

When dispatching the shader kernel, a certain number of work groups in each dimension are requested.
The product of the kernel group size and the requested number of groups will determine how many times the kernel is run.
The kernel provides an ID, represented as a 3D integer vector, which can be used to associate a kernel invokation with a voxel in the volume.

Flattening this ID will also allow for its use as an index in a GPU buffer, which can be used to pass data between dispatches.

For a volume of 64³ you might, for example, set up a kernel with a group size of (8, 8, 1), 
	which is then dispatched with a requested group count of (8, 8, 64), 
	resulting in the kernel being invoked once per voxel in the volume.

## Level of Detail

When dealing with terrain, it is desireable for the player to be able to see very far into the distance.
### Chunks




# RESULTS

# DISCUSSION

# REFERENCES
