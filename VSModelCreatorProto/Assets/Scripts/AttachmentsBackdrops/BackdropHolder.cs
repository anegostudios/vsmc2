using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using VSMC;

namespace VSMC
{
    /// <summary>
    /// The Unity object that maintains the backdrop elements.
    /// Backdrop elements don't need to move so they will all be added to the same parent.
    /// </summary>
    public class BackdropHolder : MonoBehaviour
    {

        [Header("Unity References")]
        public GameObject shapeElementPrefab;
        public Transform backdropParent;

        public Material backdropSolidMaterial;
        public Material backdropTransparentMaterial;

        //Textures
        List<LoadedTexture> backdropLoadedTextures;
        int maxTextureSize;

        Shape backdrop;
        List<ShapeElementGameObject> allGameObjectsForBackdrop;

        public void OnBackdropShapeLoaded(Shape loadedShape, BackdropOrAttachmentData data)
        {
            backdrop = loadedShape;
            gameObject.name = data.shapeFilepath;
            gameObject.SetActive(data.enabled);
            backdropSolidMaterial = new Material(backdropSolidMaterial);
            backdropTransparentMaterial = new Material(backdropTransparentMaterial);
            backdropSolidMaterial.SetFloat("_TexturesEnabled", 1);
            backdropTransparentMaterial.SetFloat("_TexturesEnabled", 1);
            loadedShape.ResolveReferencesAndUIDs(true);
            CreateTextures(loadedShape);
            CreateAllShapeElementGameObjects(loadedShape);
            AssignMaterials(data);
        }

        void CreateTextures(Shape shape)
        {
            //Copied from texture manager. 
            int i = 0;
            backdropLoadedTextures = new List<LoadedTexture>();
            foreach (var pair in shape.Textures)
            {
                backdropLoadedTextures.Add(new LoadedTexture(pair.Key, pair.Value));
                backdropLoadedTextures[i].LoadTextureFromCodeAndPath(shape);
                backdropLoadedTextures[i].ResolveTextureSize(shape);
                i++;
            }
            CreateTextureArrayForBackdrop(shape);

            // We resolve all the textures and whatnot for each element.
            foreach (ShapeElement elem in shape.Elements)
            {
                elem.ResolveFacesAndTextures(backdropLoadedTextures);
            }
        }
        
        void CreateTextureArrayForBackdrop(Shape shape)
        {
            //get max tex size.
            maxTextureSize = 16;
            Texture2DArray shapeTextureArray;
            foreach (var tex in backdropLoadedTextures)
            {
                if (tex.loadedTexture != null) maxTextureSize = Mathf.Max(maxTextureSize, tex.loadedTexture.width, tex.loadedTexture.height);
            }

            //If we don't have any loaded textures, create an empty white texture.
            if (backdropLoadedTextures == null || backdropLoadedTextures.Count == 0)
            {
                Color[] colors = new Color[maxTextureSize * maxTextureSize];
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = Color.white;
                }
                shapeTextureArray = new Texture2DArray(maxTextureSize, maxTextureSize, 1, TextureFormat.ARGB32, false);
                shapeTextureArray.SetPixels(colors, 0);
                shapeTextureArray.Apply();
                backdropSolidMaterial.SetTexture("_AvailableTextures", shapeTextureArray);
                backdropTransparentMaterial.SetTexture("_AvailableTextures", shapeTextureArray);
                return;
            }

