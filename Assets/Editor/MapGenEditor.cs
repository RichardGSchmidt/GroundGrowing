﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof (MapGenerator))]
public class MapGenEditor : Editor {

    #region Variables
    public NoiseFunctions.NoiseType noiseType;
    public bool showNoiseFunctions;
    public bool showRegionGroups;
    public bool showTextures;
    private string fileName;
    public NoiseFunctions[] oldNoises;
    #endregion

    #region Initial Setup
    void OnEnable()
    {
        EditorApplication.update += Update;
    }

    static void Update()
    {
        MapGenerator mapGen = FindObjectOfType<MapGenerator>();
        if (mapGen.noiseMapUpdateAvailable)
        {
            mapGen.UpdateSphereMap();
        }
    }

    public override void OnInspectorGUI()
    {
        

         
        MapGenerator mapGen = (MapGenerator)target;
        
        if (DrawDefaultInspector())
        {
            if ((mapGen.autoUpdate)&&!(mapGen.multithreading))
            { mapGen.GenerateMap(); }
        }
        #endregion

        #region Error Control
        //error control to keep height positive and non zero
        if (mapGen.mapWidth < 1) mapGen.mapWidth = 1;
        if (mapGen.mapHeight < 1) mapGen.mapHeight = 1;
        #endregion

        #region Image Save Functionality

        if (GUILayout.Button("Save Image"))
        {
            fileName = EditorUtility.SaveFilePanel("Save a Copy of Texture", Application.dataPath, "mapimage", "png");
            mapGen.SaveImage(fileName);
        }
        #endregion
        TerrainFoldout(ref mapGen);
        NoiseFunctionFoldout(ref mapGen);      
        if (GUILayout.Button("Generate")){mapGen.GenerateMap();}

    }

    #region Inspector Elements

    #region Terrain Elements

    public void TerrainFoldout(ref MapGenerator _mapGen)
    {
        showRegionGroups = EditorGUILayout.Foldout(showRegionGroups, "Regions");
        if (showRegionGroups)
        {
            if (GUILayout.Button("Add New Region"))
            {
                TerrainType[] placeHolder = new TerrainType[_mapGen.regions.Length + 1];
                for (int k = 0; k < _mapGen.regions.Length; k++)
                {
                    placeHolder[k] = _mapGen.regions[k];
                }
                placeHolder[_mapGen.regions.Length] = new TerrainType();
                _mapGen.regions = new TerrainType[placeHolder.Length];
                for (int k = 0; k < _mapGen.regions.Length; k++)
                {
                    _mapGen.regions[k] = placeHolder[k];
                }
                return;
            }
            if (GUILayout.Button("Save Regions"))
            {
                fileName = EditorUtility.SaveFilePanel("Save Current Regions", Application.dataPath, "Regions", "tpr");
                _mapGen.SaveTerrain(_mapGen.regions, fileName);
            }
            if (GUILayout.Button("Load Regions From File"))
            {
                fileName = EditorUtility.OpenFilePanel("Load Regions from File ", null, "tpr");
                _mapGen.LoadTerrain(fileName);
                _mapGen.GenerateMap();
            }
            if (GUILayout.Button("Sort Regions By Height"))
            {
                System.Array.Sort(_mapGen.regions);
                return;
            }
        }

        for (int i = 0; i < _mapGen.regions.Length; i++)
        {
            if (showRegionGroups)
            {
                GetInspectorElements(_mapGen.regions[i], i, _mapGen);
            }

        }
    }

    public void GetInspectorElements(TerrainType terrainType, int index, MapGenerator generator)
    {
        EditorGUILayout.Space();
        terrainType.name = EditorGUILayout.TextField("Region Name", terrainType.name);
        terrainType.color = EditorGUILayout.ColorField(terrainType.color);
        terrainType.height = EditorGUILayout.Slider("Height", (float)terrainType.height, -1.80000f, 1.80000f);

        if (GUILayout.Button("Remove"))
        {
            generator.regions = generator.regions.RemoveAt(index);
        }
    }

    #endregion

    #region Noise Elements

