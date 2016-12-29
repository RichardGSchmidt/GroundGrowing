using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibNoise.Unity.Generator;
using LibNoise.Unity.Operator;
using LibNoise.Unity;

public class MapGenerator : MonoBehaviour {

    public int mapWidth;
    public int mapHeight;
    private Noise2D noiseMap = null;
    public Texture2D[] textures = new Texture2D[3];
    public NoiseFunctions[] noiseFunctions;
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
        //noiseMap.GenerateSpherical(-1, 1, -1, 1);  //doesn't do what I thought it would
        noiseMap.GeneratePlanar(-1, 1, -1, 1, true);  //the 5 argument version is good for sphere's, may use falloff map for poles and generate seperate meshes for icecaps, not sure
        //noiseMap.GenerateSpherical(-1, 1, -1, 1); //in this position too

        //noiseMap.GenerateCylindrical(-1, 1, -1, 1);
        textures[0] = noiseMap.GetTexture(LibNoise.Unity.Gradient.Grayscale);
        textures[0].Apply();
        MapDisplay display = FindObjectOfType<MapDisplay>();
        display.DrawTextureOnPlane(textures[0]);
    }

    
}

//trying to build this entire program usable for world generation from the editor, encapsulating effects in orderable serialized classes for this reason.
[System.Serializable]
public class NoiseFunctions
{
    public enum NoiseType { Perlin, Billow, RiggedMultifractal, Voronoi};
    public float noiseScale = 0.5f;
    public NoiseType type = NoiseType.Perlin;
    public bool enabled = false;
    public Noise2D noise2d = null;
    public ModuleBase moduleBase;
    public int height;
    public int width;
   

    public void FormMesh()
    {

        if (type == NoiseType.Billow) { moduleBase = new Billow(); noise2d = new Noise2D(height,width,moduleBase); }
        else if (type == NoiseType.Perlin) { moduleBase = new Perlin(); noise2d = new Noise2D(height, width, moduleBase); }
        else if (type == NoiseType.Voronoi) { moduleBase = new Voronoi(); noise2d = new Noise2D(height, width, moduleBase); }
        else if (type == NoiseType.RiggedMultifractal) { moduleBase = new RiggedMultifractal(); noise2d = new Noise2D(height, width, moduleBase); }
        else moduleBase = null;
    }
}
