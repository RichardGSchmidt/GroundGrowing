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
    public NoiseFunction noiseToProcess;
    public NoiseFunction lastProcessed;

    public ConnectionPoint listenPoint;

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
        GUIStyle listenPointStyle,
        Action<ConnectionPoint> onClickListenPoint,
        MapGenerator mapInput
        )
    {
        mapLink = mapInput;

        rect = new Rect(position.x, position.y, width, height);
        style = listenStyle;
        listenPoint = new ConnectionPoint(this,ConnectionPointType.Listener,listenPointStyle,onClickListenPoint);
        defaultListenerStyle = listenStyle;
        selectedListenerStyle = selectedStyle;
        
    }

    public void Drag(Vector2 delta)
    {
        rect.position += delta;
    }

    public void Draw()
    {
        
        listenPoint.Draw(this);
        GUI.Box(rect, title, style);
        GetInspectorElements();
    }

    public bool ProcessEvents(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    if (rect.Contains(e.mousePosition))
                    {
                        isDragged = true;
                        GUI.changed = true;
                        isSelected = true;
                        style = selectedListenerStyle;
                    }
                    else
                    {
                        GUI.changed = true;
                        isSelected = false;
                        style = defaultListenerStyle;

                    }
                }

                if (e.button == 1 && isSelected && rect.Contains(e.mousePosition))
                {
                    ProcessContextMenu();
                    e.Use();
                }

                break;

            case EventType.MouseUp:
                isDragged = false;
                break;

            case EventType.MouseDrag:
                if (e.button == 0 && isDragged)
                {
                    Drag(e.delta);
                    e.Use();
                    return true;
                }
                break;
        }
        return false;
    }

    public void ProcessContextMenu()
    {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddDisabledItem(new GUIContent("Generate"));
        genericMenu.ShowAsContext();
    }

    private void OnClickGenerateMap()
    {
        if (OnGenerate != null)
        {
            OnGenerate(this);
        }
    }

    private void GetInspectorElements()
    {
        const float yInterval = 15f;
        EditorGUI.LabelField(new Rect(rect.x + 50f, rect.y+yInterval, 200f, 20f),"Noise Listener");
        if(GUI.Button(new Rect(rect.x+50f,rect.y+yInterval*2, 100f, 20f), "Generate Map"))
        {
            if (noiseToProcess != null)
            {
                GenerateMap();
            }
        }

    }

    public void GenerateMap()
    {
        mapLink.GroundGrowing(noiseToProcess);
    }
}
