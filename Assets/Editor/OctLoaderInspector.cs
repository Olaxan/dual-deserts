using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(OctLoaderTest))]
public class OctLoaderInspector : Editor
{
    override public void OnInspectorGUI()
    {
        OctLoaderTest loader = target as OctLoaderTest;

        DrawDefaultInspector();

        if (GUILayout.Button("Update Terrain"))
            loader.UpdateChunks();
    }
}
