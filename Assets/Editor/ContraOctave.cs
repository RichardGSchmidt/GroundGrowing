using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ContraOctave : EditorWindow {
    private List<Note> notes;

    private GUIStyle NoteStyle;

    [MenuItem("NGUI/Noise Designer",false,120)]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(ContraOctave));
    }

    private void OnEnable()
    {
        NoteStyle = new GUIStyle();
        NoteStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;

    }


    private void OnGUI()
    {
        DrawNotes();
        ProcessNoteEvents(Event.current);
        ProcessEvents(Event.current);
        MapGenerator mapGen = FindObjectOfType<MapGenerator>();
        MapGenEditor mGenEditor = (FindObjectOfType<MapGenEditor>());

    }
    private void DrawNotes()
    {
        if (notes != null)
        {
            for(int i = 0; i < notes.Count; i++)
            {
                notes[i].Draw();
            }
        }

    }
    private void ProcessEvents(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 1)
                {
                    ProcessContextMenu(e.mousePosition);
                }
                break;
        }
    }

    private void ProcessNoteEvents(Event e)
    {
        if (notes != null)
        {
            for (int i = notes.Count - 1; i >= 0; i--)
            {
                bool guiChanged = notes[i].ProcessEvents(e);

                if(guiChanged)
                {
                    GUI.changed = true;
                }
            }
        }
    }

    private void ProcessContextMenu(Vector2 mousePosition)
    {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Add Note"),false,()=>OnClickAddNote(mousePosition));
        genericMenu.ShowAsContext();
    }

    private void OnClickAddNote(Vector2 _mousePosition)
    {
        if (notes == null)
        {
            notes = new List<Note>();
        }

        notes.Add(new Note(_mousePosition, 200, 50, NoteStyle));
    }
}
