using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibNoise.Unity.Generator;
using LibNoise.Unity.Operator;
using LibNoise.Unity;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class MapGenerator : MonoBehaviour {
    #region public values
    //enum to determain which texture to generate
    public enum MapType {NoiseMap, ColorMap, Mesh, Planet};
    [Range(1, 6)]
    public int PlanetItterations;
    public enum RenderType {FlatMap, Sphere };
    [HideInInspector]
    public GameObject mapCanvas;
    
    //bool to select between seamless and non seamless map generation
    //It should be noted that seamless generation takes much more time
    public bool seamless = false;
    public bool clamped = true;
    //inspector varibles
    public MapType mapType;
    public RenderType renderType;
    public int mapWidth;
    public int mapHeight;
    public bool autoUpdate;

    public float heightMultiplier;
    public AnimationCurve heightAdjuster;

    public string seed;
    public bool useRandomSeed = true;
    //public bool autoUpdate = false;
    //hidden because this is generated via editor scripting
    [HideInInspector]
    public int seedValue = 0;
    #endregion

    private Noise2D noiseMap = null;
    [HideInInspector]
    public Texture2D[] textures = new Texture2D[3];  //other array members are for normal map / uv's
    //hidden in inspector because of custom inspector implementation
    [HideInInspector]
    public NoiseFunctions[] noiseFunctions;
    public TerrainType[] regions;
    private ModuleBase baseModule = null;
    #region MapGeneration

    //map generation script
    public void GenerateMap()
    {
        //this is the base noise module that will be manipulated 
        baseModule = null;
        //this is the noisemap that will be generated
        noiseMap = null;
        MapDisplay display = FindObjectOfType<MapDisplay>();

        //generates meshes for every noisefunction

        for (int i = 0; i < noiseFunctions.Length; i++)
        {
            if (noiseFunctions[i].enabled)
            {
                noiseFunctions[i].FormMesh();
            }
        }
        
        //manipulates the base module based on the noise modules
        for (int i = 0; i < noiseFunctions.Length; i++)
        {
            //for first valid noise pattern
            if (baseModule == null&&noiseFunctions[i].enabled)
            {
                baseModule = noiseFunctions[i].moduleBase;
            }
            //all others after the first modify the previous iteration of the baseModule
            else if(noiseFunctions[i].enabled)
            {
                baseModule = new Add(baseModule, noiseFunctions[i].moduleBase);
            }
        }
        //clamps the module to between 1 and 0
        if (clamped)
        {
            baseModule = new Clamp(0, 1, baseModule);
        }

        noiseMap = new Noise2D(mapWidth, mapHeight, baseModule);

        if (mapType == MapType.Planet)
        {
            int subdivisions = PlanetItterations;
            int resolution = 1 << subdivisions;
            int verticies = (resolution + 1) * (resolution + 1) * 4 - (resolution * 2 - 1) * 3;

            Color[] colorMap = new Color[verticies];
            Mesh newMesh = SphereMagic.CreatePlanet(PlanetItterations, 100, baseModule, heightMultiplier, ref colorMap, regions);
           
            //thank you wolfram alpha for this array size
            textures[1] = new Texture2D(resolution+1, resolution+7);
            textures[1].SetPixels(colorMap);
            display.DrawMesh(newMesh, textures[1]);


        }

        //Generates a planar map or spherical map that is either seamless or not based on user input
        else if (renderType == RenderType.FlatMap)
        {
            noiseMap.GeneratePlanar(-1, 1, -1, 1, seamless);
        }
        else if (renderType == RenderType.Sphere)
        {
            noiseMap.GenerateSpherical(-90, 90, -180, 180);
        }

        

        //generates a raw noise type based on the public enum
        if (mapType == MapType.NoiseMap)
        {
            GenerateMapCanvas();
            textures[0] = noiseMap.GetTexture(LibNoise.Unity.Gradient.Grayscale);
            textures[0].Apply();
            display.DrawTextureOnPlane(textures[0]);
        }

        //generates a color map based on the public enum
        else if (mapType == MapType.ColorMap)
        {
            GenerateMapCanvas();
            Color[] colorMap = new Color[mapWidth * mapHeight];
            textures[0] = noiseMap.GetTexture(LibNoise.Unity.Gradient.Grayscale);
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    float currentHeight = noiseMap[x, y];
                    for (int i = 0; i < regions.Length; i++)
                    {
                        if (currentHeight <= regions[i].height)
                        {
                            colorMap[y * mapWidth + x] = regions[i].color;
                            break;
                        }
                    }
                }
            }
            textures[0].SetPixels(colorMap);
            textures[0].Apply();
            display.DrawTextureOnPlane(textures[0]);
        }

        else if (mapType == MapType.Mesh)
        {
            display.TextureRender = FindObjectOfType<Renderer>();
            textures[0] = noiseMap.GetTexture(LibNoise.Unity.Gradient.Grayscale);
            Color[] colorMap = new Color[noiseMap.Width * noiseMap.Height];
            for (int y = 0; y < noiseMap.Height; y++)
            {
                for (int x = 0; x < noiseMap.Width; x++)
                {
                    float currentHeight = noiseMap[x, y];
                    for (int i = 0; i < regions.Length; i++)
                    {
                        if (currentHeight <= regions[i].height)
                        {
                            colorMap[y * noiseMap.Width + x] = regions[i].color;
                            break;
                        }
                    }
                }
            }

            textures[0].SetPixels(colorMap);
            textures[0].Apply();

            display.DrawMesh(FlatMeshGenerator.GenerateTerrainMesh(noiseMap, heightMultiplier, heightAdjuster), textures[0]);
        }
        
        
    }

    #endregion

    public void SavePresets(NoiseFunctions[] savedPresets, string destpath)//saves the map to a given string location.
    {
        NoisePresets[] presetsToSave = new NoisePresets[savedPresets.Length];
        for (int i = 0; i < savedPresets.Length; i++)
        {
            presetsToSave[i] = noiseFunctions[i].GetPresets();
        }

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(destpath);
        bf.Serialize(file, presetsToSave);
        file.Close();
    }
    
    public void SaveImage(string filePath)
    {
        System.IO.File.WriteAllBytes(filePath, textures[0].EncodeToPNG());
    }

    public void LoadPresets(string filePath)  //loads map from a given string location
    {
        if (File.Exists(filePath))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(filePath, FileMode.Open);
            NoisePresets[] loadedPresets = (NoisePresets[])bf.Deserialize(file);
            NoiseFunctions[] holder = new NoiseFunctions[loadedPresets.Length];
            for (int i = 0; i < loadedPresets.Length; i++)
            {
                holder[i] = new NoiseFunctions(loadedPresets[i]);
            }
            noiseFunctions = new NoiseFunctions[holder.Length];
            noiseFunctions = holder;
            file.Close();
        }
    }

    private void GenerateMapCanvas()
    {
        //destroys even in editmode
        DestroyImmediate(mapCanvas);
        //this will be replaced with the procedurally generated sphere in the other scene
        if (renderType == RenderType.Sphere) 
        {
            mapCanvas = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            MapDisplay display = FindObjectOfType<MapDisplay>();
            display.TextureRender = mapCanvas.GetComponent<Renderer>();
            mapCanvas.transform.position = new Vector3(0, 0, 0);
        }
        else
        {
            mapCanvas = GameObject.CreatePrimitive(PrimitiveType.Plane);
            MapDisplay display = FindObjectOfType<MapDisplay>();
            display.TextureRender = mapCanvas.GetComponent<Renderer>();
            mapCanvas.transform.position = new Vector3(0, 0, 0);
        }
    }

}
#region Serialized Data Sets
//trying to build this entire program usable for world generation from the editor, encapsulating effects in orderable serialized classes for this reason.
[System.Serializable]
public class NoiseFunctions     
{
    public enum NoiseType { Perlin, Billow, RiggedMultifractal, Voronoi, None };
    [Range(0,1)]
    //public float noiseScale = 0.5f;
    public NoiseType type = NoiseType.Perlin;
    public bool enabled = false;
    public ModuleBase moduleBase;
    [HideInInspector]
    public int height;
    [HideInInspector]
    public int width;
 
