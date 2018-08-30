using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Listener
{

    public MapGenerator mapLink;
    public Rect rect;
    public string title;
    public bool isDragged;
    public bool isSelected;

    public ConnectionPoint inPoint;

    public GUIStyle style;
    public GUIStyle defaultListenerStyle;
    public GUIStyle selectedListenerStyle;

    public Action<Listener> OnConnection;
    public Action<Listener> OnGenerate;

    public Listener(
        Vector2 position,
        float width,
        float height,
        GUIStyle listenStyle,
        GUIStyle selectedStyle,
        GUIStyle inPointStyle,
        Action<ConnectionPoint> onClickInPoint,
        Action<Listener> GenMap,
        MapGenerator mapInput
        )
    {
        mapLink = mapInput;

        rect = new Rect(position.x, position.y, width, height);
        style = listenStyle;
        defaultListenerStyle = listenStyle;
        selectedListenerStyle = selectedStyle;
        inPoint = new ConnectionPoint(this, ConnectionPointType.In, inPointStyle, onClickInPoint);
        ;
      
    }

    public void Drag(Vector2 delta)
    {
        rect.position += delta;
    }

    public void Draw()
    {
        inPoint.Draw();
        GUI.Box(rect, title, style);
    }

    public void GenerateMap()
    {

    }
}
