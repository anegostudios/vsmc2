using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using UnityEngine;

[System.Serializable]
public class ShapeJSON
{

    public static int MaxTextureSize = 128;
    


    /// <summary>
    /// The collection of textures in the shape. The Dictionary keys are the texture short names, used in each ShapeElementFace
    /// </summary>
    public Dictionary<string, string> Textures;

    /// <summary>
    /// The elements of the shape.
    /// </summary>
    public ShapeElementJSON[] Elements;

    /// <summary>
    /// The animations for the shape.
    /// We're gonna hold off on animations right now.
    /// </summary>
    //public AnimationJSON[] Animations;

    /// <summary>
    /// The width of the texture. (default: 16)
    /// </summary>
    public int TextureWidth = 16;

    /// <summary>
    /// The height of the texture (default: 16) 
    /// </summary>
    public int TextureHeight = 16;

    public Vector2[] TextureSizeMultipliers;

    public Dictionary<string, int[]> TextureSizes = new Dictionary<string, int[]>();

    public Texture2DArray loadedTextures;

    [OnDeserialized()]
    public void ResolveFacesAndTextures(StreamingContext context)
    {
        TextureSizeMultipliers = new Vector2[Textures.Count];
        foreach (ShapeElementJSON elem in Elements)
        {
            elem.ResolveFacesAndTextures(Textures);
        }
        loadedTextures = new Texture2DArray(MaxTextureSize, MaxTextureSize, Textures.Count, TextureFormat.RGBA32, false);
        loadedTextures.filterMode = FilterMode.Point;
        loadedTextures.wrapMode = TextureWrapMode.Clamp;
        int i = 0;
        foreach (var pair in Textures)
        {
            string path = "C:\\Games\\Vintagestory120\\assets\\survival\\textures\\" + pair.Value + ".png";
            if (File.Exists(path))
            {
                try
                {
                    Texture2D load = new Texture2D(0, 0);
                    load.LoadImage(File.ReadAllBytes(path));
                    Debug.Log("Successfully loaded image " + pair.Key);

                    if (TextureSizes.ContainsKey(pair.Key))
                    {
                        TextureSizeMultipliers[i] = new Vector2((float)TextureSizes[pair.Key][0] / load.width, (float)TextureSizes[pair.Key][1] / load.height);
                    }
                    else
                    {
                        TextureSizeMultipliers[i] = Vector2.one;
                    }

                    Texture2D created = new Texture2D(MaxTextureSize, MaxTextureSize);
                    for (int x = 0; x < load.width; x++)
                    {
                        for (int y = 0; y < load.height; y++)
                        {
                            created.SetPixel(x, load.height - 1 - y, load.GetPixel(x, y));
                        }
                    }
                    //created.SetPixels(0, 0, load.width, load.height, load.GetPixels());
                    created.Apply();
                    loadedTextures.SetPixels(created.GetPixels(), i);
                }
                catch (System.Exception e)
                {
                    Debug.Log("Failed to load image " + pair.Key);
                    Debug.LogException(e);
                }
            }
            i++;
        }
        loadedTextures.Apply();
    }

}
