using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace VSMC
{
    [System.Serializable]
    public class ShapeElementFace
    {

        /// <summary>
        /// The texture of the face.
        /// </summary>
        public string Texture;

        [NonSerialized]
        public string ResolvedTexture;

        /// <summary>
        /// The UV array of the face.
        /// </summary>
        public float[] Uv;

        public EnumReflectiveMode ReflectiveMode;

        public sbyte[] WindMode;

        public sbyte[] WindData;

        /// <summary>
        /// The rotation of the face.
        /// </summary>
        public float Rotation;

        /// <summary>
        /// The glow on the face.
        /// </summary>
        public int Glow;

        /// <summary>
        /// Whether or not the element is enabled.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool Enabled = true;

        /// <summary>
        /// The index of the texture to use. -1 if texture cannot be found.
        /// </summary>
        [NonSerialized]
        public int textureIndex;

        public ShapeElementFace()
        {
            Texture = null;
            Uv = new float[] { 0, 0, 1, 1 };
        }

        public void ResolveTexture(List<LoadedTexture> textures)
        {
            //If texture is null (i.e. a new shape), give it the first texture found in the texture manager.
            if (Texture == null)
            {
                Texture = "#"+textures[0].code;
            }

            //This essentially just finds the texture index based on the loaded textures.
            //Remove the # from the start of the texture.
            ResolvedTexture = Texture.Substring(1);
            int index = 0;
            foreach (LoadedTexture tex in textures)
            {
                if (tex.code.Equals(ResolvedTexture, System.StringComparison.CurrentCultureIgnoreCase))
                {
                    textureIndex = index;
                    return;
                }
                index++;
            }
            //No texture found - Use first texture as default.
            Debug.LogWarning("No texture found for element with texture " + ResolvedTexture);
            textureIndex = -1;
        }

        public string GetReadableTextureName()
        {
            if (textureIndex == -1) return "Missing Texture...";
            return ResolvedTexture;
        }

        public LoadedTexture GetLoadedTexture()
        {
            if (textureIndex == -1) return TextureManager.main.emptyTexture;
            return TextureManager.main.loadedTextures[textureIndex];
        }

        public void ResolveBeforeSerialization()
        {
            Texture = "#" + ResolvedTexture;
        }

    }
}