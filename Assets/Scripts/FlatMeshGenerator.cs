using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibNoise;
using LibNoise.Unity;
using LibNoise.Unity.Generator;

public static class FlatMeshGenerator {

//function to take a noisemap and adjust a flat terrainmesh according to that map.
    public static void GenerateTerrainMesh(Noise2D  noisemap)
    {
        //noisemap.GeneratePlanar();  
    }
}

public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;

    int triangleIndex;

    public MeshData(int meshWidth, int meshHeight)
    {
        vertices = new Vector3[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];

    }
    public void AddTriangle (int a, int b, int c)
    {

    }
}
