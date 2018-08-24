using System;
using UnityEngine;
using UnityEditor;

public class EditorEngine:Note
{

    public MapGenerator MapGen { get; set; }
    public NoiseFunction LastProcessedNoise { get; set; }
    public NoiseFunction ConnectedNoise;

    public GUIStyle defaultEngStyle;
    public GUIStyle selStyle;

    public ConnectionPoint finalNode;

    public EditorEngine(
        Vector2 position,
        float width,
        float height,
        MapGenerator _mapGen,
        GUIStyle noteStyle,
        GUIStyle selectedStyle,
        GUIStyle inPointStyle,
        Action<ConnectionPoint> OnClickInPoint)
    {
        Noise = new NoiseFunction();
        NoiseFilters = null;
        MapGen = _mapGen;
        rect = new Rect(position.x, position.y, width, height);
        style = noteStyle;
        inPoint = new ConnectionPoint(this, ConnectionPointType.In, inPointStyle, OnClickInPoint);
        defaultNoteStyle = noteStyle;
        selectedNoteStyle = selectedStyle;
    }

    public override void Draw()
    {
        inPoint.Draw();
        GUI.Box(rect, title, style);

       if (GUI.Button(rect, "Generate")) {

            MapGen.GroundGrowing();
        }

    }
  


}