    public void NoiseFunctionFoldout(ref MapGenerator _mapGen)
    {
        showNoiseFunctions = EditorGUILayout.Foldout(showNoiseFunctions, "Noise Stack");
        if (showNoiseFunctions)
        {
            //if (GUILayout.Button("Open Noise Designer")) { NoiseDesigner.ShowWindow(); }

            if (GUILayout.Button("Add New Noise Function"))
            {
                NoiseFunctions[] placeholder = new NoiseFunctions[_mapGen.noiseFunctions.Length + 1];
                for (int j = 0; j < _mapGen.noiseFunctions.Length; j++)
                {
                    placeholder[j] = _mapGen.noiseFunctions[j];
                }
                placeholder[_mapGen.noiseFunctions.Length] = new NoiseFunctions();
                _mapGen.noiseFunctions = new NoiseFunctions[placeholder.Length];
                for (int j = 0; j < placeholder.Length; j++)
                {
                    _mapGen.noiseFunctions[j] = placeholder[j];
                }
            }
            #region Save / Load Functions
            if (GUILayout.Button("Save This Noise Preset"))
            {
                fileName = EditorUtility.SaveFilePanel("Save a New Preset", Application.dataPath, "Noise Preset", "npr");
                _mapGen.SavePresets(_mapGen.noiseFunctions, fileName);
            }
            if (GUILayout.Button("Load Preset From File"))
            {
                fileName = EditorUtility.OpenFilePanel("Load a noise File ", null, "npr");
                _mapGen.LoadPresets(fileName);
                _mapGen.GenerateMap();
            }
            #endregion

            for (int i = 0; i < _mapGen.noiseFunctions.Length; i++)
            {
                if (showNoiseFunctions)
                {
                    GetInspectorElements(_mapGen.noiseFunctions[i], i, _mapGen);
                }

            }

        }
    }

