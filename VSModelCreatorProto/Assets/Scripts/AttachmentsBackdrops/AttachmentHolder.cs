using System.Collections.Generic;
using UnityEngine;

namespace VSMC
{
    public class AttachmentHolder : MonoBehaviour
    {

        [Header("Unity References")]
        public GameObject shapeElementPrefab;
        public Transform attachmentParent;

        public Material attachmentSolidMaterial;
        public Material attachmentTransparentMaterial;

        //Textures
        List<LoadedTexture> attachmentLoadedTextures;
        int maxTextureSize;

        Shape attachmentShape;
        List<ShapeElementGameObject> allGameObjectsForAttachment;

        void Start()
        {
            EditModeManager.RegisterForOnModeSelect(OnEditModeChange);
        }

        public void OnAttachmentShapeLoaded(Shape loadedShape, BackdropOrAttachmentData data)
        {
            attachmentShape = loadedShape;
            gameObject.name = data.shapeFilepath;
            gameObject.SetActive(data.enabled);
            attachmentSolidMaterial = new Material(attachmentSolidMaterial);
            attachmentTransparentMaterial = new Material(attachmentTransparentMaterial);
            attachmentSolidMaterial.SetFloat("_TexturesEnabled", 1);
            attachmentTransparentMaterial.SetFloat("_TexturesEnabled", 1);
            loadedShape.ResolveReferencesAndUIDs(true);
            CreateTextures(loadedShape);
            CreateAllShapeElementGameObjects(loadedShape);
            AssignMaterials(data);
        }

        void CreateTextures(Shape shape)
        {
            //Copied from texture manager. 
            int i = 0;
            attachmentLoadedTextures = new List<LoadedTexture>();
            foreach (var pair in shape.Textures)
            {
                attachmentLoadedTextures.Add(new LoadedTexture(pair.Key, pair.Value));
                attachmentLoadedTextures[i].LoadTextureFromCodeAndPath(shape);
                attachmentLoadedTextures[i].ResolveTextureSize(shape);
                i++;
            }
            CreateTextureArrayForAttachment(shape);

            // We resolve all the textures and whatnot for each element.
            foreach (ShapeElement elem in shape.Elements)
            {
                elem.ResolveFacesAndTextures(attachmentLoadedTextures);
            }
        }

        void CreateTextureArrayForAttachment(Shape shape)
        {
            //get max tex size.
            maxTextureSize = 16;
            Texture2DArray shapeTextureArray;
            foreach (var tex in attachmentLoadedTextures)
            {
                if (tex.loadedTexture != null) maxTextureSize = Mathf.Max(maxTextureSize, tex.loadedTexture.width, tex.loadedTexture.height);
            }

            //If we don't have any loaded textures, create an empty white texture.
            if (attachmentLoadedTextures == null || attachmentLoadedTextures.Count == 0)
            {
                Color[] colors = new Color[maxTextureSize * maxTextureSize];
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = Color.white;
                }
                shapeTextureArray = new Texture2DArray(maxTextureSize, maxTextureSize, 1, TextureFormat.ARGB32, false);
                shapeTextureArray.SetPixels(colors, 0);
                shapeTextureArray.Apply();
                attachmentSolidMaterial.SetTexture("_AvailableTextures", shapeTextureArray);
                attachmentTransparentMaterial.SetTexture("_AvailableTextures", shapeTextureArray);
                return;
            }

            //Each texture needs adding to the texture array.
            int tIndex = 0;
            shapeTextureArray = new Texture2DArray(maxTextureSize, maxTextureSize, attachmentLoadedTextures.Count, TextureFormat.ARGB32, false);
            shapeTextureArray.filterMode = FilterMode.Point;
            shapeTextureArray.wrapMode = TextureWrapMode.Clamp;
            foreach (var tex in attachmentLoadedTextures)
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
            attachmentSolidMaterial.SetTexture("_AvailableTextures", shapeTextureArray);
            attachmentTransparentMaterial.SetTexture("_AvailableTextures", shapeTextureArray);
        }

        public void CreateAllShapeElementGameObjects(Shape shape, bool tesselateFirst = true)
        {
            allGameObjectsForAttachment = new List<ShapeElementGameObject>();
            if (tesselateFirst)
            {
                ShapeTesselator.TesselateShape(shape, attachmentLoadedTextures, maxTextureSize);
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
            GameObject ch = GameObject.Instantiate(shapeElementPrefab, attachmentParent);
            ch.GetComponent<ShapeElementGameObject>().InitializeElement(elem);
            allGameObjectsForAttachment.Add(ch.GetComponent<ShapeElementGameObject>());
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
            Material toUse = data.opacity >= 0.999f ? attachmentSolidMaterial : attachmentTransparentMaterial;
            attachmentTransparentMaterial.SetFloat("_OpacityMultiplier", data.opacity);
            toUse.SetFloat("_ShowTextures", data.hideTextures ? 0 : 1);
            toUse.SetColor("_FlatColorForNoTexture", BackdropManager.main.GetColorFromIndex(data.flatColorIndex));
            foreach (ShapeElementGameObject se in allGameObjectsForAttachment)
            {
                se.gameObject.GetComponent<MeshRenderer>().sharedMaterial = toUse;
            }
        }

        public List<ShapeElementGameObject> GetGameObjects()
        {
            return allGameObjectsForAttachment;
        }

        public void SearchForStepparents()
        {
            foreach (ShapeElement e in attachmentShape.Elements)
            {
                e.SearchForStepParentInShape(ShapeHolder.CurrentLoadedShape);
                if (e.StepParentElement != null)
                {
                    ShapeTesselator.ResolveMatricesForShapeElementAndChildren(e);
                    e.gameObject.ReapplyTransformsFromMeshData(true);
                }
            }
        }

        public void RemoveStepparents()
        {
            foreach (ShapeElement e in attachmentShape.Elements)
            {
                if (e.StepParentElement != null)
                {
                    e.ClearStepParent();
                    ShapeTesselator.ResolveMatricesForShapeElementAndChildren(e);
                    e.gameObject.ReapplyTransformsFromMeshData(true);
                }
            }
        }

        void OnEditModeChange(VSEditMode mode)
        {
            if (mode == VSEditMode.Animation)
            {
                Invoke("ResetJoints", 0.1f);
            }
            else
            {
                SetJointsRecursive(attachmentShape.Elements, attachmentParent);
            }
        }

        void ResetJoints()
        {
            foreach (ShapeElement s in attachmentShape.Elements)
            {
                if (s.StepParentElement != null)
                {
                    Transform parentToUse = s.StepParentElement.gameObject.transform.parent; //Use the same joint parent as the parent element.
                    s.gameObject.transform.SetParent(parentToUse);
                    if (s.Children != null)
                    {
                        SetJointsRecursive(s.Children, parentToUse);
                    }
                }
            }
        }

        void SetJointsRecursive(ShapeElement[] elements, Transform parent)
        {
            foreach (ShapeElement e in elements)
            {
                e.gameObject.transform.SetParent(parent);
                if (e.Children != null)
                {
                    SetJointsRecursive(e.Children, parent);
                }
            }
        }

    }
}