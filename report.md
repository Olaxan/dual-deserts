# Volumetric Terrain with Dual Contouring and Octree Based LOD

Fredrik Lind  
2021-03-22

## ABSTRACT



## INTRODUCTION
In the third year of the Bachelor Programme of Computer Engineering, students at Luleå University of Technology
	are given the chance to select a research paper of their choosing (related to video games or graphics programming), 
	and attempt to implement the technology.

There is a lot of knowledge to be gained from creating a voxel engine, due to challenges with data storage and optimization
	-- often requiring the use of interesting algorithms and data structures to solve efficiently.
For that reason, as well as a long-lived interest in procedural generation,
	I decided to attempt an implementation of Dual Contouring [REFERENCE]. 
This algorithm was selected over similar, simpler methods for two reasons:
First and foremost, it's well-reknowned and delivers good results.
Secondly, I wanted to challenge myself with a more complex algorithm.

The idea was to create a sandbox with volumetric, destructible terrain,
	set in an infinite (or at least very large), procedurally generated desert environment.
A desert is forgiving, and can look good even without flora and terrain scatters.

The Unity engine was chosen due to its excellent scripting support and ease-of-use.

## THEORY
Terrain in video games is typically represented using heightmaps,
where elevation is stored as a floating point number between 0 and 1.
While this approach is fine for most applications, it has several limitations.
As it is only possible to store one single elevation value for any given point on the terrain, 
   you cannot represent caves, overhangs, or other landscape features where the terrain
   folds in on itself.
To some extent you can reproduce these features by masking out holes in the terrain and hand-placing
pre-made geometry to match. This approach requires extra work by both artists and level designers.
It is also unfeasable to use for procedurally generated terrain.

An alternative solution is to use volumetric terrain.
The terrain surface, in this context, is typically referred to as the "isosurface."

### Volume Data
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
	 
### Generating a Volume
Procedural terrain is often generated using layered noise of varying frequencies and amplitudes.
By gradually increasing the frequency of the noise lookups, while simultaneously reducing the amplitudes, you can achieve realistic-looking results.
The high-frequency noise resembles rocks and imperfections, while the low-frequencies give rise to continents and mountains.

For a traditional elevation map, it is sufficient to sample a 2D noise texture with points along the horizontal world axes as UV coordinates.
When dealing with 3D volumes, this same approach still works, by simply replacing the noise texture with a texture array (3D texture), 
	then use samples along the vertical world axis for texture indices in the array.
	
Creating actually interesting procedural terrain is an art-form in and of itself, and will not be explained in detail in this paper.

### Triangulation
In order to render volume data, there are a few methods to choose from.
Raytracing or raymarching will allow for rendering the data directly,
	without first creating a mesh.
However, it is not very performant on a large scale, and comes with a few tradeoffs.
As graphics processors are highly optimized to render triangles, 
   creating a polygonized approximation of the volume is probably a better option.
   This will also make it easier to perform collision checking.
There are many different algorithms that accomplish this, and the choice might be dependant on the format of the input data.

#### Blocks
This is the simplest method of all, with no smoothing or interpolation to talk of.
If the input data is boolean in nature -- where a voxel is either on or off -- then this is probably the best option.
The resulting terrain will be jagged and terraced, resembling large stacked blocks.
As it is very easy to calculate the vertices for any given voxel, since they are cubical in nature, this method is very fast.
It is possible to run an optimizing algorithm, which will "merge" adjacent blocks together, reducing the polygon count.

#### Marching Cubes
This algorithm, proposed at the SIGGRAPH '87 conference by Lorensen and Cline, was originally intented to visualize medical scan data.
The method uses a per-voxel density value to determine which voxels are inside the isosurface, comparing it to a specified treshold value.
It works by identifying a small number of unique triangle configurations, from which (when rotated and mirrored) it is possible to represent any isosurface with fair accuracy.
Marching Cubes uses large lookup tables to determine triangulation, which makes it very efficient, and well suited for real-time applications.
Optionally, one may perform linear interpolation between voxels to make the resulting surface smoother.

One caveat to this method is that, due to a lack of information in the input data, sharp edges on the isosurface are typically not retained.

