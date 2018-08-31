using System;
using UnityEditor;
using UnityEngine;

public class Connection
{
    public ConnectionPoint inPoint;
    public ConnectionPoint outPoint;
    public ConnectionPoint listenerPoint;
    public Action<Connection> OnClickRemoveConnection;

    public Connection(ConnectionPoint inPoint, ConnectionPoint outPoint,ConnectionPoint  lPoint,Action<Connection> OnClickRemoveConnection)
    {
        this.inPoint = inPoint;
        this.outPoint = outPoint;
        this.listenerPoint = lPoint;
        this.OnClickRemoveConnection = OnClickRemoveConnection;
    }

    public void Draw()
    {
        if (inPoint != null&&outPoint!=null)
        {
            Handles.DrawBezier(
                inPoint.rect.center,
                outPoint.rect.center,
                inPoint.rect.center + Vector2.left * 50f,
                outPoint.rect.center - Vector2.left * 50f,
                Color.white,
                null,
                2f
            );
            inPoint.note.Noise.noiseChild = outPoint.note.Noise;
            outPoint.note.Noise.noiseParent = inPoint.note.Noise;
            inPoint.restrictIntake = true;

            if (Handles.Button((inPoint.rect.center + outPoint.rect.center) * 0.5f, Quaternion.identity, 4, 8, Handles.RectangleCap))
            {
                if (OnClickRemoveConnection != null)
                {
                    OnClickRemoveConnection(this);
                    inPoint.restrictIntake = false;
                    inPoint.note.Noise.noiseChild = outPoint.note.Noise.noiseParent= null;
                }
            }
        }
        if (listenerPoint !=null&&outPoint!=null)
        {
            Handles.DrawBezier(
                listenerPoint.rect.center,
                outPoint.rect.center,
                listenerPoint.rect.center + Vector2.left * 50,
                outPoint.rect.center - Vector2.left * 50,
                Color.white,
                null,
                2f
            );
            listenerPoint.listenerTemplate.noiseToProcess = outPoint.note.Noise;
            listenerPoint.restrictIntake = true;

            if (Handles.Button((listenerPoint.rect.center*1.0f), Quaternion.identity, 4, 8, Handles.RectangleCap))
            {
                if (OnClickRemoveConnection!=null)
                {
                    OnClickRemoveConnection(this);
                    listenerPoint.restrictIntake = false;
                    listenerPoint.listenerTemplate.noiseToProcess = null;
                }
            }
        }
    }
}
