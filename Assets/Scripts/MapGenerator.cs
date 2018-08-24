using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    [HideInInspector]
    public NoiseFunction EntryPoint { get; set; }
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
    public NoiseFunction [] noiseFunctions = null;

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
    /// 
    public void GroundGrowing()
    {
        ///</summary>
        ///Things I need to add:
        ///GenarateMap(ModuleBase);
        //


        #region variables and setup


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
        EntryPoint = new NoiseFunction();
        baseModule = EntryPoint.MakeNoise();


        
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

        #region Start Multithreaded Noisemapping
        if (multithreading) { ThreadMap(); }

        #endregion
    }


    public void GenerateMap()
    {
        ///</summary>
        ///Things I need to add:
        ///GenarateMap(ModuleBase);
        //


        #region variables and setup

        //This function prevents the generation from happening if there is no noise stack to process.
        if ((noiseFunctions == null)||(noiseFunctions.Length < 1))
        {
            noiseFunctions = new NoiseFunction[1];
            noiseFunctions[0] = new NoiseFunction();
            noiseFunctions[0].GetDefault();
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
        baseModule = NoiseProcessor.InitNoise(noiseFunctions, seedValue);
 
        
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

    public void SavePresets(NoiseFunction[] savedPresets, string destpath)//saves the map to a given string location.
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
            NoiseFunction[] holder = new NoiseFunction[loadedPresets.Length];
            for (int i = 0; i < loadedPresets.Length; i++)
            {
                holder[i] = new NoiseFunction(loadedPresets[i]);
            }
            noiseFunctions = new NoiseFunction[holder.Length];
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