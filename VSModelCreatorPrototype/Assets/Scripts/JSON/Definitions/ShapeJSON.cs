using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShapeJSON
{

    /// <summary>
    /// The collection of textures in the shape. The Dictionary keys are the texture short names, used in each ShapeElementFace
    /// <br/>Note: from game version 1.20.4, this is <b>null on server-side</b> (except during asset loading start-up stage)
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

    public Dictionary<string, int[]> TextureSizes = new Dictionary<string, int[]>();


}