#### Dual Contouring
This is the method that was chosen for the application presented in this paper, primarily due to its accuracy.
Its ability to represent sharp edges was necessary to produce realistic sand dunes, as well as pyramids, rocks, and other things you might find in a desert.
Dual Contouring traditionally uses Hermite data to describe the volume being rendered. This format describes the intersection points between voxels, as well as the normals of the isosurface.
Alternatively, it is possible to store the volume as a Signed Distance Field (SDF), and calculate the intersection points and normals analytically.
An SDF describes, for any given voxel, the distance to the nearest point on the isosurface, as well as the direction (its surface normal).
The advantage of this is that SDF:s are well-researched and has many interesting properties for volume manipulation, such as easy subtraction and addition.
"Adding" a shape to another when using SDF:s is for example simply a matter of taking the minimum value of the two fields.

Calculating the normals can be done in two ways:
Either by deriving the generating functions in advance, or analytically by sampling the generator multiple times with small steps in each direction, calculating the normal based on the change in inclination. As the generator function easily grows to be complex, the latter can become fairly costly in the long run.

### GPU Acceleration

Graphics processors are actually very well suited for dealing with 3D volumes, as they closely resemble 3D textures.
A lot of the logic and optimizations that GPU:s provide can therefore be used to speed up both field generation and meshing.

There exists a specialized type of GPU shader called a "Compute Shader," used to perform complex calculations in parallel on the GPU.
A compute shader may contain one or more kernels, which are essentially program entry points.
Each kernel can specify how many threads should be assigned to a work group.
A multiple of 32 makes sense here, 64 being a good default -- as this corresponds well with the size of a warp/wavefront on AMD and NVidia GPU:s.
The group size is divided into three components: x, y, and z.

When dispatching the shader kernel, a certain number of work groups in each dimension are requested.
The product of the kernel group size and the requested number of groups will determine how many times the kernel is run.

For a volume of 64³ you might, for example, set up a kernel with a group size of (8, 8, 1), 
	which is then dispatched with a requested group count of (8, 8, 64), 
	resulting in the kernel being invoked once per voxel in the volume.

The kernel provides an ID, represented as a 3D integer vector, which can be used to associate a kernel invokation with a voxel in the volume.
Flattening this ID will also allow for its use as an index in a GPU buffer, which can be used to pass data between dispatches.

It is worth keeping in mind that, while a GPU can perform these calculations faster than a CPU by orders of magnitude,
   a bottleneck will arise when passing data between the two.

### Level of Detail

When dealing with terrain, it is desireable for the player to be able to see very far into the distance.
However, the amount of memory used to store volume data grows quickly as the volume does.
While an SDF volume of 64³ requires roughly 34 MB of storage, a 128³ volume requires 268 MB -- and doubling the volume size again to 256³ increases the storage requirement to a staggering 2 GB.
Clearly, and not very surprisingly, it's not feasable to represent the entire terrain as a single large volume.
Some method of division must therefore be implemented.

#### Uniform Chunks
Voxel terrain is typically divided into logical chunks.
These are then streamed in and out depending on the position of the player, offsetting the generation function with the world position of the chunk.
The size of a chunk depends on a few factors:
A large chunk is more efficient to render, but requires more time to re-mesh when it has been modified.
Most engines land somewhere between 32 and 64 voxels per chunk.

There is a lot to consider when deciding how to handle loading and unloading of chunks.
No matter how a chunk is represented in the game engine, creating a new one will require allocating memory, which can be slow and costly.
Re-using chunks is usually a good idea. It is also important to keep the number of loaded chunks to a minimum for rendering performance.

A simple approach to loading chunks is simply to load a number of uniformly sized chunks around the player, based on some view distance value.
This will result in a roughly spherical volume of chunks being loaded.
While this method is sufficient to ensure the player can walk around, it might not be suitable for rendering very distant terrain.
A view distance of 2 km (not uncommon at all in modern games) would require 32 chunks in either direction.
This may not sound so bad at first, but a spherical volume with a radius of 32 chunks would contain a staggering 137 258 chunks!

Skipping chunks that are entirely occluded by neighbouring chunks, or that consists entirely of air would alleviate the problem slightly.
However, this requires fairly advanced methods, and will not in itself be sufficent to achieve satisfactory performance.
Reducing the vertex count of faraway chunks would speed up rendering slightly, but require more frequent re-meshing as the LOD level increases.
It is also unlikely that rendering performance is by itself the main bottleneck.

#### Octree Based LOD
A better solution -- and a common one at that -- is to increase the size of distant terrain chunks, while retaining the same voxel volume size.
A chunk 2 km away might, for instance, occupy a space of roughly the same size, which would replace some 30 000 chunks with a size of 64 units with a single one.
This approach will result in significant performance gains, as well as the ability to render essentially endless terrain.

