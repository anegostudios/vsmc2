using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.Rendering.ProbeAdjustmentVolume;

namespace VSMC
{
    /// <summary>
    /// Texture Manager! Responsible for loading, retrieving, and editing textures.
    /// </summary>
    public class TextureManager : MonoBehaviour
    {

        public static TextureManager main;
        public Material modelMaterial;
        public Material transparentMaterial;
        public TextureManagerOverlay overlay;

        //public string textureBasePath;
        
        //Texture stuff, loaded from shape to go back into shape afterwards.
        

        //Runtime things
        public int maxTextureSize = 512;
        public string textureBasePath;
        public Texture2DArray shapeTextureArray;
        public List<LoadedTexture> loadedTextures;
        public LoadedTexture emptyTexture;

        private void Awake()
        {
            main = this;
        }

        /// <summary>
        /// This will get the values we need from a loaded shape.
        /// </summary>
        public void LoadTexturesFromShape(Shape shape)
        {
            //Properties we want to use from Shape are...
            //Shape.Textures
            //Shape.TextureWidth;
            //Shape.TextureHeight
            //Shape.TextureSizes

            emptyTexture = new LoadedTexture("", "");
            emptyTexture.ResolveTextureSize(ShapeHolder.CurrentLoadedShape);

            //This actually loads the textures. 
            int i = 0;
            loadedTextures = new List<LoadedTexture>();
            foreach (var pair in shape.Textures)
            {
                loadedTextures.Add(new LoadedTexture(pair.Key, pair.Value));
                LoadTexture(loadedTextures[i]);
                loadedTextures[i].ResolveTextureSize(shape);
                i++;
            }
            RegenerateTextureArray();

            // We resolve all the textures and whatnot for each element.
            foreach (ShapeElement elem in shape.Elements)
            {
                elem.ResolveFacesAndTextures(loadedTextures);
            }

        }

        /// <summary>
        /// This will store the values into a loaded shape. Needs to be called before saving!
        /// </summary>
        public void ApplyTexturesIntoShape(Shape shape)
        {
            shape.Textures = new Dictionary<string, string>();
            shape.TextureSizes = new Dictionary<string, int[]>();
            foreach (LoadedTexture texture in loadedTextures)
            {
                shape.Textures.Add(texture.code, texture.path);
                shape.TextureSizes.Add(texture.code, new int[] { texture.storedWidth, texture.storedHeight });
            }
        }

        public void LoadTexture(LoadedTexture tex)
        {
            LoadedTexture.LoadedTextureError valid = tex.LoadTextureFromCodeAndPath();
            tex.error = valid;  
        }

        /// <summary>
        /// This'll regenerate the texture array based on all the loaded textures.
        /// </summary>
        public void RegenerateTextureArray()
        {
            //get max tex size.
            maxTextureSize = 16;
            foreach (var tex in loadedTextures)
            {
                if (tex.loadedTexture != null) maxTextureSize = Mathf.Max(maxTextureSize, tex.loadedTexture.width, tex.loadedTexture.height);
            }

            //If we don't have any loaded textures, create an empty white texture.
            if (loadedTextures == null || loadedTextures.Count == 0)
            {
                Color[] colors = new Color[maxTextureSize * maxTextureSize];
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = Color.white;
                }
                shapeTextureArray = new Texture2DArray(maxTextureSize, maxTextureSize, 1, TextureFormat.ARGB32, false);
                shapeTextureArray.SetPixels(colors, 0);
                shapeTextureArray.Apply();
                modelMaterial.SetTexture("_AvailableTextures", shapeTextureArray);
                transparentMaterial.SetTexture("_AvailableTextures", shapeTextureArray);
                return;
            }

