using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibNoise;
using LibNoise.Unity;
using LibNoise.Unity.Generator;

public static class FlatMeshGenerator {

//function to take a noisemap and adjust a flat terrainmesh according to that map.
    public static MeshData GenerateTerrainMesh(Noise2D  noisemap, float multiplier, AnimationCurve adjustment)
    {

        float topLeftX = (noisemap.Width - 1) / 2f;
        float topLeftZ = (noisemap.Height - 1) / 2f;
        //noisemap.GeneratePlanar();
        MeshData meshData = new MeshData(noisemap.Width, noisemap.Height);
        int vertexIndex = 0;

        for (int y = 0; y < noisemap.Height; y++)
        {
            for (int x = 0; x < noisemap.Width; x++)
            {
                meshData.vertices[vertexIndex] = new Vector3(x - topLeftX, adjustment.Evaluate(noisemap[x, y]) * multiplier, topLeftZ - y);
                meshData.uvs[vertexIndex] = new Vector2(x / (float)noisemap.Width, y / (float)noisemap.Height);
                if (x < noisemap.Width - 1 && y < noisemap.Height - 1)
                {
                    meshData.AddTriangle(vertexIndex, vertexIndex + noisemap.Width + 1, vertexIndex + noisemap.Width);
                    meshData.AddTriangle(vertexIndex+noisemap.Width+1, vertexIndex, vertexIndex + 1);
                }
                vertexIndex++;
            }

        }

        return meshData;

    }
}

public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;


    int triangleIndex;

    public MeshData(int meshWidth, int meshHeight)
    {
        vertices = new Vector3[meshWidth * meshHeight];
        uvs = new Vector2[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];

    }
    public void AddTriangle (int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex+1] = b;
        triangles[triangleIndex+2] = c;
        triangleIndex += 3;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }
}
