using System;
using UnityEngine;
using UnityEditor;

public class Note
{
    /// <summary>
    /// The  noise function work is up front.
    /// 
    /// </summary>
    public NoiseFunction Noise { get; set; }
    public NoiseFilter[] NoiseFilters { get; set; }
    public Rect rect;
    public string title;
    public bool isDragged;
    public bool isSelected;

    public ConnectionPoint inPoint;
    public ConnectionPoint outPoint;

    


    public GUIStyle style;
    public GUIStyle defaultNoteStyle;
    public GUIStyle selectedNoteStyle;

    public Action<Note> OnRemoveNote;

    public Note(
        Vector2 position,
        float width,
        float height,
        GUIStyle noteStyle,
        GUIStyle selectedStyle,
        GUIStyle inPointStyle, 
        GUIStyle outPointStyle, 
        Action<ConnectionPoint> OnClickInPoint, 
        Action<ConnectionPoint> OnClickOutPoint,
        Action<Note> OnClickRemoveNote)
    {
        Noise = new NoiseFunction();
        NoiseFilters = null;

        rect = new Rect(position.x, position.y, width, height);
        style = noteStyle;
        inPoint = new ConnectionPoint(this, ConnectionPointType.In, inPointStyle, OnClickInPoint);
        outPoint = new ConnectionPoint(this, ConnectionPointType.Out, outPointStyle, OnClickOutPoint);
        defaultNoteStyle = noteStyle;
        selectedNoteStyle = selectedStyle;
        OnRemoveNote = OnClickRemoveNote;
    }

    public void Drag(Vector2 delta)
    {
        rect.position += delta;
    }

