﻿using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainLoader))]
public class TerrainLoaderInspector : Editor
{
	override public void OnInspectorGUI()
	{
		TerrainLoader loader = target as TerrainLoader;

		DrawDefaultInspector();

		if (GUILayout.Button("Remesh All"))
			loader.UpdateAll();

		if (GUILayout.Button("Update Distant"))
			loader.UpdateDistantTerrain(true);
		
	}
}
