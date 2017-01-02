using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour {

    public Renderer TextureRender;
    

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
}
