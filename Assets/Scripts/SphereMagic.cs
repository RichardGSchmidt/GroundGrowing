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
            Vector3.down, Vector3.down, Vector3.down, Vector3.down,
            Vector3.forward,
            Vector3.left,
            Vector3.back,
            Vector3.right,
            Vector3.up, Vector3.up, Vector3.up, Vector3.up
        };
       
        int[] triangles =
        {
            //notice that they count down sequentially in a pattern
            0, 4, 5,
            1, 5, 6,
            2, 6, 7,
            3, 7, 8,

             9, 5, 4,
            10, 6, 5,
            11, 7, 6,
            12, 8, 7
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
            float previousX = 1f;
            
            Vector3 vert = verts[i];
            if (vert.x == previousX)
            {
                uvs[i - 1].x = 1f;
            }
            Vector2 textureCoords;
            textureCoords.x = Mathf.Atan2(vert.x, vert.z) / (-2f * Mathf.PI);
            if (textureCoords.x < 0f)
            {
                textureCoords.x += 1f;

            }
            
            Vector2 textCoords;

            textCoords.x = Mathf.Atan2(vert.x, vert.z) / (-2 * Mathf.PI);
            if (textCoords.x < 0f)
            {
                textCoords.x += 1f;
            }

            textCoords.y = Mathf.Asin(vert.y) / Mathf.PI + 0.5f;
            uvs[i] = textCoords;
        }

        uvs[verts.Length - 4].x = uvs[0].x = 0.125f;
        uvs[verts.Length - 3].x = uvs[1].x = 0.375f;
        uvs[verts.Length - 2].x = uvs[2].x = 0.625f;
        uvs[verts.Length - 1].x = uvs[3].x = 0.875f;

    }
}