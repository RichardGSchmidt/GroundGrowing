using System;
using UnityEngine;

public enum ConnectionPointType { In, Out, Listener }
///end goal { noiseIn, noiseOut,  FilterLink, perlinOnly}

public class ConnectionPoint
{
    public Rect rect;

    public ConnectionPointType type;

    public Note note;

    public Listener listenerTemplate;

    public GUIStyle style;

    public Action<ConnectionPoint> OnClickConnectionPoint;

    public ConnectionPoint(Note note, ConnectionPointType type, GUIStyle style, Action<ConnectionPoint> OnClickConnectionPoint)
    {
        this.note = note;
        this.type = type;
        this.style = style;
        this.OnClickConnectionPoint = OnClickConnectionPoint;
        rect = new Rect(0, 0, 10f, 20f);
    }

    public ConnectionPoint(Listener listener, ConnectionPointType type, GUIStyle style, Action<ConnectionPoint>OnClickConnectionPoint)
    {
        this.listenerTemplate = listener;
        this.type = type;
        this.style = style;
        this.OnClickConnectionPoint = OnClickConnectionPoint;
        rect = new Rect(0, 0, 20f, 20f);

    }


    public void Draw()
    {
        rect.y = note.rect.y + (note.rect.height * 0.5f) - rect.height * 0.5f;

        switch (type)
        {
            case ConnectionPointType.In:
                rect.x = note.rect.x - rect.width + 8f;
                break;

            case ConnectionPointType.Out:
                rect.x = note.rect.x + note.rect.width - 8f;
                break;
            case ConnectionPointType.Listener:
                rect.x = listenerTemplate.rect.x - rect.width + 8f;
                break;
        }

        if (GUI.Button(rect, "", style))
        {
            if (OnClickConnectionPoint != null)
            {
                OnClickConnectionPoint(this);
            }
        }
    }

    public void Draw(Listener listInput)
    {
        rect.y = listInput.rect.y + (listInput.rect.height * 0.5f) - rect.height * 0.5f;

        switch (type)
        {
          
            case ConnectionPointType.Listener:
                rect.x = listenerTemplate.rect.x - rect.width + 8f;
                break;
        }

        if (GUI.Button(rect, "", style))
        {
            if (OnClickConnectionPoint != null)
            {
                OnClickConnectionPoint(this);
            }
        }
    }
}