    public void GetInspectorElements(NoiseFunctions noiseFunc, int index, MapGenerator generator)
    {
        //to autoupdate if this panel has been changed
        EditorGUI.BeginChangeCheck();

        #region Perlin Function UI
        if (noiseFunc.type == NoiseFunctions.NoiseType.Perlin)
        {
            EditorGUILayout.Space();
            string name = "Perlin Noise";
            EditorGUILayout.LabelField(name);
            noiseFunc.type = (NoiseFunctions.NoiseType)EditorGUILayout.EnumPopup("Type of Noise", noiseFunc.type);
            noiseFunc.enabled = EditorGUILayout.ToggleLeft("Enabled", noiseFunc.enabled);
            noiseFunc.frequency = (double)EditorGUILayout.Slider("Frequency",(float)noiseFunc.frequency, -20f, 20f);
            noiseFunc.lacunarity = (double)EditorGUILayout.Slider("Lacunarity", (float)noiseFunc.lacunarity, -2.0000000f, 2.5000000f);
            noiseFunc.persistence = (double)EditorGUILayout.Slider("Persistence", (float)noiseFunc.persistence, -1f, 1f);
            noiseFunc.octaves = EditorGUILayout.IntSlider("Octaves", noiseFunc.octaves, 0, 18);
            noiseFunc.qualityMode = (LibNoise.QualityMode)EditorGUILayout.EnumPopup("Quality Mode", noiseFunc.qualityMode);
            noiseFunc.blendMode = (NoiseFunctions.BlendMode)EditorGUILayout.EnumPopup("Blend Mode", noiseFunc.blendMode);
            if (GUILayout.Button("Remove"))
            {
                NoiseFunctions[] placeHolder = new NoiseFunctions[generator.noiseFunctions.Length - 1];
                placeHolder = generator.noiseFunctions.RemoveAt(index);
                generator.noiseFunctions = placeHolder;
            }
        }
        #endregion

        #region Billow Function UI
        else if (noiseFunc.type == NoiseFunctions.NoiseType.Billow)
        {
            EditorGUILayout.Space();
            string name = "Billow Noise";
            EditorGUILayout.LabelField(name);
            noiseFunc.type = (NoiseFunctions.NoiseType)EditorGUILayout.EnumPopup("Type of Noise", noiseFunc.type);
            noiseFunc.enabled = EditorGUILayout.ToggleLeft("Enabled", noiseFunc.enabled);
            noiseFunc.frequency = (double)EditorGUILayout.Slider("Frequency", (float)noiseFunc.frequency, 0f, 20f);
            noiseFunc.lacunarity = (double)EditorGUILayout.Slider("Lacunarity", (float)noiseFunc.lacunarity, 1.5000000f, 3.5000000f);
            noiseFunc.persistence = (double)EditorGUILayout.Slider("Persistence", (float)noiseFunc.persistence, 0f, 1f);
            noiseFunc.octaves = EditorGUILayout.IntSlider("Octaves", noiseFunc.octaves, 0, 18);
            noiseFunc.qualityMode = (LibNoise.QualityMode)EditorGUILayout.EnumPopup("Quality Mode", noiseFunc.qualityMode);
            noiseFunc.blendMode = (NoiseFunctions.BlendMode)EditorGUILayout.EnumPopup("Blend Mode", noiseFunc.blendMode);
            if (GUILayout.Button("Remove"))
            {
                NoiseFunctions[] placeHolder = new NoiseFunctions[generator.noiseFunctions.Length - 1];
                placeHolder = generator.noiseFunctions.RemoveAt(index);
                generator.noiseFunctions = placeHolder;
            }
        }
#endregion

        #region Voronoi UI
        else if (noiseFunc.type == NoiseFunctions.NoiseType.Voronoi)
        {
            EditorGUILayout.Space();
            string name = "Voronoi Noise";
            EditorGUILayout.LabelField(name);
            noiseFunc.type = (NoiseFunctions.NoiseType)EditorGUILayout.EnumPopup("Type of Noise", noiseFunc.type);
            noiseFunc.enabled = EditorGUILayout.ToggleLeft("Enabled", noiseFunc.enabled);
            noiseFunc.frequency = (double)EditorGUILayout.Slider("Frequency", (float)noiseFunc.frequency, 0f, 20f);
            noiseFunc.displacement = (double)EditorGUILayout.Slider("Displacement", (float)noiseFunc.displacement, 0f, 20f);
            noiseFunc.distance = EditorGUILayout.ToggleLeft("Use Distance", noiseFunc.distance);
            noiseFunc.blendMode = (NoiseFunctions.BlendMode)EditorGUILayout.EnumPopup("Blend Mode", noiseFunc.blendMode);
            if (GUILayout.Button("Remove"))
            {
                NoiseFunctions[] placeHolder = new NoiseFunctions[generator.noiseFunctions.Length - 1];
                placeHolder = generator.noiseFunctions.RemoveAt(index);
                generator.noiseFunctions = placeHolder;
            }

        }
#endregion

        #region Ridged Multifractal UI
        else if (noiseFunc.type == NoiseFunctions.NoiseType.RidgedMultifractal)
        {

            EditorGUILayout.Space();
            string name = "Ridged Multifractal";
            EditorGUILayout.LabelField(name);
            noiseFunc.type = (NoiseFunctions.NoiseType)EditorGUILayout.EnumPopup("Type of Noise", noiseFunc.type);
            noiseFunc.enabled = EditorGUILayout.ToggleLeft("Enabled", noiseFunc.enabled);
            noiseFunc.frequency = (double)EditorGUILayout.Slider("Frequency", (float)noiseFunc.frequency, 0f, 20f);
            noiseFunc.lacunarity = (double)EditorGUILayout.Slider("Lacunarity", (float)noiseFunc.lacunarity, 1.5000000f, 3.5000000f);
            noiseFunc.octaves = EditorGUILayout.IntSlider("Octaves", noiseFunc.octaves, 0, 18);
            noiseFunc.qualityMode = (LibNoise.QualityMode)EditorGUILayout.EnumPopup("Quality Mode", noiseFunc.qualityMode);
            noiseFunc.blendMode = (NoiseFunctions.BlendMode)EditorGUILayout.EnumPopup("Blend Mode", noiseFunc.blendMode);
            if (GUILayout.Button("Remove"))
            {
                NoiseFunctions[] placeHolder = new NoiseFunctions[generator.noiseFunctions.Length - 1];
                placeHolder = generator.noiseFunctions.RemoveAt(index);
                generator.noiseFunctions = placeHolder;
            }
        }
        #endregion

        #region None UI
        else if (noiseFunc.type == NoiseFunctions.NoiseType.None)
        {

            EditorGUILayout.Space();
            string name = "None";
            EditorGUILayout.LabelField(name);
            noiseFunc.type = (NoiseFunctions.NoiseType)EditorGUILayout.EnumPopup("Type of Noise", noiseFunc.type);
            if (GUILayout.Button("Remove"))
            {
                NoiseFunctions[] placeHolder = new NoiseFunctions[generator.noiseFunctions.Length - 1];
                placeHolder = generator.noiseFunctions.RemoveAt(index);
                generator.noiseFunctions = placeHolder;
            }
            noiseFunc.enabled = false;
        }

        #endregion

        //to autoupdate if this inspector element has changed
        if (generator.autoUpdate&&EditorGUI.EndChangeCheck())
        {
            generator.GenerateMap();
        }

        

    }

    #endregion

    #endregion



}
