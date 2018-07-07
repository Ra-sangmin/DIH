using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AtmosphereRenderer))]
public class AtmosphereRenderer_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Update Bounds"))
        {
            ((AtmosphereRenderer)target).UpdateBounds();
        }
    }
}
