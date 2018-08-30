using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour {

    public Renderer TextureRender;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    //used for the flat map generator
    public void DrawMesh(MeshData meshData, Texture2D texture)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();
        meshRenderer.sharedMaterial.mainTexture = texture;
        meshRenderer.sharedMaterial.mainTextureOffset = new Vector2(-0.0f, 0);
        TextureRender.transform.localScale = new Vector3(texture.height, texture.height, texture.height);

    }

    public void SetTexture(Texture2D texture)
    {
        meshRenderer.sharedMaterial.mainTexture = texture;
    }

    public void DrawMesh(Mesh mesh, Texture2D texture)
    {
        meshFilter.sharedMesh = mesh;
        meshRenderer.sharedMaterial.mainTexture = texture;
        //noisemap texture is off by a quarter left on it's axis versus the mesh.
        meshRenderer.sharedMaterial.mainTextureOffset = new Vector2(-0.25f,0);
        TextureRender.transform.localScale = new Vector3(1, 1, 1);

    }
    
}
