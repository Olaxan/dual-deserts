# Dual Deserts
A desert environment powered by Dual Contouring.
In the third year of the Bachelor Programme of Computer Engineering, 
students at Luleå University of Technology are given the chance to select a research paper of their choosing 
(related to video games or graphics programming), 
and attempt to implement the technology.

There is a lot of knowledge to be gained from creating
a voxel engine, due to challenges with data storage and optimization 
-- often requiring the use of interesting algorithms and data structures to solve efficiently.

For that reason, as well as a long-lived interest in procedural generation, 
I decided to attempt an implementation of Dual Contouring 
(Ju, Losasso, Schaefer, & Warren, 2002).

This algorithm was selected over similar, simpler methods for two reasons: 
First and foremost, it is wellrenowned and delivers good results. 
Secondly, I wanted to challenge myself with a more complex algorithm.

The idea was to create a sandbox with volumetric, destructible terrain, 
set in an infinite (or at least very large), procedurally generated desert environment. 
A desert is forgiving and can look good even without flora and terrain scatters.

The Unity engine was chosen due to its excellent
scripting support and ease-of-use

## Further Reading
For a detailed report on implementation and design, see [REPORT.md](REPORT.md).

## Usage

Open the Unity project (see below for details on version and requirements) and enter play mode,
and you will find yourself in the vast desert. You can dig by clicking, and walk freely around.

### First Person Settings
The first person character (FirstPerson-AIO) contains the Terrain Modifier script, which lets the player dig.
Here you can modify the settings for CSG operations, for example shape, size, and operation type.
If you want to add to the terrain rather than dig, change the `Type` variable.

### OctLoader Settings
This object contains the scripts that load, unload, and re-mesh chunks of terrain.
It has a multitude of editable parameters, which will be described in detail below:

#### Oct Loader Test
This poorly-named script is what handles loading and unloading of chunks, as well as CSG operations.

* `Tick Rate` determines the minimum delay in frames before the terrain octree is re-evaluated.
##### Voxel Settings
* `Voxel Size` determines the size of the SDF volume used to generate terrain. 
This should not be changed during runtime, as its size is used to allocated GPU memory.
##### LOD Settings
* `Lod Volume Size` is the size of the whole, undivided terrain octree, and as such the size of the play area.
* `Lod Logical Volume Size` is the size (in game units) of the smallest, highest LOD terrain chunks. It can be different from the `Voxel Size`, but it might be best to keep them the same.
* `Lod Fade Out Frames` determines the duration of the dithered fade-out effect when transitioning LOD levels.
* `Lod Fade` toggles the fade-out effect entirely.
* `Lod Draw Bounds` will allow you to see the terrain Octree visualised in the Scene tab during play.
* `World Objects` is a list that contains objects of LOD importance, and will determine where the octree will be refined to higher LOD levels:
	* `Terrain Object` determines the object of importance in the scene.
	* `Importance` determines the range of influence this object has, higher meaning a larger area will be refined.
##### CSG Settings
* `Csg Lod Radius Mult` will determine how well terrain modifications are represented in LOD. Increasing this will let smaller terrain modifications be represented in distant terrain.
##### Material Settings
* `Default Material` determines which material will be applied to new terrain chunks, and should probably not be changed.
	
#### CS Generator
The Generator script (CS standing for Compute Shader) handles terrain generation.

* `Terrain Shader` is a link to the compute shader resource, and should not be changed.
##### Terrain Settings
* `Surface Level` determines the lowest floor-level of the terrain.
* `Surface Scale` determines the low-level frequency of terrain noise, and should be quite small (0.000x).
* `Surface Magnitude` determines the height of the low-frequency features of the terrain (i.e. mountain height). The features will get ~1.5 times this high at most.
* `Warp Scale` determines the frequency of the position-warping noise which is then used for the rest of the terrain generation. This can result in strange-looking terrain.
* `Warp Magnitude` determines the maximum warping "offset" of the warping noise.
* `Cave Scale` has no effect.
* `Cave Magnitude` has no effect.
* `Noise Offset` determines an offset in game units into the terrain noise generator.
* `Noise Scale` is a global scaling value applied to the terrain noise generator.
* `Derivative Step` determines the epsilon value which is used to analytically calculate SDF normals.
##### Deformation Settings
* `CSG Operation Limit` determines the maximum number of modifications per chunk, and is used to allocate GPU memory.

#### Contour Generator
This script performs volume polygonization on the GPU.

##### Voxel Settings
* `Center Bias` applies a small "push" towards the center of a voxel when solving the QEF, and is required to get satisfactory results. Too much will result in blocky terrain, too little in artefacts.
* `Max Corner Distance` determines when a cutoff, SDF distances over which won't be used to create polygons.
* `Clamp Range` determines the range inside which vertices will be restricted, to prevent strange artefacts.
* `Chunks Per Frame` determines how many chunks can be generated every game tick.

## Build Instructions

The project was created in Unity version 2019.4.18f1 using a personal license with the HDRP pipeline.

## External Dependencies 

The project makes use of the following third-party tools and assets:

* First Person All-in-one  
	https://assetstore.unity.com/packages/tools/input-management/first-person-all-in-one-135316
* UnityOctree  
	https://github.com/Nition/UnityOctree
* More Effective Coroutines [FREE]  
	https://assetstore.unity.com/packages/tools/animation/more-effective-coroutines-free-54975
