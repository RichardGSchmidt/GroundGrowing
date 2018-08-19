using System.Collections;
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
    //MFU The variables to be setup in custom constructors calls
   
    //enum to determine which texture to generate
    public enum MapType {FlatTerrain, Planet};
    [Range(1, 6)]
    public int PlanetItterations;
    public float radius;
    public float seaLevelOffset;
    public enum RenderType {Greyscale, Color };
    [HideInInspector]
    public GameObject mapCanvas;
    
    //bool to select between seamless and non seamless map generation for 2d maps
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
    public bool useRandomSeed;
    public TerrainType[] regions;
    [HideInInspector]
    public int seedValue;
    #endregion

    #region Private / Hidden values


    private DateTime latestTimeProcessRequested;
    private GameObject waterMesh;
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

    #region Map Generation
    /// <summary>
    /// The Core Map Generation Method, in essence this method
    /// takes the stack of noise methods and processes them
    /// sequentially and generates a resultant noise map.
    /// It then calls a function to generate the Mesh (or 3D object)
    /// and applies the noise to the mesh inside of the function 
    /// to draw the mesh.
    /// 
    /// Also in this located in this region is the map texture generator,
    /// which actually makes up a signifigant bulk of the computational 
    /// cost of most map generation schemas I've found useful.
    /// -RGS
    /// </summary>
    public void GenerateMap()
    {
        //MFU Need to work on the hiearchy / code reduction
        //in this method in particular.

        //MFU After reorganization add easier initial generation 
        //calls with different types of arguments.
        
        #region variables and setup
        //autoUpdate saftey catch disabled after implementing multithreading, may be added again if this is pulled back out.
        //if (autoUpdate && (mapWidth > 400 || mapHeight > 200))
        //{
        //    mapWidth = 400;
        //    mapHeight = 200;
       //     Debug.Log("Texture resolution reduced to 400x200 max during Auto Update!");
        //}

        //this is the base noise module that will be manipulated 
        baseModule = null;
        
        //this is the noisemap that will be generated
        noiseMap = null;
        
        //next two commands interface with multithreaded renderer
        HaltThreads();
        reset = true;


        MapDisplay display = FindObjectOfType<MapDisplay>();
        //this object needs better selection handling to
        //account for multiple  object types
        waterMesh = GameObject.FindGameObjectWithTag("water");

        if (!useRandomSeed)
        {
            seedValue = seed.GetHashCode();
        }
        else
            seedValue = UnityEngine.Random.Range(0, 10000000);


        #endregion

        #region Noise Function Setup        
        for (int i = 0; i < noiseFunctions.Length; i ++)
        {
            noiseFunctions[i].seed = seedValue + i;
            noiseFunctions[i].MakeNoise();

            //for first valid noise pattern simply pass the noise function
            if (baseModule == null && noiseFunctions[i].enabled)
            {
                baseModule = noiseFunctions[i].moduleBase;
            }

            //all others valid add to the previous iteration of the baseModule
            else if (noiseFunctions[i].enabled)
            {
                //this is where I want to do blend mode adjustments using
                //libNoise add, blend, subtract, multiply etc as an effect (along with falloffs maybe)
                baseModule = new Add(baseModule, noiseFunctions[i].moduleBase);
            }
        }

        //clamps the module to between 1 and 0, sort of...
        //because of the way coherent noise works, it's not possible to completely
        //eliminate the possibility that a value will fall outside these ranges.
     
        if (clamped)
        {
            baseModule = new Clamp(0, 1, baseModule);
        }
        noiseMap = new Noise2D(mapWidth, mapHeight, baseModule);
        #endregion

        #region Planet Generator
        if (mapType == MapType.Planet)
        {
            noiseMap = new Noise2D(100, 100, baseModule);
            noiseMap.GenerateSpherical(-90, 90, -180, 180);
            mapTexture = GetMapTexture(renderType, noiseMap);
            if (waterMesh != null)
            {
                //MFU better Sea Level Autoupdate / switch
                waterMesh.transform.localScale = 2 * (new Vector3(radius + seaLevelOffset, radius + seaLevelOffset, radius + seaLevelOffset));
            }
            display.DrawMesh(SphereMagic.CreatePlanet(PlanetItterations, radius, baseModule, heightMultiplier, regions), mapTexture);
        }
        #endregion

        #region Flat Terrain Generator


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
        if (drawInProgress)
        {
            HaltThreads();
        }

        latestTimeProcessRequested = DateTime.Now;
        drawInProgress = true;
        stop = false;
        StartCoroutine(TextureRefiner());
        #endregion
    }

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

    //MFU  Need to make the 2d arrays here 1d arrays to cure the hiccups.
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
    //MFU Need to implement a non multicore rendering call as well
    #region Multithreading Handlers
    //threading handler function
    IEnumerator TextureRefiner()
    {
        MapDisplay display = FindObjectOfType<MapDisplay>();
        noiseThread = new Thread(ProcessTextures);
        noiseThread.Start();
        while (drawInProgress)
        {
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
    {
        MapDisplay display = FindObjectOfType<MapDisplay>();
        noiseMap = updatedMap;
        Debug.Log("updating map");
        mapTexture = GetMapTexture(renderType, noiseMap);
        display.DrawMesh(SphereMagic.CreatePlanet(PlanetItterations, radius, baseModule, heightMultiplier, regions), mapTexture);
        noiseMapUpdateAvailable = false;
    }

    //make this itterative to approach mapwidth
    void ProcessTextures()
    {
           //MFU This needs to be moved downstream somehow
           //I may need to write a seperate texture draw process
           //Specficially for multicore rendering.
        var processTimestamp =  latestTimeProcessRequested;

        int count=0;

        //the crazyness in the for loop's i value is in order to allow it to be
        //used as a resolution manipulator without doing the math multiple times inside a loop

        for (int i = 16; i > 1; i=i/2)
        {
            if(processTimestamp != latestTimeProcessRequested)
            {
                Debug.Log("Stopping Thread via Time Check");
                return;
            }
            if (i == 16)
            {
                reset = false;
            }
            count++;
            Noise2D placeHolder;
            Debug.Log("drawing map " + count);
            placeHolder = new Noise2D(mapWidth / i, mapHeight / i, baseModule);
            placeHolder.GenerateSpherical(-90, 90, -180, 180);
            Debug.Log("Map " + count + " drawn");
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

    #endregion

    #region File IO
    //MFU Add Terrain group IO

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

    #endregion

}

    #region Serialized Data Sets

#region NoiseFunctions Serializable Container
[System.Serializable]
public class NoiseFunctions     
{
    public enum NoiseType { Perlin, Billow, RidgedMultifractal, Voronoi, None };
    //[Range(0,1)]
    //public float noiseScale = 0.5f;
    public NoiseType type = NoiseType.Perlin;
    public bool enabled = false;
    public ModuleBase moduleBase;
    
 
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

        return preset;
    }
    #endregion

    //generates the mesh based on selected noise type
    public void MakeNoise()
    {

        if (type == NoiseType.Billow) { moduleBase = new Billow(frequency, lacunarity, persistence, octaves, seed, qualityMode);}
        else if (type == NoiseType.Perlin) { moduleBase = new Perlin(frequency,lacunarity,persistence,octaves,seed,qualityMode);}
        else if (type == NoiseType.Voronoi) { moduleBase = new Voronoi(frequency,displacement,seed,distance);}
        else if (type == NoiseType.RidgedMultifractal) { moduleBase = new RidgedMultifractal(frequency,lacunarity,octaves,seed,qualityMode);}
        else moduleBase = null;


    }
}
#endregion

#region Noise Presets Serializable Container
//used to save an .npr file
[System.Serializable]
public struct NoisePresets
{
    public enum NoiseType { Perlin, Billow, RidgedMultifractal, Voronoi, None };
    public enum QualityMode { Low, Medium, High };
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
#endregion


#region Terrain Groups
//serializable to be accessible in the inspector
[System.Serializable]
public struct TerrainType
{
    public string name;
    public double height;
    public Color color;
}
#endregion

#endregion