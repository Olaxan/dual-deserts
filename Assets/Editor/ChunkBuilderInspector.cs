using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CSContourGenerator))]
public class ChunkBuilderInspector : Editor
{
	override public void OnInspectorGUI()
	{
		CSContourGenerator generator = target as CSContourGenerator;

		DrawDefaultInspector();

		//if (GUILayout.Button("Remesh All"))
		//{
		//	generator.UpdateAll();
		//}
		
	}
}