    public void Draw()
    {
        inPoint.Draw();
        outPoint.Draw();
        GUI.Box(rect, title, style);
        GetInspectorElements(Noise);
        
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
                        style = selectedNoteStyle;
                    }
                    else
                    {
                        GUI.changed = true;
                        isSelected = false;
                        style = defaultNoteStyle;
                        
                    }
                }

                if(e.button ==1 && isSelected && rect.Contains(e.mousePosition))
                {
                    ProcessContextMenu();
                    e.Use();
                }

                break;

            case EventType.MouseUp:
                isDragged = false;
                break;

            case EventType.MouseDrag:
                if(e.button==0&&isDragged)
                {
                    Drag(e.delta);
                    e.Use();
                    return true;
                }
                break;
        }
        return false;
    }

    private void ProcessContextMenu()
    {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Remove Noise"), false, OnClickRemoveNote);
        genericMenu.ShowAsContext();
    }

    private void OnClickRemoveNote()
    {
        if (OnRemoveNote!=null)
        {
            OnRemoveNote(this);
        }
    }

    public void GetInspectorElements(NoiseFunction _Noise)
    {
        #region Default window setup
        const float yInterval = 15f;
        Rect typeRect = new Rect(rect.x + 10f, rect.y + yInterval, 175f, 20f);
        Rect enabledRect = new Rect(rect.x + 350f, rect.y + 2*yInterval, 10f, 20f);
        Rect freqRect = new Rect(rect.x + 10f, rect.y + 3*yInterval, 400f, 20f);
        Rect lacRect = new Rect(rect.x + 10f, rect.y + 4*yInterval, 400f, 20f);
        Rect octRect = new Rect(rect.x + 10f, rect.y + 5*yInterval, 400f, 20f);
        Rect quaRect = new Rect(rect.x + 10f, rect.y + 10*yInterval, 400f, 20f);
        Rect bleRect = new Rect(rect.x+10f,rect.y+11*yInterval,400f,20f);
        Rect perRect = new Rect(rect.x + 10f, rect.y + 6*yInterval, 400f, 20f);
        rect = new Rect(rect.x, rect.y, 500f, 200f);
        #endregion

        #region Perlin Function UI
        if (_Noise.type == NoiseFunction.NoiseType.Perlin)
        {
            _Noise.type = (NoiseFunction.NoiseType)EditorGUI.EnumPopup(typeRect, _Noise.type);
            _Noise.enabled = EditorGUI.ToggleLeft(enabledRect, "Enabled", _Noise.enabled);
            _Noise.frequency = (double)EditorGUI.Slider(freqRect,"Frequency", (float)_Noise.frequency, 0f, 20f);
            _Noise.lacunarity = (double)EditorGUI.Slider(lacRect,"Lacunarity", (float)_Noise.lacunarity, 0f, 1.5000000f);
            _Noise.persistence = (double)EditorGUI.Slider(perRect,"Persistence", (float)_Noise.persistence, 0f, 1f);
            _Noise.octaves = EditorGUI.IntSlider(octRect, "Octaves", _Noise.octaves, 0, 18);
            _Noise.qualityMode = (LibNoise.QualityMode)EditorGUI.EnumPopup(quaRect, "Quality Mode", _Noise.qualityMode);
            _Noise.Blend = (NoiseFunction.BlendMode)EditorGUI.EnumPopup(bleRect, "Blend Mode", _Noise.Blend);
        }
        #endregion

        #region Billow Function UI
        else if (_Noise.type == NoiseFunction.NoiseType.Billow)
        {
            _Noise.type = (NoiseFunction.NoiseType)EditorGUI.EnumPopup(typeRect, _Noise.type);
            _Noise.enabled = EditorGUI.ToggleLeft(enabledRect,"Enabled", _Noise.enabled);
            _Noise.frequency = (double)EditorGUI.Slider(freqRect, "Frequency", (float)_Noise.frequency, 0f, 20f);
            _Noise.lacunarity = (double)EditorGUI.Slider(lacRect, "Lacunarity", (float)_Noise.lacunarity, 1.5f, 3.5f);
            _Noise.persistence = (double)EditorGUI.Slider(perRect,"Persistence", (float)_Noise.persistence, 0f, 1f);
            _Noise.octaves = EditorGUI.IntSlider(octRect, "Octaves", _Noise.octaves, 0, 18);
            _Noise.qualityMode = (LibNoise.QualityMode)EditorGUI.EnumPopup(quaRect, "Quality Mode", _Noise.qualityMode);
            _Noise.Blend = (NoiseFunction.BlendMode)EditorGUI.EnumPopup(bleRect, "Blend Mode", _Noise.Blend);
        }
        #endregion

        #region Voronoi UI
        else if (_Noise.type == NoiseFunction.NoiseType.Voronoi)
        {
            _Noise.type = (NoiseFunction.NoiseType)EditorGUI.EnumPopup(typeRect, _Noise.type);
            _Noise.enabled = EditorGUI.ToggleLeft(enabledRect,"Enabled", _Noise.enabled);
            _Noise.frequency = (double)EditorGUI.Slider(freqRect,"Frequency", (float)_Noise.frequency, 0f, 20f);
            _Noise.displacement = (double)EditorGUI.Slider(lacRect,"Displacement", (float)_Noise.displacement, 0f, 20f);
            _Noise.distance = EditorGUI.ToggleLeft(octRect,"Use Distance", _Noise.distance);
            _Noise.Blend = (NoiseFunction.BlendMode)EditorGUI.EnumPopup(bleRect,"Blend Mode", _Noise.Blend);

        }
        #endregion

        #region Ridged Multifractal UI
        else if (_Noise.type == NoiseFunction.NoiseType.RidgedMultifractal)
        {

            _Noise.type = (NoiseFunction.NoiseType)EditorGUI.EnumPopup(typeRect, _Noise.type);
            _Noise.enabled = EditorGUI.ToggleLeft(enabledRect,"Enabled", _Noise.enabled);
            _Noise.frequency = (double)EditorGUI.Slider(freqRect,"Frequency", (float)_Noise.frequency, 0f, 20f);
            _Noise.lacunarity = (double)EditorGUI.Slider(lacRect,"Lacunarity", (float)_Noise.lacunarity, 1.5000000f, 3.5000000f);
            _Noise.octaves = EditorGUI.IntSlider(octRect, "Octaves", _Noise.octaves, 0, 18);
            _Noise.qualityMode = (LibNoise.QualityMode)EditorGUI.EnumPopup(quaRect,"Quality Mode", _Noise.qualityMode);
            _Noise.Blend = (NoiseFunction.BlendMode)EditorGUI.EnumPopup(bleRect,"Blend Mode", _Noise.Blend);

        }
        #endregion

        #region None UI
        else if (_Noise.type == NoiseFunction.NoiseType.None)
        {
            _Noise.type = (NoiseFunction.NoiseType)EditorGUI.EnumPopup(typeRect, _Noise.type);

        }

        #endregion

    }
}


