﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibNoise.Generator;
using LibNoise.Operator;
using LibNoise;
using System;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class MapGenerator : MonoBehaviour
{
    #region public values

    //enum to determine which texture to generate
    public enum MapType { FlatTerrain, Planet };
    [Range(1, 6)]
    public int PlanetItterations;
    public float radius;
    public float seaLevelOffset;
    public enum RenderType { Greyscale, Color };
    [HideInInspector]
    public GameObject mapCanvas;

    //bool to select between seamless and non seamless map generation for 2d maps
    //It should be noted that seamless generation takes much more time
    [HideInInspector]
    public bool seamless = false;
    public bool multithreading = true;
    public bool oceans = true;
    public bool clamped = true;
    //inspector varibles
    [HideInInspector]
    public MapType mapType = MapType.Planet;
    [HideInInspector]
    public RenderType renderType;
    public int mapWidth;
    [HideInInspector]
    public int mapHeight;
    public bool autoUpdate;

    public float heightMultiplier;
    public AnimationCurve heightAdjuster;

    public string seed;
    public bool useRandomSeed;
    [HideInInspector]
    public TerrainType[] regions;
    [HideInInspector]
    public int seedValue;
    public GameObject waterMesh;
    #endregion

    #region Private / Hidden values


    private int latestTimeProcessRequested;
    private Noise2D noiseMap = null;
    [HideInInspector]
    public Texture2D mapTexture;

    bool reset;
    bool drawInProgress;
    bool stop;
    Thread noiseThread;
    public Noise2D updatedMap;
    bool threadRunning;

    [HideInInspector]
    public bool noiseMapUpdateAvailable;

    //hidden in inspector because of custom inspector implementation
    [HideInInspector]
    public NoiseFunctions[] noiseFunctions;

    private ModuleBase baseModule = null;
    #endregion

    #region Map Generator
    /// <summary>
    /// The Core Map Generation Method, in essence this method
    /// takes the stack of noise methods and processes them
    /// sequentially and generates a resultant noise map.
    /// It then calls a function to generate the Mesh (or 3D object)
    /// and applies the noise to the mesh inside of the function 
    /// to draw the mesh.

    /// -RGS
    /// </summary>
    public void GenerateMap()
    {

        #region variables and setup

        //This function prevents the generation from happening if there is no noise stack to process.
        if (noiseFunctions.Length < 1)
        {
            Debug.Log("Blank Noise functions, Please load noise functions before generating.");
            return;
        }

        //this is the base noise module that will be manipulated 
        baseModule = null;

        //this is the noisemap that will be generated
        noiseMap = null;


        ///next two commands interface with multithreaded renderer
        ///to shut it down if it's running.  The bool adds a hard 
        ///abort check that doesn't rely on the timestamp
  
        HaltThreads();
        reset = true;

        //misc Setup
        MapDisplay display = FindObjectOfType<MapDisplay>();

        #endregion

        #region Noise Map Initialization

        //Generates a random seed and passes it to the noise processor
        if (!useRandomSeed) { seedValue = seed.GetHashCode(); }
        else seedValue = UnityEngine.Random.Range(0, 10000000);
        baseModule = InitNoise(noiseFunctions, seedValue);
 
        
        //This clamps the module to between 1 and 0, sort of...
        //because of the way coherent noise works, it's not possible to completely
        //eliminate the possibility that a value will fall outside these ranges.
        if (clamped)
        {
            baseModule = new Clamp(0, 1, baseModule);
        }
        noiseMap = new Noise2D(mapWidth, mapHeight, baseModule);

        #endregion

        #region Planet Generator Setup
        if (mapType == MapType.Planet)
        {
            mapHeight = mapWidth / 2;
            renderType = RenderType.Color;

            if ((oceans) && (!waterMesh.activeSelf))
            {
                waterMesh.SetActive(true);
            }
            if (waterMesh != null)
            {
                waterMesh.transform.localScale = 2 * (new Vector3(radius + seaLevelOffset, radius + seaLevelOffset, radius + seaLevelOffset));

                if (!oceans)
                {
                    waterMesh.SetActive(false);
                }
            }
        }
        #endregion

        #region Flat Terrain Generator
        //non functional

        else if (mapType == MapType.FlatTerrain)
        {
            noiseMap = new Noise2D(100, 100, baseModule);
            noiseMap.GeneratePlanar(-1, 1, -1, 1, seamless);
            display.TextureRender = FindObjectOfType<Renderer>();
            mapTexture = GetMapTexture(renderType, noiseMap);
            display.DrawMesh(FlatMeshGenerator.GenerateTerrainMesh(noiseMap, heightMultiplier, heightAdjuster), mapTexture);
        }
        #endregion

        #region Start Multithreaded Noisemapping
        if (multithreading){ThreadMap();}
 
        #endregion
    }

    #endregion

    #region Noise Processor
    public ModuleBase InitNoise(NoiseFunctions[] noiseStack, int seed)
    {
        ModuleBase _baseModule = null;
        for (int i = 0; i < noiseStack.Length; i++)
        {
            noiseStack[i].seed = seed + i;

            //for first valid noise pattern simply pass the noise function
            if ((noiseStack[i].enabled) && (_baseModule == null))
            {
                _baseModule = noiseStack[i].MakeNoise();
            }

            //all others valid add to / subtract from the previous iteration of the baseModule
            else if (noiseStack[i].enabled)
            {
                if (noiseFunctions[i].blendMode == NoiseFunctions.BlendMode.Add)
                {
                    _baseModule = new Add(_baseModule, noiseStack[i].MakeNoise());
                }
                if (noiseFunctions[i].blendMode == NoiseFunctions.BlendMode.Subtract)
                {
                    _baseModule = new Subtract(_baseModule, noiseStack[i].MakeNoise());
                }
            }
        }
        return _baseModule;
    }
    #endregion

    #region Work in Progress
    public Mesh GeneratePlane()
    {
        /// <summary>
        /// This function will be the one that gets called to generate a map from coordinates.
        /// </summary>

        Mesh mesh = new Mesh();
        return mesh;
    }
    #endregion

    #region Map Texture Generator

    

    private Texture2D GetMapTexture(RenderType typeIn, Noise2D noiseIn)
    {
        Texture2D mapReturned;
        Color[] colorMap = new Color[noiseIn.Width * noiseIn.Height];
        mapReturned = new Texture2D(noiseIn.Width, noiseIn.Height);
        if (typeIn == RenderType.Greyscale)
        {
            mapReturned = noiseIn.GetTexture();
        }
        else
        {

            for (int y = 0; y < noiseIn.Height; y++)
            {
                for (int x = 0; x < noiseIn.Width; x++)
                {
                    float currentHeight = noiseIn[x, y];
                    for (int i = 0; i < regions.Length; i++)
                    {
                        if (currentHeight <= regions[i].height)
                        {
                            if ((i == 0) || (i == regions.Length - 1))
                            {
                                colorMap[y * noiseIn.Width + x] = regions[i].color;
                                break;
                            }
                            else
                            {
                                colorMap[y * noiseIn.Width + x] = Color.Lerp(regions[i - 1].color, regions[i].color, (float)(currentHeight - regions[i].height) / (float)(regions[i + 1].height - regions[i].height));
                                break;
                            }
                        }
                    }
                }
            }

            mapReturned.SetPixels(colorMap);
        }
        mapReturned.Apply();
        mapReturned.filterMode = FilterMode.Point;
        return mapReturned;
    }
    #endregion

    #region Single Thread Handler
    /// <summary>
    /// write me pls k thanks!
    /// </summary>
    #endregion

    #region Multithreading Handlers
    //threading handler functions

    private void ThreadMap()
    ///summary
    ///initializes multithreading
    {
        if (drawInProgress)
        {
            HaltThreads();
        }

        latestTimeProcessRequested = DateTime.Now.GetHashCode();
        drawInProgress = true;
        stop = false;
        StartCoroutine(TextureRefiner());
    }

    IEnumerator TextureRefiner()
    ///summary
    ///This  is the process used to launch
    ///the noise processor and as the handler
    ///for incoming texture maps.

    {
        MapDisplay display = FindObjectOfType<MapDisplay>();
        noiseThread = new Thread(ProcessTextures);
        noiseThread.Start();
        while (drawInProgress)
        {
            //Debug.Log("Tick");
            if (noiseMapUpdateAvailable)
            {
                UpdateSphereMap();
            }
            else
            {
                yield return new WaitForSeconds(.2f);
            }

        }
        Debug.Log("IEnumerator shutdown");
        noiseThread.Abort();
        yield break;
    }

    public void UpdateSphereMap()
    ///summary
    ///This is the process that updates the texture and
    ///the mesh.
    {
        MapDisplay display = FindObjectOfType<MapDisplay>();
        noiseMap = updatedMap;
        Debug.Log(latestTimeProcessRequested + "updating map");
        mapTexture = GetMapTexture(renderType, noiseMap);
        display.DrawMesh(SphereMagic.CreatePlanet(PlanetItterations, radius, baseModule, heightMultiplier, regions), mapTexture);
        noiseMapUpdateAvailable = false;
    }

    void ProcessTextures()
        ///summary
        ///This thread processes the textures
        ///incrementally in increasing resolutions.
        ///It uses abort checks that extend to 
        ///the more computationaly intense parts
        ///of the code--halting and aborting threads
        ///that are no longer necessary mid process.
    {
        var processTimestamp = latestTimeProcessRequested;
        int count = 0;

        //the crazyness in the for loop's i value is in order to allow it to be
        //used as a resolution manipulator without doing the math multiple times inside a loop

        for (int i = 16; i > 1; i = i / 2)
        {
            Debug.Log((count + 1) + " run start by " + processTimestamp);
            if (processTimestamp != latestTimeProcessRequested)
            {
                Debug.Log("Stopping Thread " + processTimestamp + " for thread " + latestTimeProcessRequested);
                drawInProgress = false;
                return;
            }
            if (i == 16)
            {
                reset = false;
            }
            count++;
            Noise2D placeHolder;
            Debug.Log("drawing map " + count + " by " + processTimestamp);
            placeHolder = new Noise2D(mapWidth / i, mapHeight / i, baseModule);
            placeHolder.GenerateSpherical(-90, 90, -180, 180, ref latestTimeProcessRequested, ref processTimestamp, ref reset);
            if (latestTimeProcessRequested != processTimestamp)
            {
                Debug.Log("Stopping Thread (2nd shallow catch)" + processTimestamp + " for thread" + latestTimeProcessRequested);
                drawInProgress = false;
                return;

            }
            Debug.Log("Map " + count + " drawn by " + processTimestamp);
            updatedMap = placeHolder;
            noiseMapUpdateAvailable = true;
        }
        Debug.Log("Thread Shutdown");
        drawInProgress = false;
    }
    #endregion

    #region Generation Exit Methods and Halts
    private void OnApplicationQuit()
    {
        HaltThreads();
    }

    private void OnDestroy()
    {
        HaltThreads();
    }

    public void HaltThreads()
    {
        stop = true;
        noiseMapUpdateAvailable = false;
        drawInProgress = false;
        StopAllCoroutines();
        if (noiseThread != null)
        {
            noiseThread.Abort();
        }

    }
    #endregion

    #region File IO

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

    public void SaveTerrain(TerrainType[] savedPresets, string destpath)//saves the map to a given string location.
    {
        TerrainPresets[] presetsToSave = new TerrainPresets[savedPresets.Length];
        for (int i = 0; i < savedPresets.Length; i++)
        {
            presetsToSave[i] = savedPresets[i].getTPR();
        }

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(destpath);
        bf.Serialize(file, presetsToSave);
        file.Close();
    }

    public void SaveImage(string filePath)
    {
        System.IO.File.WriteAllBytes(filePath, mapTexture.EncodeToPNG());
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

    public void LoadTerrain(string filePath)  //loads map from a given string location
    {
        if (File.Exists(filePath))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(filePath, FileMode.Open);
            TerrainPresets[] loadedPresets = (TerrainPresets[])bf.Deserialize(file);
            TerrainType[] holder = new TerrainType[loadedPresets.Length];
            for (int i = 0; i < loadedPresets.Length; i++)
            {
                holder[i] = new TerrainType(loadedPresets[i]);
            }

            regions = new TerrainType[holder.Length];
            regions = holder;
            file.Close();
        }
    }
    #endregion

}

    #region Serialized Data Sets