    [Range(0f,20f)]
    public double frequency;
    [Range(2.0000000f, 2.5000000f)]
    public double lacunarity;
    [Range(0f, 1f)]
    public double persistence;
    [Range(1,18)]
    public int octaves;
    public int seed;
    public QualityMode qualityMode;

    public double displacement;
    public bool distance;

    public NoiseFunctions()
    {
        enabled = true;
        frequency = 1;
        lacunarity = 2.2;
        persistence = 0.5;
        octaves = 1;
        qualityMode = QualityMode.Low;
        displacement = 1;
        distance = true;

    }

    #region Noise Functions Preset Handling
    public NoiseFunctions(NoisePresets presets)
    {
        enabled = presets.enabled;
        frequency = presets.frequency;
        lacunarity = presets.lacunarity;
        persistence = presets.persistence;
        octaves = presets.octaves;
        if (presets.qualityMode == NoisePresets.QualityMode.High)
        {
            qualityMode = QualityMode.High;
        }
        else if (presets.qualityMode == NoisePresets.QualityMode.Medium)
        {
            qualityMode = QualityMode.Medium;
        }
        else 
        {
            qualityMode = QualityMode.Low;
        }

        if (presets.noiseType == NoisePresets.NoiseType.Billow)
        {
            type = NoiseType.Billow;
        }
        else if (presets.noiseType == NoisePresets.NoiseType.Perlin)
        {
            type = NoiseType.Perlin;
        }
        else if (presets.noiseType == NoisePresets.NoiseType.RiggedMultifractal)
        {
            type = NoiseType.RiggedMultifractal;
        }
        else if (presets.noiseType == NoisePresets.NoiseType.Voronoi)
        {
            type = NoiseType.Voronoi;
        }
        else 
        {
            type = NoiseType.None;
        }



        displacement = presets.displacement;
        distance = presets.distance;
    }
    public NoisePresets GetPresets()
    {
        NoisePresets preset = new NoisePresets();
        preset.enabled = enabled;
        preset.frequency = frequency;
        preset.lacunarity = lacunarity;
        preset.persistence = persistence;
        preset.octaves = octaves;
        preset.displacement = displacement;
        preset.distance = distance;


        if (qualityMode == QualityMode.High)
        {
            preset.qualityMode = NoisePresets.QualityMode.High;
        }
        else if (qualityMode == QualityMode.Medium)
        {
            preset.qualityMode = NoisePresets.QualityMode.Medium;
        }
        else 
        {
            preset.qualityMode = NoisePresets.QualityMode.Low;
        }

        if (type == NoiseType.Perlin)
        {
            preset.noiseType = NoisePresets.NoiseType.Perlin;
        }
        else if (type == NoiseType.Billow)
        {
            preset.noiseType = NoisePresets.NoiseType.Billow;
        }
        else if (type == NoiseType.RiggedMultifractal)
        {
            preset.noiseType = NoisePresets.NoiseType.RiggedMultifractal;
        }
        else if (type == NoiseType.Voronoi)
        {
            preset.noiseType = NoisePresets.NoiseType.Voronoi;
        }
        else
        {
            preset.noiseType = NoisePresets.NoiseType.None;
        }

        return preset;
    }
    #endregion

