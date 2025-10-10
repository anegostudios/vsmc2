using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VSMC
{
    /// <summary>
    /// This is used to handle and navigate each UV space per face.
    /// </summary>
    public class UVSpace : MonoBehaviour
    {
        public int faceIndex;
        public RectTransform zoomAndPanTransform;
        public RawImage mainTexture;
        public RawImage grid;
        public RectTransform uvShape;
        public Slider zoomSlider;
        public UVImagePanner panner;
        Vector2 uiPixelsPerUVPixel;
        RectTransform myRect;
        float[] cUVs;

        //Movement stuff.
        Vector2 lmbStartPos;
        bool lmbDown;
        float[] lmbStartUVs;
        Vector2 rmbStartPos;
        bool rmbDown;
        float[] rmbStartUVs;


        private void Start()
        {
            myRect = GetComponent<RectTransform>();
        }

        public void SetTexture(LoadedTexture tex)
        {
            mainTexture.texture = tex.loadedTexture;
            grid.uvRect = new Rect(0, 0, tex.storedWidth, tex.storedHeight);
        }

        private void Update()
        {
            OnZoomOrPositionChanged();
            if (ObjectSelector.main.GetCurrentlySelected() == null) return;
            if (lmbDown || rmbDown)
            {
                Vector2 cmpos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(myRect, Input.mousePosition, null, out cmpos);
                if (lmbDown)
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
                    selFaces[faceIndex] = true;
                    TaskSetAllUVs setUVTask = new TaskSetAllUVs(ObjectSelector.main.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element, selFaces, lmbStartUVs, nUVs);
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
                else if (rmbDown)
                {
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
                }
            }


        }

        public void SetUVPosition(float[] uvs)
        {
            cUVs = (float[])uvs.Clone();
            float[] perPixelUVs = new float[]
            {
                uvs[0] / grid.uvRect.width,
                uvs[1] / grid.uvRect.height,
                uvs[2] / grid.uvRect.width,
                uvs[3] / grid.uvRect.height
            };

            uvShape.anchorMin = new Vector2(Mathf.Min(perPixelUVs[0], perPixelUVs[2]), Mathf.Min(1-perPixelUVs[3], 1 - perPixelUVs[1]));
            uvShape.anchorMax = new Vector2(Mathf.Max(perPixelUVs[0], perPixelUVs[2]), Mathf.Max(1-perPixelUVs[3], 1 - perPixelUVs[1]));
            uvShape.anchoredPosition = Vector2.zero;
            Vector2 sizeDelta = Vector2.zero;
            if (uvShape.anchorMin.x == uvShape.anchorMax.x)
            {
                sizeDelta.x = 4;
            }
            if (uvShape.anchorMin.y == uvShape.anchorMax.y)
            {
                sizeDelta.y = 4;
            }
            uvShape.sizeDelta = sizeDelta;
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
            PointerEventData ped = data as PointerEventData;
            if (ped.button == PointerEventData.InputButton.Left)
            {
                lmbDown = true;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(myRect, Input.mousePosition, null, out lmbStartPos);
                lmbStartUVs = (float[])cUVs.Clone();

            }
            else if (ped.button == PointerEventData.InputButton.Right)
            {
                rmbDown = true;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(myRect, Input.mousePosition, null, out rmbStartPos);
                rmbStartUVs = (float[])cUVs.Clone();
            }
        }


    }
}