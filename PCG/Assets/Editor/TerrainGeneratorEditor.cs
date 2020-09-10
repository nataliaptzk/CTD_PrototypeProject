using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TerrainGenerator script = (TerrainGenerator) target;

        if (DrawDefaultInspector())
        {
            // script.Initiate();
        }

        if (GUILayout.Button("Initiate"))
        {
            script.Initiate();
        }

        if (GUILayout.Button("Revaluate"))
        {
            script.Revaluate();
        }
        
        if (GUILayout.Button("Dummy Amounts"))
        {
            script.DummyAmounts();
        }

        if (GUILayout.Button("Save Mesh as Asset"))
        {
            script.SaveMesh();
        }
    }
}