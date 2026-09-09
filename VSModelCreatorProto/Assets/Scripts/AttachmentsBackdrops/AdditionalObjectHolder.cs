using System.Collections.Generic;
using UnityEngine;

namespace VSMC
{
    public class AdditionalObjectHolder : IShapeHolder
    {

        public Material solidMaterial;
        public Material transparentMaterial;

        //Textures
        List<LoadedTexture> loadedTextures;
        int maxTextureSize;

        Shape attachmentShape;
        List<ShapeElementGameObject> allGameObjectsForAdditionalObject;

        void Start()
        {
            EditModeManager.RegisterForOnModeSelect(OnEditModeChange);
        }

        public void OnAttachmentShapeLoaded(Shape loadedShape, AdditionalShapeData data)
        {
            attachmentShape = loadedShape;
            gameObject.name = data.shapeFilepath;
            gameObject.SetActive(data.enabled);
            solidMaterial = new Material(solidMaterial);
            transparentMaterial = new Material(transparentMaterial);
            solidMaterial.SetFloat("_TexturesEnabled", 1);
            transparentMaterial.SetFloat("_TexturesEnabled", 1);
            loadedShape.ResolveReferencesAndUIDs(true);
            CreateTextures(loadedShape);
            CreateAllShapeElementGameObjects(loadedShape);
            AssignMaterials(data);
        }

        void CreateTextures(Shape shape)
        {
            //Copied from texture manager. 
            int i = 0;
            loadedTextures = new List<LoadedTexture>();
            foreach (var pair in shape.Textures)
            {
                loadedTextures.Add(new LoadedTexture(pair.Key, pair.Value));
                loadedTextures[i].LoadTextureFromCodeAndPath(shape);
                loadedTextures[i].ResolveTextureSize(shape);
                i++;
            }
            CreateTextureArrayForAttachment(shape);

            // We resolve all the textures and whatnot for each element.
            foreach (ShapeElement elem in shape.Elements)
            {
                elem.ResolveFacesAndTextures(loadedTextures);
            }
        }

        void CreateTextureArrayForAttachment(Shape shape)
        {
            //get max tex size.
            maxTextureSize = 16;
            Texture2DArray shapeTextureArray;
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
                solidMaterial.SetTexture("_AvailableTextures", shapeTextureArray);
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
            solidMaterial.SetTexture("_AvailableTextures", shapeTextureArray);
            transparentMaterial.SetTexture("_AvailableTextures", shapeTextureArray);
        }

        public void CreateAllShapeElementGameObjects(Shape shape, bool tesselateFirst = true)
        {
            allGameObjectsForAdditionalObject = new List<ShapeElementGameObject>();
            if (tesselateFirst)
            {
                ShapeTesselator.TesselateShape(shape, loadedTextures, maxTextureSize);
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
            GameObject ch = GameObject.Instantiate(shapeElementPrefab, noJointParent);
            ch.GetComponent<ShapeElementGameObject>().InitializeElement(elem);
            allGameObjectsForAdditionalObject.Add(ch.GetComponent<ShapeElementGameObject>());
            if (createChildren && elem.Children != null)
            {
                foreach (ShapeElement child in elem.Children)
                {
                    CreateShapeElementGameObject(child);
                }
            }
            return ch;
        }


        public void AssignMaterials(AdditionalShapeData data)
        {
            Material toUse = data.opacity >= 0.999f ? solidMaterial : transparentMaterial;
            transparentMaterial.SetFloat("_OpacityMultiplier", data.opacity);
            toUse.SetFloat("_ShowTextures", data.hideTextures ? 0 : 1);
            toUse.SetColor("_FlatColorForNoTexture", BackdropManager.main.GetColorFromIndex(data.flatColorIndex));
            foreach (ShapeElementGameObject se in allGameObjectsForAdditionalObject)
            {
                se.gameObject.GetComponent<MeshRenderer>().sharedMaterial = toUse;
            }
        }

        public List<ShapeElementGameObject> GetGameObjects()
        {
            return allGameObjectsForAdditionalObject;
        }

        void OnEditModeChange(VSEditMode mode)
        {
            if (mode != VSEditMode.Animation)
            {
                ReparentGameObjectsToNoJoints();
            }
        }


        public override void ReparentGameObjectsByJoints()
        {
            ShapeTesselator.ResolveAllMatricesForShape(GetShape(), true);
            foreach (ShapeElement e in attachmentShape.Elements)
            {
                e.gameObject.ReapplyTransformsFromMeshData(true);
            }
            List<ShapeElement> elements = new List<ShapeElement>();
            elements.AddRange(attachmentShape.Elements);
            while (elements.Count > 0)
            {
                ShapeElement cElem = elements[0];
                elements.RemoveAt(0);
                if (cElem.Children != null) elements.AddRange(cElem.Children);
                elements.AddRange(cElem.CurrentStepChildren);

                //Create joint IDs if needed.
                while (jointParents.Count <= cElem.JointId + 1)
                {
                    GameObject newJoint = new GameObject("Joint " + jointParents.Count);
                    newJoint.transform.parent = jointParentParent;
                    jointParents.Add(newJoint.transform);
                }
                cElem.gameObject.transform.SetParent(jointParents[cElem.JointId], false);
            }
        }

        public override void ReparentGameObjectsToNoJoints()
        {
            ShapeTesselator.ResolveAllMatricesForShape(GetShape());
            foreach (ShapeElement e in attachmentShape.Elements)
            {
                e.gameObject.ReapplyTransformsFromMeshData(true);
            }
            List<ShapeElement> elements = new List<ShapeElement>();
            elements.AddRange(attachmentShape.Elements);
            while (elements.Count > 0)
            {
                ShapeElement cElem = elements[0];
                elements.RemoveAt(0);
                if (cElem.Children != null) elements.AddRange(cElem.Children);

                cElem.gameObject.transform.SetParent(noJointParent, false);
            }
        }

        public override Shape GetShape()
        {
            return attachmentShape;
        }
    }
}