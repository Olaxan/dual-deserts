using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TerrainObject
{
	public GameObject terrainObject;
	public float importance = 1.0f;

	public float GetDistanceTo(Vector3 position)
	{
		return (terrainObject.transform.position - position).magnitude / importance;
	}
}

public class TerrainOctree 
{

	// Root node of the octree
	TerrainOctreeNode rootNode;

	// Size that the octree was on creation
	readonly int initialSize;

	// Minimum side length that a node can be - essentially an alternative to having a max depth
	readonly int minSize;

	/// <summary>
	/// Constructor for the point octree.
	/// </summary>
	/// <param name="initialWorldSize">Size of the sides of the initial node. The octree will never shrink smaller than this.</param>
	/// <param name="initialWorldPos">Position of the centre of the initial node.</param>
	/// <param name="minNodeSize">Nodes will stop splitting if the new nodes would be smaller than this.</param>
	public TerrainOctree(int initialWorldSize, Vector3Int initialWorldPos, int minNodeSize) {
		if (minNodeSize > initialWorldSize) {
			Debug.LogWarning("Minimum node size must be at least as big as the initial world size. Was: " + minNodeSize + " Adjusted to: " + initialWorldSize);
			minNodeSize = initialWorldSize;
		}
		initialSize = initialWorldSize;
		minSize = minNodeSize;
		rootNode = new TerrainOctreeNode(initialSize, minSize, initialWorldPos);
	}

	// #### PUBLIC METHODS ####

	public void Evaluate(
			List<TerrainObject> terrainObjects,
			HashSet<TerrainOctreeNode> newLeaves,
			HashSet<TerrainOctreeNode> trimmedLeaves)
    {
		rootNode.Evaluate(terrainObjects, newLeaves, trimmedLeaves);
    }

	public HashSet<TerrainOctreeNode> GetAllLeafNodes() {
		var objects = new HashSet<TerrainOctreeNode>();
		rootNode.GetAllLeafNodes(objects);
		return objects;
	}

	/// <summary>
	/// Draws node boundaries visually for debugging.
	/// Must be called from OnDrawGizmos externally. See also: DrawAllObjects.
	/// </summary>
	public void DrawAllBounds() {
		rootNode.DrawAllBounds();
	}
}
