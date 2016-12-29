using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibNoise.Unity.Generator;
using LibNoise.Unity.Operator;
using LibNoise.Unity;
public class MapGenerator : MonoBehaviour {

    //Poles
    public double southPole = -90;
    public double northPole = 90;
    public double westPole = -180;
    public double eastPole = 180;

    //probaly going to be a 2:1 ratio here for spheres
    public int width = 4096;
    public int height = 2048;

    //stuff to use to cook the map with
    public string stringSeed = "default";
    private int seed = 100;
    public double circumference = 44236800.0;
    public double minElevation = -8192.0;
    public double maxElevation = 8192.0;
    public double continentalFrequency = 1.0;
    public double continentalLacunarity = 2.17623646324; //this needs to be random, but close to 2.0;
    public double moutainLacunarity = 2.3742583832321231; // same
    public double hillLacunarity = 2.18383423423412671; // yep
    public double plainsLacunarity = 2.212845213145; // also
    public double desertLacunarity = 2.2828374858321; //last
    public double mountainSwirl = 1.0;
    public double hillSwirl = 1.0;
    public double desertSwirl = 1.0;
    [Range(-1, 1)]
    public double seaLevel = 0.0;
    public double shelfLevel = -0.375;
    public double mountainsAmount = 0.5;
    public double hillAmount = 1.5d / 2d;
    public double desertAmount = .03125; //deserts can cover all terrain
    public double terrainOffset = 1.0;
    public double mountainGlaciation = 1.275; //amount of ice, close to but greater than 1
    public double continentHeightScale = 0.25; //1 quarter of one to sea level
    const double riverDepth = 0.0234375;


    public Texture2D MakeMap()
    {
        Texture2D texture = new Texture2D(width, height);
        return texture;
    }

    public Perlin MakeContinent()
    {
        int octaves = 14;
        double persistance = 0.5;
        QualityMode qualityMode;
        qualityMode = QualityMode.High;
        Perlin perlin = new Perlin(continentalFrequency, continentalLacunarity, persistance, octaves,seed, qualityMode);
        return perlin;
    }

    public Curve CurveContinent(Perlin perlin)
    {
        Curve curve = new Curve(perlin);
        curve.Add(-2.0000 + seaLevel, -1.625 + seaLevel);
        curve.Add(-1.0000 + seaLevel, -1.375 + seaLevel);
        curve.Add( 0.0000 + seaLevel, -0.375 + seaLevel);
        curve.Add( 0.0625 + seaLevel,  0.125 + seaLevel);
        curve.Add( 0.1250 + seaLevel,  0.250 + seaLevel);
        curve.Add( 0.2500 + seaLevel,  1.000 + seaLevel);
        curve.Add( 0.5000 + seaLevel,  0.250 + seaLevel);
        curve.Add( 0.7500 + seaLevel,  0.250 + seaLevel);
        curve.Add( 1.0000 + seaLevel,  0.500 + seaLevel);
        curve.Add( 2.0000 + seaLevel,  0.500 + seaLevel);
        return curve;
    }

    public Perlin Carver()
    {
        double persistance = 0.5;
        int octaves = 11;
        double offsetFreq = 4.34375;
        Perlin perlin = new Perlin(continentalFrequency * offsetFreq, continentalLacunarity, persistance, octaves, seed + 1, QualityMode.Medium);
        return perlin;
    }

    public ScaleBias CarverScaler(Perlin perlin)
    {
        double scale = 0.375;
        double bias = 0.625;
        ScaleBias scaleBias = new ScaleBias(scale, bias, perlin);
        return scaleBias;
    }

    public Min CarvedContinent(ScaleBias scaleBias, Curve curve)
    {
        Min min = new Min(scaleBias, curve);
        return min;
    }

    public Clamp clampedContinent(Min min)
    {
        Clamp clamp = new Clamp(-1.0, 1.0, min);
        return clamp;
    }

    public Cache cacheContinent(Clamp clamp)
    {
        Cache cache = new Cache(clamp);
        return cache;
    }


}
