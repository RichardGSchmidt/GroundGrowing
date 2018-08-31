using System;
using UnityEngine;
using UnityEditor;

public class FilterNode : Note
{
    public NoiseFilter noiseFilter;

    public Action<FilterNode> OnRemoveFilter;

    public FilterNode(
        Vector2 position,
        float width,
        float height,
        GUIStyle noteStyle,
        GUIStyle selectedStyle,
        GUIStyle inPointStyle,
        GUIStyle outPointStyle,
        Action<ConnectionPoint> OnClickInPoint,
        Action<ConnectionPoint> OnClickOutPoint,
        Action<Note> OnClickRemoveNote,
        Action<FilterNode> OnClickRemoveFilter,
        NoiseFilter inFilter

        ):base(
        position,
        width,
        height,
        noteStyle,
        selectedStyle,
        inPointStyle,
        outPointStyle,
        OnClickInPoint,
        OnClickOutPoint,
        OnClickRemoveNote
            )
    {
        noiseFilter = inFilter;
        OnRemoveNote = OnClickRemoveNote;
        OnRemoveFilter = OnClickRemoveFilter;
    }


    public override void Draw()
    {
        inPoint.Draw();
        outPoint.Draw();
        GUI.Box(rect, title, style);
        GetInspectorElements(noiseFilter,ref rect);
    }

    public override void ProcessContextMenu()
    {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Remove Filter"), false, OnClickRemoveNote);
        genericMenu.ShowAsContext();
    }

    public override void OnClickRemoveNote()
    {
        
        if (OnRemoveFilter != null)
        {
            OnRemoveFilter(this);
        }
        

    }

    public void OnClickRemoveFilter()
    {
        if (OnRemoveFilter!=null)
        {
            OnRemoveFilter(this);
        }
    }

    public void GetInspectorElements(NoiseFilter _filter,ref Rect _rect)
    {
        _filter.GetInspectorElements(ref _rect);
    }

}