            //Each texture needs adding to the texture array.
            int tIndex = 0;
            shapeTextureArray = new Texture2DArray(maxTextureSize, maxTextureSize, loadedTextures.Count, TextureFormat.ARGB32, false);
            shapeTextureArray.filterMode = FilterMode.Point;
            shapeTextureArray.wrapMode = TextureWrapMode.Clamp;
            foreach (var tex in loadedTextures)
            {
                // This may seem to be quite odd code.
                //  The material works by using a Unity texture array - The shader then accesses the textures based on that.
                //  However, the texture array requires that all its textures are of the same size. Because of this, any loaded texture has to be padded to the correct size.

                // Also the damn UVs start from bottom-left, whereas texture sizes start from top-left.
                // Means I can't just set pixels, I have to hack it. It doesn't take too long really.

                Texture2D created = new Texture2D(maxTextureSize, maxTextureSize);
                if (tex.loadedTexture != null) 
                {
                    for (int x = 0; x < tex.loadedTexture.width; x++)
                    {
                        for (int y = 0; y < tex.loadedTexture.height; y++)
                        {
                            created.SetPixel(x, tex.loadedTexture.height - 1 - y, tex.loadedTexture.GetPixel(x, y));
                            
                            //This will probably need to change, but it sets the pixel to be completely empty if transparent.
                            /*
                            if (tex.loadedTexture.GetPixel(x, y).a < 0.5)
                            {
                                created.SetPixel(x, tex.loadedTexture.height - 1 - y, new Color(0, 0, 0, 0));
                            }
                            */
                        }
                    }
                    created.Apply();
                }
                //LoadedTextures is the texture array. We set the pixels of texture 'i'.
                shapeTextureArray.SetPixels(created.GetPixels(), tIndex);
                tIndex++;
            }
            shapeTextureArray.Apply();
            OnTexturePropertiesChanged();
        }

        public void ChangeTextureBasePath(string newBasePath)
        {
            textureBasePath = newBasePath;
            Shape shape = ShapeHolder.CurrentLoadedShape;
            int i = 0;
            foreach (LoadedTexture tex in loadedTextures)
            {
                LoadTexture(loadedTextures[i]);
                loadedTextures[i].ResolveTextureSize(shape);
                i++;
            }
            RegenerateTextureArray();
        }

        public bool ChangeTextureCode(LoadedTexture tex, string newCode)
        {
            string oldCode = tex.code;
            tex.code = newCode;
            foreach(ShapeElement elem in ShapeElementRegistry.main.GetAllShapeElements())
            {
                foreach (ShapeElementFace face in elem.FacesResolved)
                {
                    if (face?.ResolvedTexture == oldCode)
                    {
                        face.Texture = "#"+newCode;
                    }
                }
            }
            
            //Need to reapply the texture to the faces.
            foreach (ShapeElement elem in ShapeElementRegistry.main.GetAllShapeElements())
            {
                elem.ResolveFacesAndTextures(loadedTextures);
            }
            OnTexturePropertiesChanged();
            return true;
        }

        public void SetTextureSize(LoadedTexture tex, int newWidth, int newHeight)
        {
            tex.storedWidth = newWidth;
            tex.storedHeight = newHeight;
            ApplyTexturesIntoShape(ShapeHolder.CurrentLoadedShape);
            tex.ResolveTextureSize(ShapeHolder.CurrentLoadedShape);
            OnTexturePropertiesChanged();
        }

        public bool ChangeTexturePath(LoadedTexture tex, string newPath)
        {
            //Actually this one's really easy!
            //Edit: After second thought, no it's not.
            //  Changing the texture path should really auto change the texture size too. Oh well, do that later...
            tex.path = newPath;
            tex.LoadTextureFromCodeAndPath();
            RegenerateTextureArray();
            OnTexturePropertiesChanged();
            return true;
        }

        public void OnTextureArrayModified()
        {
            //Need to reapply the texture to the faces.
            foreach (ShapeElement elem in ShapeElementRegistry.main.GetAllShapeElements())
            {
                elem.ResolveFacesAndTextures(loadedTextures);
            }
            RegenerateTextureArray();
            OnTexturePropertiesChanged();
        }

        /// <summary>
        /// Refereshes the shape based on loaded textures.
        /// </summary>
        public void OnTexturePropertiesChanged()
        {
            modelMaterial.SetTexture("_AvailableTextures", shapeTextureArray);
            transparentMaterial.SetTexture("_AvailableTextures", shapeTextureArray);
            foreach (ShapeElement elem in ShapeElementRegistry.main.GetAllShapeElements())
            {
                elem.RecreateObjectMesh();
            }
        }

        public TextureManagerOverlay GetTextureOverlay()
        {
            return overlay;
        }

    }
}