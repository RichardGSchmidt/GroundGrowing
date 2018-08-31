

///    [MenuItem("NGUI/Noise Designer",false,120)]
///    public static void ShowWindow()
///    {
///        EditorWindow.GetWindow(typeof(ContraOctave));
///    }
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ContraOctave : EditorWindow
{
    private List<Note> notes;
    private List<Connection> connections;
    private List<FilterNode> filters;
    private Listener coListener;

    private MapGenerator coMapGen;

    private GUIStyle nodeStyle;
    private GUIStyle selectedNodeStyle;
    private GUIStyle inPointStyle;
    private GUIStyle listenerPointStyle;
    private GUIStyle outPointStyle;

    private ConnectionPoint selectedInPoint;
    private ConnectionPoint selectedOutPoint;
    private ConnectionPoint selectedListenPoint;

    private bool ListenerConnected;

    private Vector2 offset;
    private Vector2 drag;

    [MenuItem("Window/Note Based Editor")]
    public static void ShowWindow()
    {
        ContraOctave window = GetWindow<ContraOctave>();
        window.titleContent = new GUIContent("Note Based Editor");
    }

    private void OnEnable()
    {
        nodeStyle = new GUIStyle();
        nodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
        nodeStyle.border = new RectOffset(12, 12, 12, 12);

        selectedNodeStyle = new GUIStyle();
        selectedNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;
        selectedNodeStyle.border = new RectOffset(12, 12, 12, 12);

        inPointStyle = new GUIStyle();
        inPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left.png") as Texture2D;
        inPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left on.png") as Texture2D;
        inPointStyle.border = new RectOffset(4, 4, 12, 12);

        listenerPointStyle = new GUIStyle();
        listenerPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left.png") as Texture2D;
        listenerPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left on.png") as Texture2D;
        listenerPointStyle.border = new RectOffset(4, 4, 12, 12);

        outPointStyle = new GUIStyle();
        outPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right.png") as Texture2D;
        outPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right on.png") as Texture2D;
        outPointStyle.border = new RectOffset(4, 4, 12, 12);
    }

    private void OnGUI()
    {
        if (coMapGen == null)
        {
            coMapGen = FindObjectOfType<MapGenerator>();
        }

        DrawGrid(20, 0.2f, Color.gray);
        DrawGrid(100, 0.4f, Color.gray);

        DrawListener();
        DrawNodes();
        DrawFilters();
        DrawConnections();

        DrawConnectionLine(Event.current);

        ProcessNodeEvents(Event.current);
        ProcessListenerEvents(Event.current);
        ProcessFilterEvents(Event.current);
        ProcessEvents(Event.current);

        if (GUI.changed) Repaint();
    }

    private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
    {
        int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
        int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

        Handles.BeginGUI();
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        offset += drag * 0.5f;
        Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

        for (int i = 0; i < widthDivs; i++)
        {
            Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
        }

        for (int j = 0; j < heightDivs; j++)
        {
            Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
        }

        Handles.color = Color.white;
        Handles.EndGUI();
    }

    private void DrawNodes()
    {
        if (notes != null)
        {
            for (int i = 0; i < notes.Count; i++)
            {
                notes[i].Draw();
            }
        }
    }

    private void DrawFilters()
    {
        if (filters != null)
        {
            for (int i = 0; i < filters.Count; i++)
            {
                filters[i].Draw();
            }
        }
    }

    private void DrawListener()
    {
        if (coListener !=null)
        {
            coListener.Draw();
        }
    }

    private void DrawConnections()
    {
        if (connections != null)
        {
            for (int i = 0; i < connections.Count; i++)
            {
                connections[i].Draw();
            }
        }
    }

    private void ProcessEvents(Event e)
    {
        drag = Vector2.zero;

        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    ClearConnectionSelection();
                }

                if (e.button == 1)
                {
                    ProcessContextMenu(e.mousePosition);
                }
                break;

            case EventType.MouseDrag:
                if (e.button == 0)
                {
                    OnDrag(e.delta);
                }
                break;
        }
    }

    private void ProcessNodeEvents(Event e)
    {
        if (notes != null)
        {
            for (int i = notes.Count - 1; i >= 0; i--)
            {
                bool guiChanged = notes[i].ProcessEvents(e);

                if (guiChanged)
                {
                    GUI.changed = true;
                }
            }
        }
    }

    private void ProcessFilterEvents(Event e)
    {
        if (filters!=null)
        {
            for (int i = filters.Count - 1; i >=0; i--)
            {
                bool guiChanged = filters[i].ProcessEvents(e);

                if (guiChanged)
                {
                    GUI.changed = true;
                }
            }
        }
    }

    private void ProcessListenerEvents(Event e)
    {
        if (coListener!=null)
        {
            bool guiChanged = coListener.ProcessEvents(e);
            if (guiChanged)
            {
                GUI.changed = true;
            }
        }
    }

    private void DrawConnectionLine(Event e)
    {
        if (selectedInPoint != null && selectedOutPoint == null)
        {
            Handles.DrawBezier(
                selectedInPoint.rect.center,
                e.mousePosition,
                selectedInPoint.rect.center + Vector2.left * 50f,
                e.mousePosition - Vector2.left * 50f,
                Color.white,
                null,
                2f
            );

            

            GUI.changed = true;
        }

        if (selectedOutPoint != null && selectedInPoint == null)
        {
            Handles.DrawBezier(
                selectedOutPoint.rect.center,
                e.mousePosition,
                selectedOutPoint.rect.center - Vector2.left * 50f,
                e.mousePosition + Vector2.left * 50f,
                Color.white,
                null,
                2f
            );

            GUI.changed = true;
        }

        if(selectedListenPoint!=null&&selectedOutPoint==null)
        {
            Handles.DrawBezier(
                selectedListenPoint.rect.center,
                e.mousePosition,
                selectedListenPoint.rect.center - Vector2.left * 50f,
                e.mousePosition + Vector2.left * 50f,
                Color.white,
                null,
                2f
            );
        }
        if(selectedOutPoint!=null&&selectedListenPoint==null)
        {
            Handles.DrawBezier(
                selectedOutPoint.rect.center,
                e.mousePosition,
                selectedOutPoint.rect.center - Vector2.left * 50f,
                e.mousePosition + Vector2.left * 50f,
                Color.white,
                null,
                2f
            );
        }
    }

    private void ProcessContextMenu(Vector2 mousePosition)
    {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Add Noise"), false, () => OnClickAddNode(mousePosition));
        genericMenu.AddItem(new GUIContent("Add Listener"), false, () => OnClickAddListener(mousePosition));
        AddFilterMenuItem(genericMenu, "Filters/Clamp", new ClampFilter(), mousePosition);
        AddFilterMenuItem(genericMenu, "Filters/Scale", new ScaleFilter(), mousePosition);
        AddFilterMenuItem(genericMenu, "Filters/Translate", new TranslateFilter(), mousePosition);
        AddFilterMenuItem(genericMenu, "Filters/Rotate", new RotateFilter(), mousePosition);
        AddFilterMenuItem(genericMenu, "Filters/Scale Bias", new ScaleBiasFilter(), mousePosition);
        AddFilterMenuItem(genericMenu, "Filters/Invert", new InvertFilter(), mousePosition);
        AddFilterMenuItem(genericMenu, "Filters/ABS", new ABSFilter(), mousePosition);
        AddFilterMenuItem(genericMenu, "Filters/Terrace", new TerraceFilter(), mousePosition);
        AddFilterMenuItem(genericMenu, "Filters/Turbulence", new TurbulenceFilter(), mousePosition);
        AddFilterMenuItem(genericMenu, "Filters/Exponent", new ExponentFilter(), mousePosition);
        genericMenu.ShowAsContext();
    }

    private void AddFilterMenuItem(GenericMenu menu, string menuPath, NoiseFilter _filter, Vector2 mousePosition)
    {
        menu.AddItem(new GUIContent(menuPath),false,()=> OnClickAddFilter(mousePosition,_filter));
    }



    private void OnDrag(Vector2 delta)
    {
        drag = delta;

        if (notes != null)
        {
            for (int i = 0; i < notes.Count; i++)
            {
                notes[i].Drag(delta);
            }
        }

        if(coListener!=null)
        {
            coListener.Drag(delta);
        }

        GUI.changed = true;
    }

    private void OnGenerate(MapGenerator mapGenInput)
    {
        if (mapGenInput != null)
        {
            mapGenInput.GroundGrowing();
        }
        else return;
    }

    private void OnClickAddListener(Vector2 mousePostion)
    {
        coListener = new Listener(mousePostion, 200, 300, nodeStyle, selectedNodeStyle, inPointStyle, OnClickListenerPoint, coMapGen);
    }

    private void OnClickAddNode(Vector2 mousePosition)
    {
        if (notes == null)
        {
            notes = new List<Note>();
        }

        notes.Add(new Note(mousePosition, 200, 50, nodeStyle, selectedNodeStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode));
    }

    private void OnClickAddFilter(Vector2 mousePosition, NoiseFilter filterIn)
    {
        if (filters == null)
        {
            filters = new List<FilterNode>();
        }
        filters.Add( new FilterNode(mousePosition, 200, 50, nodeStyle, selectedNodeStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint,OnClickRemoveNode ,OnClickRemoveFilter, filterIn));
    }

    private void OnClickInPoint(ConnectionPoint inPoint)
    {
        selectedInPoint = inPoint;

        if (selectedOutPoint != null)
        {
            if (selectedOutPoint.note != selectedInPoint.note)
            {
                CreateConnection();
                ClearConnectionSelection();
            }
            else
            {
                ClearConnectionSelection();
            }
        }
    }

    private void OnClickListenerPoint(ConnectionPoint lPoint)
    {
        selectedListenPoint = lPoint;
        if(selectedOutPoint!=null)
        {
            CreateConnection();
            ClearConnectionSelection();
        }
    }

    private void OnClickOutPoint(ConnectionPoint outPoint)
    {
        selectedOutPoint = outPoint;

        if (selectedInPoint != null)
        {
            if (selectedOutPoint.note != selectedInPoint.note)
            {
                CreateConnection();
                ClearConnectionSelection();
            }
            else
            {
                ClearConnectionSelection();
            }
        }
    }

    private void OnClickRemoveFilter(FilterNode filter)
    {
        if (connections != null)
        {
            List<Connection> connectionsToRemove = new List<Connection>();

            for (int i = 0; i < connections.Count; i++)
            {
                if (connections[i].inPoint == filter.inPoint || connections[i].outPoint == filter.outPoint)
                {
                    connectionsToRemove.Add(connections[i]);
                }
            }

            for (int i = 0; i < connectionsToRemove.Count; i++)
            {
                connections.Remove(connectionsToRemove[i]);
            }

            connectionsToRemove = null;
        }

        filters.Remove(filter);
    }

    private void OnClickRemoveNode(Note note)
    {
        if (connections != null)
        {
            List<Connection> connectionsToRemove = new List<Connection>();

            for (int i = 0; i < connections.Count; i++)
            {
                if (connections[i].inPoint == note.inPoint || connections[i].outPoint == note.outPoint)
                {
                    connectionsToRemove.Add(connections[i]);
                }
            }

            for (int i = 0; i < connectionsToRemove.Count; i++)
            {
                connections.Remove(connectionsToRemove[i]);
            }

            connectionsToRemove = null;
        }
        if (note != null&&notes!=null)
        {
            notes.Remove(note);
        }
        
    }

    private void OnClickRemoveConnection(Connection connection)
    {
        connections.Remove(connection);
    }

    private void CreateConnection()
    {
        if (connections == null)
        {
            connections = new List<Connection>();
        }

        connections.Add(new Connection(selectedInPoint, selectedOutPoint, selectedListenPoint, OnClickRemoveConnection));
    }

    private void ClearConnectionSelection()
    {
        selectedInPoint = null;
        selectedOutPoint = null;
        selectedListenPoint = null;
    }
}