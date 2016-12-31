using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof (MapGenerator))]
public class MapGenEditor : Editor {

    public NoiseFunctions.NoiseType noiseType;
    public bool showNoiseFunctions;
    public bool showTextures;

    public override void OnInspectorGUI()
    {
        MapGenerator mapGen = (MapGenerator)target;
        //Universal Displays
        mapGen.mapType = (MapGenerator.MapType)EditorGUILayout.EnumPopup("Type of Map to Generate", mapGen.mapType);
        mapGen.seamless = EditorGUILayout.ToggleLeft("Seamless Map",mapGen.seamless);
        mapGen.autoUpdate = EditorGUILayout.ToggleLeft("Autoupdate (Warning, Slow)", mapGen.autoUpdate);
        mapGen.mapWidth = EditorGUILayout.IntField("Width of Map: ", mapGen.mapWidth);
        mapGen.mapHeight = EditorGUILayout.IntField("Height of Map: ", mapGen.mapHeight);
        mapGen.seed = EditorGUILayout.TextField("Seed: ", mapGen.seed);

        
        //error control to keep height positive and non zero
        if (mapGen.mapWidth < 1) mapGen.mapWidth = 1;
        if (mapGen.mapHeight < 1) mapGen.mapHeight = 1;

        //start the fold out for noise functions
        showNoiseFunctions = EditorGUILayout.Foldout(showNoiseFunctions, "Noise Functions");
        if (showNoiseFunctions)
        {
            if (GUILayout.Button("Add New Noise Function"))
            {
                NoiseFunctions[] placeholder = new NoiseFunctions[mapGen.noiseFunctions.Length + 1];
                for (int j = 0; j < mapGen.noiseFunctions.Length; j++)
                {
                    placeholder[j] = mapGen.noiseFunctions[j];
                }
                placeholder[mapGen.noiseFunctions.Length] = new NoiseFunctions();
                mapGen.noiseFunctions = new NoiseFunctions[placeholder.Length];
                for (int j = 0; j < placeholder.Length; j++)
                {
                    mapGen.noiseFunctions[j] = placeholder[j];
                }
                return;
            }

        }

        //going through the noise functions
        for (int i = 0; i < mapGen.noiseFunctions.Length; i++)
        {
            if (showNoiseFunctions)
            {
                GetInspectorElements(mapGen.noiseFunctions[i], i);
            }
            //transfers height / width
            mapGen.noiseFunctions[i].height = mapGen.mapHeight;
            mapGen.noiseFunctions[i].width = mapGen.mapWidth;
            
            //seed distribution, lots of i's scattered for fun
            if (!mapGen.useRandomSeed)
            {
                    mapGen.seedValue = mapGen.seed.GetHashCode()*i;
            }

            else
            {
                mapGen.seedValue = Random.Range(0, 10000000);
            }
                mapGen.noiseFunctions[i].seed = mapGen.seedValue + i;
        }

        //autoupdate function, so far it's really slow, may remove
        if (mapGen.autoUpdate)
        {
            mapGen.GenerateMap();
        }        

        if (GUILayout.Button("Generate"))
        {
            mapGen.GenerateMap();
        }
    }