#region NoiseFunctions Serializable Container
[System.Serializable]
public class NoiseFunctions     
{
    public enum BlendMode {Add, Subtract };
    public enum NoiseType { Perlin, Billow, RidgedMultifractal, Voronoi, None };
    //[Range(0,1)]
    //public float noiseScale = 0.5f;
    public NoiseType type = NoiseType.Perlin;
    public bool enabled = false;
 
    [Range(0f,20f)]
    public double frequency;
    [Range(2.0000000f, 2.5000000f)]
    public double lacunarity;
    [Range(0f, 1f)]
    public double persistence;
    [Range(1,18)]
    public int octaves;
    public int seed;
    public BlendMode blendMode;
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
        blendMode = BlendMode.Add;

    }
    public ModuleBase MakeNoise()
    {
        //this function builds the base module out of the noise function that calls it.
        ModuleBase _baseModule = null;
        if (type == NoiseType.Billow) { _baseModule = new Billow(frequency, lacunarity, persistence, octaves, seed, qualityMode); }
        else if (type == NoiseType.Perlin) { _baseModule = new Perlin(frequency, lacunarity, persistence, octaves, seed, qualityMode); }
        else if (type == NoiseType.Voronoi) { _baseModule = new Voronoi(frequency, displacement, seed, distance); }
        else if (type == NoiseType.RidgedMultifractal) { _baseModule = new RidgedMultifractal(frequency, lacunarity, octaves, seed, qualityMode); }
        return _baseModule;
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
        else if (presets.noiseType == NoisePresets.NoiseType.RidgedMultifractal)
        {
            type = NoiseType.RidgedMultifractal;
        }
        else if (presets.noiseType == NoisePresets.NoiseType.Voronoi)
        {
            type = NoiseType.Voronoi;
        }
        else 
        {
            type = NoiseType.None;
        }

        if (presets.blendMode == NoisePresets.BlendMode.Subtract)
        {
            blendMode = BlendMode.Subtract;
        }

        else
        {
            blendMode = BlendMode.Add;
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
        else if (type == NoiseType.RidgedMultifractal)
        {
            preset.noiseType = NoisePresets.NoiseType.RidgedMultifractal;
        }
        else if (type == NoiseType.Voronoi)
        {
            preset.noiseType = NoisePresets.NoiseType.Voronoi;
        }
        else
        {
            preset.noiseType = NoisePresets.NoiseType.None;
        }

        if (blendMode == BlendMode.Subtract)
        {
            preset.blendMode = NoisePresets.BlendMode.Subtract;
        }
        else
        {
            preset.blendMode = NoisePresets.BlendMode.Add;
        }

        return preset;
    }
    #endregion
    
}
#endregion

