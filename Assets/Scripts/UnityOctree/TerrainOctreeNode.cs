using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// A node in a PointOctree
// Copyright 2014 Nition, BSD licence (see LICENCE file). www.momentstudio.co.nz
public class TerrainOctreeNode {
	// Centre of this node
	public Vector3Int Center { get; private set; }

	// Length of the sides of this node
	public int SideLength { get; private set; }

	// Minimum size for a node in this octree
	int minSize;

	// Bounding box that represents this node
	Bounds bounds = default(Bounds);

	// Child nodes, if any
	TerrainOctreeNode[] children = null;

	bool HasChildren { get { return children != null; } }

	// bounds of potential children to this node. These are actual size (with looseness taken into account), not base size
	Bounds[] childBounds;

	// For reverting the bounds size after temporary changes
	Vector3Int actualBoundsSize;

	/// <summary>
	/// Constructor.
	/// </summary>
	/// <param name="baseLengthVal">Length of this node, not taking looseness into account.</param>
	/// <param name="minSizeVal">Minimum size of nodes in this octree.</param>
	/// <param name="centerVal">Centre position of this node.</param>
	public TerrainOctreeNode(int baseLengthVal, int minSizeVal, Vector3Int centerVal) {
		SetValues(baseLengthVal, minSizeVal, centerVal);
	}

	// #### PUBLIC METHODS ####
	//
	
	public void Evaluate(TerrainObject viewer, List<TerrainObject> additionalObjects)
	{
		float minDist = viewer.GetDistanceTo(Center);

		if (additionalObjects.Count > 0)
			minDist = Mathf.Min(minDist, additionalObjects.Min(obj => obj.GetDistanceTo(Center)));

		bool shouldSplit = (minDist < SideLength && SideLength > minSize);

		if (shouldSplit)
		{
			if (!HasChildren)
				Split();

			foreach (var child in children)
			{
				child.Evaluate(viewer, additionalObjects);
			}
		}
		else if (HasChildren)
		{
			Merge();
		}
	}

	public void GetAllLeafNodes(HashSet<TerrainOctreeNode> result)
	{
		if (children == null) {
			result.Add(this);
		}
		else
		{
			for (int i = 0; i < 8; i++) {
				children[i].GetAllLeafNodes(result);
			}
		}
	}

	/// <summary>
	/// Set the 8 children of this octree.
	/// </summary>
	/// <param name="childOctrees">The 8 new child nodes.</param>
	public void SetChildren(TerrainOctreeNode[] childOctrees) {
		if (childOctrees.Length != 8) {
			Debug.LogError("Child octree array must be length 8. Was length: " + childOctrees.Length);
			return;
		}

		children = childOctrees;
	}

	/// <summary>
	/// Draws node boundaries visually for debugging.
	/// Must be called from OnDrawGizmos externally. See also: DrawAllObjects.
	/// </summary>
	/// <param name="depth">Used for recurcive calls to this method.</param>
	public void DrawAllBounds(int depth = 0) {
		int tintVal = depth / 7; // Will eventually get values > 1. Color rounds to 1 automatically
		Gizmos.color = new Color(tintVal, 0, 1.0f - tintVal);

		Bounds thisBounds = new Bounds(Center, new Vector3Int(SideLength, SideLength, SideLength));
		Gizmos.DrawWireCube(thisBounds.center, thisBounds.size);

		if (children != null) {
			depth++;
			for (int i = 0; i < 8; i++) {
				children[i].DrawAllBounds(depth);
			}
		}
		Gizmos.color = Color.white;
	}

	/// <summary>
	/// Find which child node this object would be most likely to fit in.
	/// </summary>
	/// <param name="objPos">The object's position.</param>
	/// <returns>One of the eight child octants.</returns>
	public int BestFitChild(Vector3Int objPos) {
		return (objPos.x <= Center.x ? 0 : 1) + (objPos.y >= Center.y ? 0 : 4) + (objPos.z <= Center.z ? 0 : 2);
	}

	public bool Equals(TerrainOctreeNode n1, TerrainOctreeNode n2)
	{
		if (n1 == null || n2 == null)
			return (n1 == null && n2 == null);

		return (n1.Center == n2.Center);
	}

	public int GetHashCode(TerrainOctreeNode n1)
	{
		return n1.Center.GetHashCode();
	}

    // #### PRIVATE METHODS ####