    public void GetInspectorElements(NoiseFunctions noiseFunc, int index)
    {

        if (noiseFunc.type == NoiseFunctions.NoiseType.Perlin)
        {
            EditorGUILayout.Space();
            string name = "Perlin Noise";
            EditorGUILayout.LabelField(name);
            noiseFunc.type = (NoiseFunctions.NoiseType)EditorGUILayout.EnumPopup("Type of Noise", noiseFunc.type);
            noiseFunc.enabled = EditorGUILayout.ToggleLeft("Enabled", noiseFunc.enabled);
            noiseFunc.frequency = (double)EditorGUILayout.Slider("Frequency",(float)noiseFunc.frequency, 0f, 20f);
            noiseFunc.lacunarity = (double)EditorGUILayout.Slider("Lacunarity", (float)noiseFunc.lacunarity, 2.0000000f, 2.5000000f);
            noiseFunc.persistence = (double)EditorGUILayout.Slider("Persistence", (float)noiseFunc.persistence, 0f, 1f);
            noiseFunc.octaves = EditorGUILayout.IntSlider("Octaves", noiseFunc.octaves, 0, 18);
            noiseFunc.qualityMode = (LibNoise.Unity.QualityMode)EditorGUILayout.EnumPopup("Quality Mode", noiseFunc.qualityMode);
            if (GUILayout.Button("Remove"))
            {
                MapGenerator mapGen = (MapGenerator)target;
                NoiseFunctions[] placeholder = new NoiseFunctions[mapGen.noiseFunctions.Length - 1];
                for (int i = 0; i < mapGen.noiseFunctions.Length; i++)
                {
                    int tempIndex = 0;
                    if (i != index)
                    {
                        placeholder[tempIndex] = mapGen.noiseFunctions[i];
                        tempIndex++;
                    }
                }
                mapGen.noiseFunctions = new NoiseFunctions[mapGen.noiseFunctions.Length - 1];
                for (int i = 0; i < mapGen.noiseFunctions.Length; i++)
                {
                    mapGen.noiseFunctions[i] = placeholder[i];
                }
                
            }
        }
        else if (noiseFunc.type == NoiseFunctions.NoiseType.Billow)
        {
            EditorGUILayout.Space();
            string name = "Billow Noise";
            EditorGUILayout.LabelField(name);
            noiseFunc.type = (NoiseFunctions.NoiseType)EditorGUILayout.EnumPopup("Type of Noise", noiseFunc.type);
            noiseFunc.enabled = EditorGUILayout.ToggleLeft("Enabled", noiseFunc.enabled);
            noiseFunc.frequency = (double)EditorGUILayout.Slider("Frequency", (float)noiseFunc.frequency, 0f, 20f);
            noiseFunc.lacunarity = (double)EditorGUILayout.Slider("Lacunarity", (float)noiseFunc.lacunarity, 2.0000000f, 2.5000000f);
            noiseFunc.persistence = (double)EditorGUILayout.Slider("Persistence", (float)noiseFunc.persistence, 0f, 1f);
            noiseFunc.octaves = EditorGUILayout.IntSlider("Octaves", noiseFunc.octaves, 0, 18);
            noiseFunc.qualityMode = (LibNoise.Unity.QualityMode)EditorGUILayout.EnumPopup("Quality Mode", noiseFunc.qualityMode);
            if (GUILayout.Button("Remove"))
            {
                MapGenerator mapGen = (MapGenerator)target;
                NoiseFunctions[] placeholder = new NoiseFunctions[mapGen.noiseFunctions.Length - 1];
                for (int i = 0; i < mapGen.noiseFunctions.Length; i++)
                {
                    int tempIndex = 0;
                    if (i != index)
                    {
                        placeholder[tempIndex] = mapGen.noiseFunctions[i];
                        tempIndex++;
                    }
                }
                mapGen.noiseFunctions = new NoiseFunctions[mapGen.noiseFunctions.Length - 1];
                for (int i = 0; i < mapGen.noiseFunctions.Length; i++)
                {
                    mapGen.noiseFunctions[i] = placeholder[i];
                }

            }
        }
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
            if (GUILayout.Button("Remove"))
            {
                MapGenerator mapGen = (MapGenerator)target;
                NoiseFunctions[] placeholder = new NoiseFunctions[mapGen.noiseFunctions.Length - 1];
                for (int i = 0; i < mapGen.noiseFunctions.Length; i++)
                {
                    int tempIndex = 0;
                    if (i != index)
                    {
                        placeholder[tempIndex] = mapGen.noiseFunctions[i];
                        tempIndex++;
                    }
                }
                mapGen.noiseFunctions = new NoiseFunctions[mapGen.noiseFunctions.Length - 1];
                for (int i = 0; i < mapGen.noiseFunctions.Length; i++)
                {
                    mapGen.noiseFunctions[i] = placeholder[i];
                }

            }

        }
        else if (noiseFunc.type == NoiseFunctions.NoiseType.RiggedMultifractal)
        {

            EditorGUILayout.Space();
            string name = "Rigged Multifractal";
            EditorGUILayout.LabelField(name);
            noiseFunc.type = (NoiseFunctions.NoiseType)EditorGUILayout.EnumPopup("Type of Noise", noiseFunc.type);
            noiseFunc.enabled = EditorGUILayout.ToggleLeft("Enabled", noiseFunc.enabled);
            noiseFunc.frequency = (double)EditorGUILayout.Slider("Frequency", (float)noiseFunc.frequency, 0f, 20f);
            noiseFunc.lacunarity = (double)EditorGUILayout.Slider("Lacunarity", (float)noiseFunc.lacunarity, 2.0000000f, 2.5000000f);
            noiseFunc.octaves = EditorGUILayout.IntSlider("Octaves", noiseFunc.octaves, 0, 18);
            noiseFunc.qualityMode = (LibNoise.Unity.QualityMode)EditorGUILayout.EnumPopup("Quality Mode", noiseFunc.qualityMode);
            if (GUILayout.Button("Remove"))
            {
                MapGenerator mapGen = (MapGenerator)target;
                NoiseFunctions[] placeholder = new NoiseFunctions[mapGen.noiseFunctions.Length - 1];
                for (int i = 0; i < mapGen.noiseFunctions.Length; i++)
                {
                    int tempIndex = 0;
                    if (i != index)
                    {
                        placeholder[tempIndex] = mapGen.noiseFunctions[i];
                        tempIndex++;
                    }
                }
                mapGen.noiseFunctions = new NoiseFunctions[mapGen.noiseFunctions.Length - 1];
                for (int i = 0; i < mapGen.noiseFunctions.Length; i++)
                {
                    mapGen.noiseFunctions[i] = placeholder[i];
                }
                return;

            }
        }
        else if (noiseFunc.type == NoiseFunctions.NoiseType.None)
        {

            EditorGUILayout.Space();
            string name = "None";
            EditorGUILayout.LabelField(name);
            noiseFunc.type = (NoiseFunctions.NoiseType)EditorGUILayout.EnumPopup("Type of Noise", noiseFunc.type);
            if (GUILayout.Button("Remove"))
            {
                MapGenerator mapGen = (MapGenerator)target;
                NoiseFunctions[] placeholder = new NoiseFunctions[mapGen.noiseFunctions.Length - 1];
                for (int i = 0; i < mapGen.noiseFunctions.Length; i++)
                {
                    int tempIndex = 0;
                    if (i != index)
                    {
                        placeholder[tempIndex] = mapGen.noiseFunctions[i];
                        tempIndex++;
                    }
                }
                mapGen.noiseFunctions = new NoiseFunctions[mapGen.noiseFunctions.Length - 1];
                for (int i = 0; i < mapGen.noiseFunctions.Length; i++)
                {
                    mapGen.noiseFunctions[i] = placeholder[i];
                }

            }
            noiseFunc.enabled = false;
        }
    }
    
}
