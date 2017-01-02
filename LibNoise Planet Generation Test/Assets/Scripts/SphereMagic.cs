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

        Vector2[] uvs = new Vector2[verts.Length];
        CreateUV(verts, uvs);

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
        mesh.uv = uvs;
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

    private static void CreateUV(Vector3[] verts, Vector2[]uvs)
    {
        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 v = verts[i];
            Vector2 textCoords;
            textCoords.x = Mathf.Atan2(v.x, v.z) / (-2 * Mathf.PI);
            if (textCoords.x < 0f)
            {
                textCoords.x += 1f;
            }

            textCoords.y = Mathf.Asin(v.y) / Mathf.PI + 0.5f;
            uvs[i] = textCoords;
        }

    }
}