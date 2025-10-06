using System;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.ProbeAdjustmentVolume;

namespace VSMC
{
    /// <summary>
    /// Stores a single texture that has been loaded for a model.
    /// </summary>
    public class LoadedTexture
    {
        public string code;
        public string path;
        public Texture2D loadedTexture;
        public LoadedTextureError error;
        public int storedWidth;
        public int storedHeight;
        public Vector2 storedTextureSizeMultiplier;


        public enum LoadedTextureError
        {
            Valid,
            InvalidFilePath,
            CouldntLoad
        }

        public LoadedTexture(string textureCode, string texturePath)
        {
            code = textureCode;
            path = texturePath;
        }

        public LoadedTextureError LoadTextureFromCodeAndPath()
        {
            //Probably a good idea to unload an already loaded texture if it exists.
            if (loadedTexture != null)
            {
                GameObject.Destroy(loadedTexture);
                loadedTexture = null;
            }

            string fullPath = Path.Combine(TextureManager.main.textureBasePath, path + ".png");

            if (!File.Exists(fullPath)) return LoadedTextureError.InvalidFilePath;
                
            //Load the texture
            try
            {
                Texture2D load = new Texture2D(0, 0);
                load.LoadImage(File.ReadAllBytes(fullPath));
                loadedTexture = load;
                loadedTexture.filterMode = FilterMode.Point;
            }
            catch (System.Exception e)
            {
                Debug.Log("Failed to load image " + code);
                Debug.LogException(e);
                return LoadedTextureError.CouldntLoad;
            }
            ResolveTextureSize(ShapeLoader.main.shapeHolder.cLoadedShape);
            return LoadedTextureError.Valid;
        }

        public void ResolveTextureSize(Shape shape)
        {
            /*
             * I really need to figure out the texture sizes. This is odd to me.
             */

            if (shape.TextureSizes.ContainsKey(code))
            {
                storedWidth = shape.TextureSizes[code][0];
                storedHeight = shape.TextureSizes[code][1];
            }
            else
            {
                storedWidth = shape.TextureWidth;
                storedHeight = shape.TextureHeight;
            }

            if (loadedTexture == null) return;
            storedTextureSizeMultiplier = new Vector2((float)storedWidth / loadedTexture.width, (float)storedHeight / loadedTexture.height);
        }

    }
}