using Newtonsoft.Json;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        /// Whether auto resolution should be enabled for this face.
        /// Will only save if turned off. Saves as "autoUv".
        /// </summary>  
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, PropertyName = "autoUv")]
        [DefaultValue(true)]
        public bool autoResolutionForUV = true;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(true)]
        public bool snapUV = true;


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

        public void CalculateAutoUV(Vector2 faceDimension, bool forceAutoAndSnap = false)
        {
            if (autoResolutionForUV || forceAutoAndSnap)
            {

                if (snapUV || forceAutoAndSnap)
                {
                    // We prevent subpixel UV mapping so that one can still resize elements slighty to fix z-fighting
                    // without messing up the UV map
                    Vector2 scale = GetVoxelToPixelScale();

                    Uv[0] = (int)Mathf.Round(Uv[0] * scale.x) / scale.x;
                    Uv[1] = (int)Mathf.Round(Uv[1] * scale.y) / scale.y;

                    float width = faceDimension.x;
                    float height = faceDimension.y;

                    if (RotationIndex == 0 || RotationIndex == 2)
                    {
                        // Math.max because if the element is not even a full pixel wide, we should still use a single pixel to texture it

                        Uv[2] = Uv[0] + Mathf.Max(1 / scale.x, Mathf.Floor(width * scale.x + 0.000001f) / scale.x);      // Stupid rounding errors -.-
                        Uv[3] = Uv[1] + Mathf.Max(1 / scale.y, Mathf.Floor(height * scale.y + 0.000001f) / scale.y);
                    }
                    else
                    {
                        Uv[2] = Uv[0] + Math.Max(1 / scale.x, Mathf.Floor(height * scale.x + 0.000001f) / scale.x);
                        Uv[3] = Uv[1] + Math.Max(1 / scale.y, Mathf.Floor(width * scale.y + 0.000001f) / scale.y);
                    }
                }
                else
                {


                    if (RotationIndex == 0 || RotationIndex == 2)
                    {
                        Uv[2] = Uv[0] + faceDimension.x;
                        Uv[3] = Uv[1] + faceDimension.y;
                    }
                    else
                    {
                        Uv[2] = Uv[0] + faceDimension.y;
                        Uv[3] = Uv[1] + faceDimension.x;
                    }
                }
            }
        }

        /// <summary>
        /// Determines if, when loaded, the face should use autoUV and snapping. 
        /// </summary>
        public void AutomaticallyDisableAutoUVOrSnapping(Vector2 faceDimension)
        {
            float[] testingUV = new float[4];
            // We prevent subpixel UV mapping so that one can still resize elements slighty to fix z-fighting
            // without messing up the UV map
            Vector2 scale = GetVoxelToPixelScale();

            testingUV[0] = (int)Mathf.Round(Uv[0] * scale.x) / scale.x;
            testingUV[1] = (int)Mathf.Round(Uv[1] * scale.y) / scale.y;

            float width = faceDimension.x;
            float height = faceDimension.y;

            if (RotationIndex == 0 || RotationIndex == 2)
            {
                // Math.max because if the element is not even a full pixel wide, we should still use a single pixel to texture it

                testingUV[2] = testingUV[0] + Mathf.Max(1 / scale.x, Mathf.Floor(width * scale.x + 0.000001f) / scale.x);      // Stupid rounding errors -.-
                testingUV[3] = testingUV[1] + Mathf.Max(1 / scale.y, Mathf.Floor(height * scale.y + 0.000001f) / scale.y);
            }
            else
            {
                testingUV[2] = testingUV[0] + Math.Max(1 / scale.x, Mathf.Floor(height * scale.x + 0.000001f) / scale.x);
                testingUV[3] = testingUV[1] + Math.Max(1 / scale.y, Mathf.Floor(width * scale.y + 0.000001f) / scale.y);
            }

            if (testingUV[0] != Uv[0] || testingUV[1] != Uv[1] || testingUV[2] != Uv[2] || testingUV[3] != Uv[3])
            {
                //If the values don't match, then disable snapUV.
                snapUV = false;
            }
            else
            {
                snapUV = true;
                autoResolutionForUV = true;
                return;
            }

            testingUV[0] = Uv[0];
            testingUV[1] = Uv[1];
            if (RotationIndex == 0 || RotationIndex == 2)
            {
                testingUV[2] = Uv[0] + faceDimension.x;
                testingUV[3] = Uv[1] + faceDimension.y;
            }
            else
            {
                testingUV[2] = Uv[0] + faceDimension.y;
                testingUV[3] = Uv[1] + faceDimension.x;
            }

            if (testingUV[0] != Uv[0] || testingUV[1] != Uv[1] || testingUV[2] != Uv[2] || testingUV[3] != Uv[3])
            {
                //If the values don't match, then disable snapUV.
                autoResolutionForUV = false;
            }
            else
            {
                autoResolutionForUV = true;
                return;
            }
        }

        public int RotationIndex
        {
            get
            {
                //Easy rounding.
                if (Rotation < 45) return 0;
                else if (Rotation < 135) return 1;
                else if (Rotation < 225) return 2;
                else return 3;
            }
            set
            {
                Rotation = Mathf.RoundToInt(value * 90);
            }
        }

        public Vector2 GetVoxelToPixelScale()
        {
            return new Vector2(2, 2);
        }

        public float uvWidth(bool forceSnap = false)
        {
            if (!snapUV && !forceSnap) return Mathf.Abs(Uv[2] - Uv[0]);

            // We prevent subpixel UV mapping so that one can still resize elements slighty to fix z-fighting
            // without messing up the UV map
            Vector2 scale = GetVoxelToPixelScale();

            return Mathf.RoundToInt(Mathf.Abs(Uv[2] - Uv[0]) * scale.x) / scale.x;
        }

        public float uvHeight(bool forceSnap = false)
        {
            if (!snapUV && !forceSnap) return Mathf.Abs(Uv[3] - Uv[1]);

            // We prevent subpixel UV mapping so that one can still resize elements slighty to fix z-fighting
            // without messing up the UV map
            Vector2 scale = GetVoxelToPixelScale();

            return Mathf.RoundToInt(Mathf.Abs(Uv[3] - Uv[1]) * scale.y) / scale.y;
        }
    }
}