    //generates the mesh based on selected noise type
    public void FormMesh()
    {

        if (type == NoiseType.Billow) { moduleBase = new Billow(frequency, lacunarity, persistence, octaves, seed, qualityMode);}
        else if (type == NoiseType.Perlin) { moduleBase = new Perlin(frequency,lacunarity,persistence,octaves,seed,qualityMode);}
        else if (type == NoiseType.Voronoi) { moduleBase = new Voronoi(frequency,displacement,seed,distance);}
        else if (type == NoiseType.RiggedMultifractal) { moduleBase = new RiggedMultifractal(frequency,lacunarity,octaves,seed,qualityMode);}
        else moduleBase = null;


    }
}

[System.Serializable]
public struct NoisePresets
{
    public enum NoiseType { Perlin, Billow, RiggedMultifractal, Voronoi, None };
    public enum QualityMode
    {
        Low,
        Medium,
        High,
    }
    public NoiseType noiseType;
    public bool enabled;
    public double frequency;
    public double lacunarity;
    public double persistence;
    public int octaves;
    public QualityMode qualityMode;
    public double displacement;
    public bool distance;

}


//serializable to be accessible in the inspector
[System.Serializable]
public struct TerrainType
{
    public string name;
    public double height;
    public Color color;
}
#endregion