            //Each texture needs adding to the texture array.
            int tIndex = 0;
            shapeTextureArray = new Texture2DArray(maxTextureSize, maxTextureSize, backdropLoadedTextures.Count, TextureFormat.ARGB32, false);
            shapeTextureArray.filterMode = FilterMode.Point;
            shapeTextureArray.wrapMode = TextureWrapMode.Clamp;
            foreach (var tex in backdropLoadedTextures)
            {
                // This may seem to be quite odd code.
                //  The material works by using a Unity texture array - The shader then accesses the textures based on that.
                //  However, the texture array requires that all its textures are of the same size. Because of this, any loaded texture has to be padded to the correct size.

                // Also the damn UVs start from bottom-left, whereas texture sizes start from top-left.
                // Means I can't just set pixels, I have to hack it. It doesn't take too long really.

                Texture2D created = new Texture2D(maxTextureSize, maxTextureSize);
                Color[] cols = created.GetPixels();
                Color invalidColor = new Color(1, 0, 1, 1);
                for (int i = 0; i < cols.Length; i++)
                {
                    cols[i] = invalidColor;
                }
                created.SetPixels(cols);

                if (tex.loadedTexture != null)
                {
                    for (int x = 0; x < tex.loadedTexture.width; x++)
                    {
                        for (int y = 0; y < tex.loadedTexture.height; y++)
                        {
                            created.SetPixel(x, tex.loadedTexture.height - 1 - y, tex.loadedTexture.GetPixel(x, y));
                        }
                    }
                    created.Apply();
                }
                //LoadedTextures is the texture array. We set the pixels of texture 'i'.
                shapeTextureArray.SetPixels(created.GetPixels(), tIndex);
                tIndex++;
            }
            shapeTextureArray.Apply();
            backdropSolidMaterial.SetTexture("_AvailableTextures", shapeTextureArray);
            backdropTransparentMaterial.SetTexture("_AvailableTextures", shapeTextureArray);
        }

        // <summary>
        /// Creates all the game objects for an entire shape.
        /// </summary>
        public void CreateAllShapeElementGameObjects(Shape shape, bool tesselateFirst = true)
        {
            allGameObjectsForBackdrop = new List<ShapeElementGameObject>();
            if (tesselateFirst)
            {
                ShapeTesselator.TesselateShape(shape, backdropLoadedTextures, maxTextureSize);
            }

            foreach (ShapeElement elem in shape.Elements)
            {
                CreateShapeElementGameObject(elem);
            }
        }

        /// <summary>
        /// Creates a gameobject for a specific element (and its children, optional).
        /// </summary>
        public GameObject CreateShapeElementGameObject(ShapeElement elem, bool createChildren = true)
        {
            GameObject ch = GameObject.Instantiate(shapeElementPrefab, backdropParent);
            ch.GetComponent<ShapeElementGameObject>().InitializeElement(elem);
            allGameObjectsForBackdrop.Add(ch.GetComponent<ShapeElementGameObject>());
            if (createChildren && elem.Children != null)
            {
                foreach (ShapeElement child in elem.Children)
                {
                    CreateShapeElementGameObject(child);
                }
            }
            return ch;
        }

        public void AssignMaterials(BackdropOrAttachmentData data)
        {
            Material toUse = data.opacity >= 0.999f ? backdropSolidMaterial : backdropTransparentMaterial;
            backdropTransparentMaterial.SetFloat("_OpacityMultiplier", data.opacity);
            toUse.SetFloat("_ShowTextures", data.hideTextures ? 0 : 1);
            toUse.SetColor("_FlatColorForNoTexture", BackdropManager.main.GetColorFromIndex(data.flatColorIndex));
            foreach (ShapeElementGameObject se in allGameObjectsForBackdrop)
            {
                se.gameObject.GetComponent<MeshRenderer>().sharedMaterial = toUse;
            }
        }

        public List<ShapeElementGameObject> GetGameObjects()
        {
            return allGameObjectsForBackdrop;
        }

        public void SearchForStepparents()
        {
            foreach (ShapeElement e in ShapeHolder.CurrentLoadedShape.Elements)
            {
                e.SearchForStepParentInShape(backdrop);
                if (e.StepParentElement != null)
                {
                    ShapeTesselator.ResolveMatricesForShapeElementAndChildren(e);
                    e.gameObject.ReapplyTransformsFromMeshData(true);
                }
            }
        }

        public void RemoveStepparents()
        {
            foreach (ShapeElement e in ShapeHolder.CurrentLoadedShape.Elements)
            {
                //Check that the step parent is actually from this backdrop.
                if (allGameObjectsForBackdrop.Any(x => { return x.element == e.StepParentElement; }))
                {
                    e.ClearStepParent();
                    ShapeTesselator.ResolveMatricesForShapeElementAndChildren(e);
                    e.gameObject.ReapplyTransformsFromMeshData(true);
                }
            }
        }

    }
}