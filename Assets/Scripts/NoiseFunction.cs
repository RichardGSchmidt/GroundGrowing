using System.Collections;
using System.Collections.Generic;
using LibNoise;
using LibNoise.Generator;


using UnityEngine;

public class NoiseFunction
{
    public enum BlendMode { Add, Subtract };
    public enum NoiseType { Perlin, Billow, RidgedMultifractal, Voronoi, None };
    //[Range(0,1)]
    //public float noiseScale = 0.5f;
    public NoiseType type = NoiseType.Perlin;
    public bool enabled = false;
    public NoiseFunction noiseParent;
    public NoiseFilter[] noiseFilters;

    [Range(0f, 20f)]
    public double frequency;
    [Range(2.0000000f, 2.5000000f)]
    public double lacunarity;
    [Range(0f, 1f)]
    public double persistence;
    [Range(1, 18)]
    public int octaves;
    public int seed;
    public BlendMode blendMode;
    public QualityMode qualityMode;

    public double displacement;
    public bool distance;

    public NoiseFunction()
    {
        type = NoiseType.None;
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

    public void GetDefault()
    {
        type = NoiseType.Perlin;
        enabled = true;
        frequency = 1.1;
        lacunarity = 2.2;
        persistence = 0.6;
        octaves = 7;
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
    public NoiseFunction(NoisePresets presets)
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
  
public abstract class NoiseJoiner
{
    public int mode;
    public NoiseFunction[] joinedNoises;
}

public abstract class NoiseFilter
{
    public int mode;
}

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