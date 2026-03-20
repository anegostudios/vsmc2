using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace VSMC
{
    /// <summary>
    /// Manages all the UV layout malarkey.
    /// </summary>
    public class UVLayoutManager : MonoBehaviour
    {
        public static UVLayoutManager main;
        public UVImageLayout imageLayout;
        public UVSpace[] uvSpaces;

        public Color deselectedFaceOutlineColor;
        public Color selectedFaceOutlineColor;

        [Header("Entity Texturing")]
        //public EntityTextureUVSpace entityUVSpace;
        public GameObject entityTextureUIElementsParent;
        public GameObject standardTextureUIElementsParent;

        //There should be one UV space per valid texture.
        public GameObject entityTextureUVSpacePrefab;
        public Transform entityTextureUVSpaceParent;
        public Dictionary<string, EntityTextureUVSpace> textureCodeToUVSpace;

        //Actual entity texturing UV elements
        public GameObject entityUVEntryPrefab;
        public Transform invalidEntityUvEntryParent;
        public Color[] faceColors;
        public Dictionary<int, EntityTextureUVEntry[]> shapeElementUIDToUVEntries;

        bool hasCreatedEntityTextureUVSpaces = false; 
        bool[] mostRecentSelFaces;

        private void Awake()
        {
            main = this;
        }

        void Start()
        {
            ObjectSelector.main.RegisterForObjectSelectedEvent(OnElementSelect);
            ObjectSelector.main.RegisterForObjectDeselectedEvent(OnElementDeselect);
        }

        public void RefreshAllUVSpaces(bool forceReset = false)
        {
            bool entityTex = TextureEditor.GetEntityTextureMode();
            entityTextureUIElementsParent.SetActive(entityTex);
            standardTextureUIElementsParent.SetActive(!entityTex);
            if (!entityTex)
            {
                entityTextureUVSpaceParent.gameObject.SetActive(false);
                if (!ObjectSelector.main.IsAnySelected())
                {
                    for (int i = 0; i < uvSpaces.Length; i++)
                    {
                        uvSpaces[i].gameObject.SetActive(false);
                    }
                    return;
                }
                ShapeElementGameObject cSel = ObjectSelector.main.GetCurrentlySelected().GetComponent<ShapeElementGameObject>();
                for (int i = 0; i < uvSpaces.Length; i++)
                {
                    uvSpaces[i].SetTexture(cSel.element.FacesResolved[i].GetLoadedTexture());
                    uvSpaces[i].SetUVPosition(cSel.element.FacesResolved[i].Uv);
                }
            }
            else if (forceReset || !hasCreatedEntityTextureUVSpaces)
            {
                for (int i = 0; i < uvSpaces.Length; i++)
                {
                    uvSpaces[i].gameObject.SetActive(false);
                }
                CreateEntityTextureUVSpaces();
                entityTextureUVSpaceParent.gameObject.SetActive(true);
            }
        }

        public void OnSelectedFacesChanged(bool[] selFaces)
        {
            mostRecentSelFaces = selFaces;
            if (TextureEditor.GetEntityTextureMode())
            {
                OnFaceSelectionChangedForEntityTexturing(selFaces);
                imageLayout.ReorganizeElements();
                return;
            }
            if (!ObjectSelector.main.IsAnySelected())
            {
                for (int i = 0; i < uvSpaces.Length; i++)
                {
                    uvSpaces[i].gameObject.SetActive(false);
                }
                imageLayout.ReorganizeElements();
                return;
            }
            ShapeElementGameObject cSel = ObjectSelector.main.GetCurrentlySelected().GetComponent<ShapeElementGameObject>();
            for (int i = 0; i < selFaces.Length && i < uvSpaces.Length; i++)
            {
                uvSpaces[i].gameObject.SetActive(selFaces[i] && cSel.element.FacesResolved[i].Enabled);
            }

            imageLayout.ReorganizeElements();
        }

        public void OnEntityTextureModeChange()
        {
            if (ShapeHolder.CurrentLoadedShape == null) return;
            if (EditModeManager.main.cEditMode != VSEditMode.Texture) return;
            RefreshAllUVSpaces(true);
        }

        public void CreateEntityTextureUVSpaces()
        {
            foreach (Transform t in entityTextureUVSpaceParent.transform)
            {
                Destroy(t.gameObject);
            }
            textureCodeToUVSpace = new Dictionary<string, EntityTextureUVSpace>();

            foreach (LoadedTexture tex in TextureManager.main.loadedTextures)
            {
                EntityTextureUVSpace etSpace = Instantiate(entityTextureUVSpacePrefab, entityTextureUVSpaceParent).GetComponent<EntityTextureUVSpace>();
                etSpace.SetTexture(tex);
                textureCodeToUVSpace.Add(tex.code.ToLower(), etSpace);
            }
            hasCreatedEntityTextureUVSpaces = true;
            CreateEntityTextureUVEntriesForAllElements();
        }

        public void CreateEntityTextureUVEntriesForAllElements()
        {
            //First clear the existing UV entries.
            shapeElementUIDToUVEntries = new Dictionary<int, EntityTextureUVEntry[]>();
            foreach (EntityTextureUVSpace uvSpace in textureCodeToUVSpace.Values)
            {
                foreach (Transform t in uvSpace.uvEntriesParent)
                {
                    Destroy(t.gameObject);
                }
            }
            foreach (Transform t in invalidEntityUvEntryParent)
            {
                Destroy(t.gameObject);
            }

            foreach (ShapeElement e in ShapeElementRegistry.main.GetAllShapeElements())
            {
                EntityTextureUVEntry[] faceUVs = new EntityTextureUVEntry[6];
                bool doneName = false;
                for (int i = 0; i < e.FacesResolved.Length; i++)
                {
                    string texCode = e.FacesResolved[i].ResolvedTexture;
                    if (texCode == null) texCode = "";
                    if (textureCodeToUVSpace.TryGetValue(texCode.ToLower(), out var space))
                    {
                        faceUVs[i] = Instantiate(entityUVEntryPrefab, space.uvEntriesParent).GetComponent<EntityTextureUVEntry>();
                        faceUVs[i].Initialize(space, e, i, !doneName);
                        doneName = doneName || faceUVs[i].elemName.gameObject.activeSelf;
                    }
                    else
                    {
                        //Invalid texture code... We still need a UV entry, but it needs to be located elsewhere and invisible.
                        //Debug.LogWarning("Could not map texture code of " + e.FacesResolved[i].ResolvedTexture + " to any UV space for element: " + e.Name);
                        faceUVs[i] = Instantiate(entityUVEntryPrefab, invalidEntityUvEntryParent).GetComponent<EntityTextureUVEntry>();
                        faceUVs[i].InitializeAsEmpty(e, i);
                    }
                }
                shapeElementUIDToUVEntries.Add(e.elementUID, faceUVs);
            }

            if (ObjectSelector.main.IsAnySelected())
            {
                EntityTextureUVEntry[] spaces = shapeElementUIDToUVEntries[ObjectSelector.main.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element.elementUID];
                for (int i = 0; i < 6; i++)
                {
                    spaces[i].OnSelect();
                    if (mostRecentSelFaces == null)
                    {
                        spaces[i].ResolveFaceSelected(mostRecentSelFaces[i]);
                    }
                }
            }
        }

        void OnElementSelect(GameObject sel)
        {
            //Only need to worry about this for the entity texture mode.
            //Standard UV is managed in individual UVSpace.
            if (EditModeManager.main.cEditMode != VSEditMode.Texture || !TextureEditor.GetEntityTextureMode()) return;
            foreach (EntityTextureUVEntry entry in shapeElementUIDToUVEntries[sel.GetComponent<ShapeElementGameObject>().element.elementUID])
            {
                entry.OnSelect();
            }
        }

        void OnElementDeselect(GameObject desel)
        {
            //Only need to worry about this for the entity texture mode.
            //Standard UV is managed in individual UVSpace.
            if (EditModeManager.main.cEditMode != VSEditMode.Texture || !TextureEditor.GetEntityTextureMode()) return;
            foreach (EntityTextureUVEntry entry in shapeElementUIDToUVEntries[desel.GetComponent<ShapeElementGameObject>().element.elementUID])
            {
                entry.OnDeselect();
            }
        }

        /*
                public void RefreshAllUISpacePositions()
                {
                    if (shapeElementToUVsByFace == null) CreateEntityTextureUVElements();
                    foreach (EntityTextureUVEntry[] entries in shapeElementToUVsByFace.Values)
                    {
                        foreach (EntityTextureUVEntry i in entries)
                        {
                            i.SetUVPosition();
                        }
                    }
                }
        */

        void OnFaceSelectionChangedForEntityTexturing(bool[] selFaces)
        {
            if (EditModeManager.main.cEditMode != VSEditMode.Texture || !TextureEditor.GetEntityTextureMode() || shapeElementUIDToUVEntries == null) return;
            if (ObjectSelector.main.IsAnySelected())
            {
                EntityTextureUVEntry[] spaces = shapeElementUIDToUVEntries[ObjectSelector.main.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element.elementUID];
                for (int i = 0; i < 6; i++)
                {
                    spaces[i].ResolveFaceSelected(selFaces[i]);
                }
            }
        }

        public void RecalculateUVPositionsForSingleElement(ShapeElement elem)
        {
            if (TextureEditor.GetEntityTextureMode())
            {
                EntityTextureUVEntry[] faceUVs = shapeElementUIDToUVEntries[elem.elementUID];
                bool doneName = false;
                for (int i = 0; i < elem.FacesResolved.Length; i++)
                {
                    string texCode = elem.FacesResolved[i].ResolvedTexture;
                    if (texCode == null) texCode = "";
                    if (textureCodeToUVSpace.TryGetValue(texCode.ToLower(), out var space))
                    {
                        faceUVs[i].transform.SetParent(space.uvEntriesParent);
                        faceUVs[i].UpdateElementSpace(space, !doneName);
                        doneName = doneName || faceUVs[i].elemName.gameObject.activeSelf;
                    }
                    else
                    {
                        //Invalid texture code... We still need a UV entry, but it needs to be located elsewhere and invisible.
                        //Debug.LogWarning("Could not map texture code of " + e.FacesResolved[i].ResolvedTexture + " to any UV space for element: " + e.Name);
                        faceUVs[i].transform.SetParent(invalidEntityUvEntryParent);
                        faceUVs[i].UpdateElementSpace(null, false);
                    }
                }
            }
            else
            {
                RefreshAllUVSpaces();
            }
        }
    }
}