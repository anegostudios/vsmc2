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
    /// </summary>
    public VSAnimation[] Animations;

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
        // This code is testing crap that will be rewritten.
        // We resolve all the textures and whatnot for each element.
        TextureSizeMultipliers = new Vector2[Textures.Count];
        foreach (ShapeElementJSON elem in Elements)
        {
            elem.ResolveFacesAndTextures(Textures);
        }
        
        //This actually loads the textures. 
        // Yes, I'm very aware that this is hardcoded to use my own filepaths. Later this will be in a completely seperate class (i.e. TextureManager)
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

                    // This may seem to be quite odd code - And again, is going to be rewritten. But...
                    //  The material works by using a Unity texture array - The shader then accesses the textures based on that.
                    //  However, the texture array requires that all its textures are of the same size. BEcause of this, any loaded texture has to be padded to the correct size.
                    //  There is definitely a better way of doing this - Either using a single texture that with custom calculated UVs - Or by using a texture array that recalculates size based on the largest texture.

                    //Also the damn UVs start from bottom-left, whereas texture sizes start from top-left.
                    //Means I can't just set pixels, I have to hack it. It doesn't take too long really.
                    Texture2D created = new Texture2D(MaxTextureSize, MaxTextureSize);
                    for (int x = 0; x < load.width; x++)
                    {
                        for (int y = 0; y < load.height; y++)
                        {
                            created.SetPixel(x, load.height - 1 - y, load.GetPixel(x, y));
                        }
                    }
                    created.Apply();
                    
                    //LoadedTextures is the texture array. We set the pixels of texture 'i'.
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