#region Noise Presets Serializable Container
//used to save an .npr file
[System.Serializable]
public struct NoisePresets
{
    public enum NoiseType { Perlin, Billow, RidgedMultifractal, Voronoi, None };
    public enum QualityMode { Low, Medium, High };
    public enum BlendMode { Add, Subtract };
    public NoiseType noiseType;
    public bool enabled;
    public double frequency;
    public double lacunarity;
    public double persistence;
    public int octaves;
    public QualityMode qualityMode;
    public BlendMode blendMode;
    public double displacement;
    public bool distance;

}
#endregion


#region Terrain Groups
//serializable to be accessible in the inspector
[System.Serializable]
public class TerrainType:IComparable<TerrainType>
{
    public string name;
    public double height;
    public Color color;

    public TerrainType()
    {
        name = "pick a name";
        height = 0;
        color = Color.white;
    }

    public TerrainType(TerrainPresets sourcePresets)
    {
        name = sourcePresets.name;
        height = sourcePresets.height;
        color = new Color(sourcePresets.r, sourcePresets.g, sourcePresets.g, sourcePresets.a);
    }

    public TerrainPresets getTPR()
    {
        TerrainPresets outPut = new TerrainPresets();
        outPut = ConvertTPR(this);
        return outPut;
    }

    public TerrainPresets ConvertTPR(TerrainType Source)
    {
        TerrainPresets outPut = new TerrainPresets
        {
            name = Source.name,
            height = Source.height,
            r = Source.color.r,
            g = Source.color.g,
            b = Source.color.b,
            a = Source.color.a
        };
        return outPut;
    }

    public TerrainType ConvertTPR(TerrainPresets Source)
    {
        TerrainType outPut = new TerrainType(Source);
        return outPut;
    }

    public int CompareTo(TerrainType e){return height.CompareTo(e.height);}

}
#endregion

#region Terrain Serializable Storage Container
[System.Serializable]
public struct TerrainPresets
{
    public string name;
    public double height;
    public float r;
    public float b;
    public float g;
    public float a;
}
#endregion

    #endregion