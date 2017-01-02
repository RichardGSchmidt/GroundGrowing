using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class SphereMagic : MonoBehaviour
{

    public int subdivisions = 0;

    //heightmap will modify radius
    public float radius = 1f;  //preserve this
    //radius adjustment fuction that is per x,y,z based on return from libnoise noise values (or heightmap)

    private void Awake()
    {
        GetComponent<MeshFilter>().mesh = MagicSphere.Create(subdivisions, radius);
    }
}


public static class MagicSphere
{
    public static Mesh Create (int subdivisions, float radius)
    {
        Vector3[] verts =
        {
            Vector3.down,
            Vector3.forward,
            Vector3.left,
            Vector3.back,
            Vector3.right,
            Vector3.up
        };

        int[] triangles =
        {
            0,1,2,
            0,2,3,
            0,3,4,
            0,4,1,

            5,2,1,
            5,3,2,
            5,4,3,
            5,1,4
        };

        //every time the sphere isn't a unit sphere the radius will adjust
        //the nosie will likely have to be imbeded in some future variation of this
        //loop

        Vector3[] normals = new Vector3[verts.Length];
        Normalize(verts, normals);

        if (radius != 1f)
        {
            for (int i = 0; i < verts.Length; i++)
            {
                verts[i] *= radius;
            }
        }

        Mesh mesh = new Mesh();
        mesh.name  = "MagicSphere";
        mesh.vertices = verts;
        mesh.normals = normals;
        mesh.triangles = triangles;
        return mesh;
    }

    private static void Normalize (Vector3[] vertices, Vector3[] norms)
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            norms[i] = vertices[i] = vertices[i].normalized;
        }
    }
}