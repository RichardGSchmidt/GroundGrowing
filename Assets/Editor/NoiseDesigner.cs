using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NoiseDesigner : EditorWindow {
    [MenuItem("NGUI/Noise Designer",false,120)]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(NoiseDesigner));
    }


    private void OnGUI()
    {
        MapGenerator mapGen = FindObjectOfType<MapGenerator>();
        MapGenEditor mGenEditor = (FindObjectOfType<MapGenEditor>());

    }
}