    /// <summary>
    /// Set values for this node. 
    /// </summary>
    /// <param name="baseLengthVal">Length of this node, not taking looseness into account.</param>
    /// <param name="minSizeVal">Minimum size of nodes in this octree.</param>
    /// <param name="centerVal">Centre position of this node.</param>
    void SetValues(int baseLengthVal, int minSizeVal, Vector3Int centerVal) {
		SideLength = baseLengthVal;
		minSize = minSizeVal;
		Center = centerVal;

		// Create the bounding box.
		actualBoundsSize = new Vector3Int(SideLength, SideLength, SideLength);
		bounds = new Bounds(Center, actualBoundsSize);

		float quarter = SideLength / 4f;
		float childActualLength = SideLength / 2;
		Vector3 childActualSize = new Vector3(childActualLength, childActualLength, childActualLength);
		childBounds = new Bounds[8];
		childBounds[0] = new Bounds(Center + new Vector3(-quarter, quarter, -quarter), childActualSize);
		childBounds[1] = new Bounds(Center + new Vector3(quarter, quarter, -quarter), childActualSize);
		childBounds[2] = new Bounds(Center + new Vector3(-quarter, quarter, quarter), childActualSize);
		childBounds[3] = new Bounds(Center + new Vector3(quarter, quarter, quarter), childActualSize);
		childBounds[4] = new Bounds(Center + new Vector3(-quarter, -quarter, -quarter), childActualSize);
		childBounds[5] = new Bounds(Center + new Vector3(quarter, -quarter, -quarter), childActualSize);
		childBounds[6] = new Bounds(Center + new Vector3(-quarter, -quarter, quarter), childActualSize);
		childBounds[7] = new Bounds(Center + new Vector3(quarter, -quarter, quarter), childActualSize);
	}

	/// <summary>
	/// Splits the octree into eight children.
	/// </summary>
	void Split() {
		int quarter = SideLength / 4;
		int newLength = SideLength / 2;
		children = new TerrainOctreeNode[8];
		children[0] = new TerrainOctreeNode(newLength, minSize, Center + new Vector3Int(-quarter, quarter, -quarter));
		children[1] = new TerrainOctreeNode(newLength, minSize, Center + new Vector3Int(quarter, quarter, -quarter));
		children[2] = new TerrainOctreeNode(newLength, minSize, Center + new Vector3Int(-quarter, quarter, quarter));
		children[3] = new TerrainOctreeNode(newLength, minSize, Center + new Vector3Int(quarter, quarter, quarter));
		children[4] = new TerrainOctreeNode(newLength, minSize, Center + new Vector3Int(-quarter, -quarter, -quarter));
		children[5] = new TerrainOctreeNode(newLength, minSize, Center + new Vector3Int(quarter, -quarter, -quarter));
		children[6] = new TerrainOctreeNode(newLength, minSize, Center + new Vector3Int(-quarter, -quarter, quarter));
		children[7] = new TerrainOctreeNode(newLength, minSize, Center + new Vector3Int(quarter, -quarter, quarter));
	}

	/// <summary>
	/// Merge all children into this node - the opposite of Split.
	/// Note: We only have to check one level down since a merge will never happen if the children already have children,
	/// since THAT won't happen unless there are already too many objects to merge.
	/// </summary>
	void Merge() {
		children = null;
	}

	/// <summary>
	/// Checks if outerBounds encapsulates the given point.
	/// </summary>
	/// <param name="outerBounds">Outer bounds.</param>
	/// <param name="point">Point.</param>
	/// <returns>True if innerBounds is fully encapsulated by outerBounds.</returns>
	static bool Encapsulates(Bounds outerBounds, Vector3Int point) {
		return outerBounds.Contains(point);
	}

	/// <summary>
	/// Returns the closest distance to the given ray from a point.
	/// </summary>
	/// <param name="ray">The ray.</param>
	/// <param name="point">The point to check distance from the ray.</param>
	/// <returns>Squared distance from the point to the closest point of the ray.</returns>
	public static float SqrDistanceToRay(Ray ray, Vector3Int point) {
		return Vector3.Cross(ray.direction, point - ray.origin).sqrMagnitude;
	}
}

public class TerrainOctreeNodeEqualityComparer : IEqualityComparer<TerrainOctreeNode>
{
	public bool Equals(TerrainOctreeNode n1, TerrainOctreeNode n2)
	{
		if (n1 == null || n2 == null)
			return (n1 == null && n2 == null);

		return (n1.Center == n2.Center);
	}

	public int GetHashCode(TerrainOctreeNode obj)
	{
		return obj.Center.GetHashCode();
	}
}
