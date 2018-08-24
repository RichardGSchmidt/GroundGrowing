using System;
using UnityEngine;

public enum ConnectionPointType { In, Out }
///end goal { noiseIn, noiseOut,  FilterLink, perlinOnly}

public class ConnectionPoint
{
    public Rect rect;

    public ConnectionPointType type;

    public Note note;
    public EditorEngine eng;

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

    public ConnectionPoint(EditorEngine _engine,ConnectionPointType type, GUIStyle style, Action<ConnectionPoint> OnClickConnectionPoint)
    {
        this.eng = _engine;
        this.type = type;
        this.style = style;
        this.OnClickConnectionPoint = OnClickConnectionPoint;
        rect = new Rect(0, 0, 10f, 20f);
    }

    public void Draw()
    {
        if (note != null)
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
            }
        }

        else if (eng != null)
        {
            rect.y = eng.rect.y + (eng.rect.height * 0.5f) - rect.height * 0.5f;

            switch (type)
            {
                case ConnectionPointType.In:
                    rect.x = eng.rect.x - rect.width + 8f;
                    break;

                case ConnectionPointType.Out:
                    rect.x = eng.rect.x + note.rect.width - 8f;
                    break;
            }
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