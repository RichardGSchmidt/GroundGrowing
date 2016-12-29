using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibNoise.Unity.Generator;
using LibNoise.Unity.Operator;
using LibNoise.Unity;

public class MapGenerator : MonoBehaviour {

    public enum MapType {NoiseMap, HeightMap};
    public MapType mapType;
    public int mapWidth;
    public int mapHeight;
    public string seed;
    public bool useRandomSeed = true;
    public bool autoUpdate = false;
    [HideInInspector]
    public int seedValue = 0;
    private Noise2D noiseMap = null;
    public Texture2D[] textures = new Texture2D[3];
    public NoiseFunctions[] noiseFunctions;
    public TerrainType[] regions;
    private ModuleBase baseModule = null;


    public void GenerateMap()
    {
        baseModule = null;
        noiseMap = null;
        for (int i = 0; i < noiseFunctions.Length; i++)
        {
            if (noiseFunctions[i].enabled)
            {
                noiseFunctions[i].FormMesh();
            }
        }
        for (int i = 0; i < noiseFunctions.Length; i++)
        {
            if (baseModule == null&&noiseFunctions[i].enabled)
            {
                baseModule = noiseFunctions[i].moduleBase;
            }
            else if(noiseFunctions[i].enabled)
            {
                baseModule = new Add(baseModule, noiseFunctions[i].moduleBase);
            }
        }

        noiseMap = new Noise2D(mapWidth, mapHeight, baseModule);
        //noiseMap.GenerateSpherical(-1, 1, -1, 1);  //Needs research
        noiseMap.GeneratePlanar(-1, 1, -1, 1, true);  //the 5 argument version is good for sphere's, may use falloff map for poles and generate seperate meshes for icecaps, not sure

        if (mapType == MapType.NoiseMap)
        {
            textures[0] = noiseMap.GetTexture(LibNoise.Unity.Gradient.Grayscale);
            textures[0].Apply();
        }
        else if (mapType == MapType.HeightMap)
        {
            Color[] colorMap = new Color[mapWidth * mapHeight];
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
        }
        MapDisplay display = FindObjectOfType<MapDisplay>();
        display.DrawTextureOnPlane(textures[0]);
    }

    
}

//trying to build this entire program usable for world generation from the editor, encapsulating effects in orderable serialized classes for this reason.
[System.Serializable]
public class NoiseFunctions
{
    public enum NoiseType { Perlin, Billow, RiggedMultifractal, Voronoi };
    [Range(0,1)]
    public float noiseScale = 0.5f;
    public NoiseType type = NoiseType.Perlin;
    public bool enabled = false;
    public ModuleBase moduleBase;
    [HideInInspector]
    public int height;
    [HideInInspector]
    public int width;

    //Billow and Perlin Inputs
    [Range(0f,20f)]
    public double frequency; // on all
    [Range(2.0000000f, 2.5000000f)]
    public double lacunarity; // on all but voronoi
    //[HideInInspector]
    [Range(0f, 1f)]
    public double persistence;  //Not on Rigged MultiFractal or voronoi
    //[HideInInspector]
    [Range(1,18)]
    public int octaves;//all but voronoi
    [HideInInspector]
    public int seed;//yeah all
    public QualityMode qualityMode;//not on voronoi
    //Voronoi Inputs also frequency and seed from previous
    //[HideInInspector]
    public double displacement;
    //[HideInInspector]
    public bool distance;




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
public struct TerrainType
{
    public string name;
    public double height;
    public Color color;
}

