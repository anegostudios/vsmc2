using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace VSMC
{
    /// <summary>
    /// The entity texture UV space...
    /// All faces need to exist on this single space.
    /// </summary>
    public class EntityTextureUVSpace : MonoBehaviour
    {

        public RectTransform zoomAndPanTransform;
        public RawImage mainTexture;
        public RawImage grid;
        public Transform uvEntriesParent;
        public Slider zoomSlider;
        public UVImagePanner panner;
        public Color[] faceColors;
        public TMP_Text textureNameText;
        public float mousewheelScrollSpeed = 0.1f;
        Vector2 uiPixelsPerUVPixel;
        RectTransform myRect;

        //Movement stuff.
        Vector2 lmbStartPos;
        bool lmbDown;
        Vector2 lmbStartEntityUvs;
        float[] lmbStartUVs;
        Vector2 rmbStartPos;
        bool rmbDown;
        Vector2 rmbStartPosForPanner;
        float[] rmbStartUVs;

        //Face & Element Selection
        EntityTextureUVEntry cSelectedUvEntry;
        EntityTextureUVEntry[] uvEntriesUnderMouseCursorOnClick;
        int lastSelectedUvEntryOnClick;
        Vector2 lmbPositionForScrollingThroughEntries;

        void Start()
        {
            myRect = GetComponent<RectTransform>();
            //EditModeManager.RegisterForOnModeSelect(OnEditModeSelect);
        }

        public void SetTexture(LoadedTexture tex)
        {
            mainTexture.texture = tex.loadedTexture;
            grid.uvRect = new Rect(0, 0, tex.storedWidth, tex.storedHeight);
            textureNameText.text = tex.code.ToUpper();
            //RefreshAllUISpacePositions();
        }

        private void Update()
        {
            OnZoomOrPositionChanged();

            if (lmbDown || rmbDown)
            {
                Vector2 cmpos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(myRect, Input.mousePosition, null, out cmpos);
                if (lmbDown && ObjectSelector.main.IsAnySelected() && cSelectedUvEntry != null && !cSelectedUvEntry.elem.autoUnwrap) //Auto-Unwrap Settings
                {
                    Vector2 diff = cmpos - lmbStartPos;
                    //Convert the actual difference into UV pixels, respective of the current zoom.
                    diff /= (uiPixelsPerUVPixel);

                    //clamp to 1.
                    if (!Input.GetKey(KeyCode.LeftControl))
                    {
                        diff = new Vector2((int)diff.x, (int)diff.y);
                    }

                    //UVs be upside down.
                    diff.y *= -1;

                    float[] nUVs = new float[] { lmbStartUVs[0] + diff.x, lmbStartUVs[1] + diff.y, lmbStartUVs[2] + diff.x, lmbStartUVs[3] + diff.y };
                    bool[] selFaces = new bool[6];
                    selFaces[cSelectedUvEntry.faceIndex] = true;
                    TaskSetAllUVs setUVTask = new TaskSetAllUVs(cSelectedUvEntry.elem, selFaces, lmbStartUVs, nUVs);
                    setUVTask.DoTask();

                    if (!Input.GetMouseButton(0))
                    {
                        if (!(diff.x == 0 && diff.y == 0))
                        {
                            UndoManager.main.CommitTask(setUVTask);
                        }
                        lmbDown = false;
                        return;
                    }
                }
                else if (lmbDown && ObjectSelector.main.IsAnySelected() && cSelectedUvEntry != null && cSelectedUvEntry.elem.autoUnwrap)
                {
                    //Auto unwrap functionality!
                    Vector2 diff = cmpos - lmbStartPos;
                    //Convert the actual difference into UV pixels, respective of the current zoom.
                    diff /= (uiPixelsPerUVPixel);

                    //clamp to 1.
                    if (!Input.GetKey(KeyCode.LeftControl))
                    {
                        diff = new Vector2((int)diff.x, (int)diff.y);
                    }

                    //UVs be upside down.
                    diff.y *= -1;

                    Vector2 newEntityUv = lmbStartEntityUvs + diff;
                    TaskSetEntityTextureUVPosition setEntityUVTask = new TaskSetEntityTextureUVPosition(cSelectedUvEntry.elem, newEntityUv, lmbStartEntityUvs);
                    setEntityUVTask.DoTask();

                    if (!Input.GetMouseButton(0))
                    {
                        if (!(diff.x == 0 && diff.y == 0))
                        {
                            UndoManager.main.CommitTask(setEntityUVTask);
                        }
                        lmbDown = false;
                        return;
                    }
                }
                if (lmbDown)
                {
                    if (!Input.GetMouseButton(0))
                    {
                        lmbDown = false;
                        return;
                    }
                }
                else if (rmbDown)
                {
                    Vector2 diff = rmbStartPos - (Vector2)Input.mousePosition;
                    panner.SetPan(rmbStartPosForPanner + (diff / myRect.rect.size / zoomSlider.value));
                    OnZoomOrPositionChanged();

                    if (!Input.GetMouseButton(1))
                    {
                        rmbDown = false;
                        return;
                    }

                    /*
                    Vector2 diff = cmpos - rmbStartPos;
                    //Convert the actual difference into UV pixels, respective of the current zoom.
                    diff /= (uiPixelsPerUVPixel);

                    //clamp to 1.
                    if (!Input.GetKey(KeyCode.LeftControl))
                    {
                        diff = new Vector2((int)diff.x, (int)diff.y);
                    }

                    //UVs be upside down.
                    diff.y *= -1;

                    float[] nUVs = new float[] { rmbStartUVs[0], rmbStartUVs[1], rmbStartUVs[2] + diff.x, rmbStartUVs[3] + diff.y };
                    bool[] selFaces = new bool[6];
                    selFaces[faceIndex] = true;
                    TaskSetAllUVs setUVTask = new TaskSetAllUVs(ObjectSelector.main.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element, selFaces, rmbStartUVs, nUVs);
                    setUVTask.DoTask();

                    if (!Input.GetMouseButton(1))
                    {
                        if (!(diff.x == 0 && diff.y == 0))
                        {
                            UndoManager.main.CommitTask(setUVTask);
                        }
                        rmbDown = false;
                        return;
                    }
                    */
                }
            }
        }

        public void OnZoomOrPositionChanged()
        {
            zoomAndPanTransform.localScale = Vector3.one * zoomSlider.value;
            Vector2 panVal = (panner.cVal - new Vector2(0.5f, 0.5f)) * zoomSlider.value;
            zoomAndPanTransform.anchoredPosition = myRect.rect.size * -panVal;

            //Calculate the mouse per pixel stuff.
            uiPixelsPerUVPixel = new Vector2((myRect.rect.width) / grid.uvRect.width, (myRect.rect.height) / grid.uvRect.height) * zoomSlider.value;

        }

        public void MouseDown(BaseEventData data)
        {
            //Mouse down should select the face and object of whatever is under the cursor and initialize the move for it. 
            // But only if the element is under the cursor.

            //This took an embarrassingly long time to fix and I cannot be bothered to go back and make it neater.
            PointerEventData ped = data as PointerEventData;
            if (ped.button == PointerEventData.InputButton.Left)
            {
                if (((Vector2)Input.mousePosition).IsNearlyEqual(lmbPositionForScrollingThroughEntries) &&
                uvEntriesUnderMouseCursorOnClick != null && uvEntriesUnderMouseCursorOnClick.Length > 0)
                {
                    //Carry on scrolling through list...
                    lastSelectedUvEntryOnClick++;
                    if (lastSelectedUvEntryOnClick >= uvEntriesUnderMouseCursorOnClick.Length)
                    {
                        lastSelectedUvEntryOnClick = 0;
                    }
                    SelectUVEntry(uvEntriesUnderMouseCursorOnClick[lastSelectedUvEntryOnClick]);
                }
                else
                {
                    lastSelectedUvEntryOnClick = 0;
                    List<EntityTextureUVEntry> uvEntries = new List<EntityTextureUVEntry>();
                    lmbPositionForScrollingThroughEntries = (Vector2)Input.mousePosition;
                    foreach (RectTransform t in uvEntriesParent)
                    {
                        if (RectTransformUtility.RectangleContainsScreenPoint(t, Input.mousePosition))
                        {
                            uvEntries.Add(t.GetComponent<EntityTextureUVEntry>());
                            Debug.Log("Mouse is over element " + t.GetComponent<EntityTextureUVEntry>().name);
                        }
                    }
                    uvEntriesUnderMouseCursorOnClick = new EntityTextureUVEntry[uvEntries.Count];
                    for (int i = 0; i < uvEntries.Count; i++)
                    {
                        uvEntriesUnderMouseCursorOnClick[i] = uvEntries[i];
                    }
                    if (uvEntries.Count > 0)
                    {
                        SelectUVEntry(uvEntriesUnderMouseCursorOnClick[0]);
                    }
                    else
                    {
                        ObjectSelector.main.DeselectAll();
                    }
                }

                RectTransformUtility.ScreenPointToLocalPointInRectangle(myRect, Input.mousePosition, null, out lmbStartPos);
                lmbDown = true;

                //lmbStartUVs = (float[])cUVs.Clone();
            }
            else if (ped.button == PointerEventData.InputButton.Right)
            {
                rmbDown = true;
                rmbStartPos = Input.mousePosition;
                rmbStartPosForPanner = panner.cVal;
                //RectTransformUtility.ScreenPointToLocalPointInRectangle(myRect, Input.mousePosition, null, out rmbStartPos);

            }
        }
        
        public void MouseWheelScrollOver(BaseEventData data)
        {
            ExtendedPointerEventData ped = data as ExtendedPointerEventData;
            zoomSlider.value += ped.scrollDelta.y * mousewheelScrollSpeed;
        }

        void SelectUVEntry(EntityTextureUVEntry entry)
        {
            entry.SelectThisElementAndFace();
            cSelectedUvEntry = entry;
            lmbStartUVs = (float[])entry.elem.FacesResolved[entry.faceIndex].Uv.Clone();
            lmbStartEntityUvs = new Vector2((float)entry.elem.entityTextureUV[0], (float)entry.elem.entityTextureUV[1]);
        }

        public Color GetColorForFace(int index)
        {
            return faceColors[index];
        }

    }
}