using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour {

    public Renderer TextureRender;

    public void DrawTextureOnPlane(Texture2D texture)
    {
        TextureRender.sharedMaterial.mainTexture = texture;
        TextureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }
}
