using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour {

    public Renderer TextureRender;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public void DrawTextureOnPlane(Texture2D texture)
    {
        MapGenerator mapGen = FindObjectOfType<MapGenerator>();
        TextureRender.sharedMaterial.mainTexture = texture;
        if (mapGen.renderType == MapGenerator.RenderType.FlatMap)
        {
            TextureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);
        }

        else
        {
            TextureRender.transform.localScale = new Vector3(texture.height, texture.height, texture.height);
        }
    }


    public void DrawMesh(MeshData meshData, Texture2D texture)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();
        meshRenderer.sharedMaterial.mainTexture = texture;

    }

    public void DrawMesh(Mesh mesh, Texture2D texture)
    {
        meshFilter.sharedMesh = mesh;
        meshRenderer.sharedMaterial.mainTexture = texture;
    }
    
}