A good data structure for this is the octree; a tree structure where every node contains exactly 8 children.
Starting with a base size of some value N -- which can be very large -- we recursively divide the cube into 8 parts, each N/8 in size,
	and then dividing those parts again, until we reach a minimum specified value, which could be the size of our voxel field size.
The division of cells should be based on the distance between their origin, and the LOD observer.
As the observer moves through the level, nearby cells can split, and cells further away can merge.

As the complexity scales logarithmically, doubling the view distance when using this method only requires 7 additional chunks.
A disadvantage to this approach is that octrees are fairly memory inefficient, and slow to iterate if required. On the other hand they are very fast to search.

### Texturing Procedural Meshes
Procedurally generated meshes don't have predefined UV information, meaning any texture applied to them will stretch across its entirety.
It may be possible to generate UV:s depending on the complexity of the terrain, but a better alternative may be to use triplanar texture mapping.
This technique uses world-space coordinates to sample the texture, which removes the need for UV:s entirely.
Applying a single texture on the mesh isn't sufficient -- areas with steep inclination along the axis being sampled will receive a very stretched texture.
This can be solved by sampling the texture three times, once per axis, and then blending the result together.

### Terrain Modifications
Most games using volumetric terrain will want to allow the player to somehow modify the terrain, either by digging or placing terrain.
This can be achieved in different ways depending on the format of the volume data, but when using SDF:s to represent the terrain
	 a good solution may be Constructive Solid Geometry (CSG) modelling.
Complex objects can be represented with this method as a series of operations using simple primitive shapes.
These primitives include any shape that can somehow be expressed as a mathematical function, taking a position as an input and returning the smallest distance from that point to the shape.
They are sometimes referred to as distance estimators.

There are three main CSG operations to speak of:
 * The Union operation will combine two primitives.
 * The Intersection operation will subtract one primitive from another.
 * The Difference operation will result in a primitive existing only in the intersection between two shapes.

Traditionally, the operations are represented as a tree. In the use-case described here, a list of operations is sufficient.
A struct describing an operation was implemented. When the user modifies the terrain, a node containing the operation is added to a list.
This list is sent to the terrain generation algorithm whenever a chunk needs rebuilding. 
The terrain generator then iterates over the list as part of the generation function, applying operations sequentially.
Since it is part of the SDF generator function, the operations will be considered when calculating normals later-on.
This process will have to repeat several times per voxel, and is fairly slow. On the GPU it is fast enough, but will most likely cause delay if attempted on the CPU.
A caveat of running on the GPU is that the space for operations must be allocated beforehand, resulting in a fixed number of operations per chunk.
However, since the data is very small when compared to HD textures and meshes, the number of operations can be fairly large.

## RESULTS
The final result is able to produce terrain that, while not boundless, is large enough to stretch into the horizon.
The player can dig and place terrain freely, as well as on chunk boundaries, with an (adjustable) limit of 1024 operations per chunk.
The LOD system is able to accomodate several objects of importance, such that you can "pin" terrain at a certain LOD level. 
This allows for pre-placed structures to be kept in high LOD at all times, ensuring they are visible in the distance. Their radius can be adjusted per-item.
At lower LOD levels, chunks are made to overlap slightly, ensuring there are no visible cracks.
However, there are occasional cracks visible between chunks of high LOD level.
Terrain with low LOD level can occasionally suffer from artefacts, such as "spikes" appearing.

## DISCUSSION
Overall, I'm quite happy with the result, and I mostly achieved what I set out to achieve.
The view distance is excellent, and rendering performance is fairly good.

There are a good few limitations, however.

Even on fairly good computers it is common to get stutters when modifying the terrain, 
	 especially on the seams of chunks, where up to 8 chunks will need to be remeshed instantly.

The graphical issues in distant terrain, as well as the cracks between chunks can no doubt be adressed by dialling back the Dual Contouring parameters with regards to chunk size.

Chunks lack occlusion testing, and as a result will render even if they are completely enclosed in neighbouring terrain chunks, or completely empty.
If caves were to be added, this would cause such terrible overdraw problems that the game would grind to a halt.
It is probably not necessary to have an equal vertical draw distance as horizontal (although this would allow for some really interesting caves).

Terrain generation and polygonization happens on the GPU, with structured buffers housing all the volume and triangle data.
Afterwards, this data is transferred back to the CPU, where it is inserted into Unity's rendering API -- which no doubt returns it to the GPU.
This back-and-forth is a bottleneck that could probably be adressed, retaining the triangle information on the GPU.
That said, unless collision is handled on the GPU as well, the CPU will need some knowledge of the data.
Performing the readback asynchronously could help reduce stutters when re-meshing.

## REFERENCES
