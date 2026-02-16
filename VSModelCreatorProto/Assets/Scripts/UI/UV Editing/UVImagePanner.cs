using UnityEngine;
using UnityEngine.EventSystems;

namespace VSMC
{
    public class UVImagePanner : MonoBehaviour
    {
        public Vector2 cVal;
        public RectTransform panCursor;
        public float centerSnapLimit;
        RectTransform rect;
        bool mouseDownOnElem;

        void Start()
        {
            rect = GetComponent<RectTransform>();
            cVal = new Vector2(0.5f, 0.5f);
            centerSnapLimit = centerSnapLimit * centerSnapLimit;
        }

        // Update is called once per frame
        void Update()
        {
            if (mouseDownOnElem)
            {
                if (!Input.GetMouseButton(0))
                {
                    mouseDownOnElem = false;
                    return;
                }

                Vector2 result;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, Input.mousePosition, null, out result);
                result /= rect.sizeDelta;

                //Add a bit of snapping to the center.
                if (result.sqrMagnitude < centerSnapLimit) 
                { 
                    result = Vector2.zero; 
                }

                result += new Vector2(0.5f, 0.5f);
                result = new Vector2(Mathf.Clamp01(result.x), Mathf.Clamp01(result.y));
                panCursor.anchorMin = result;
                panCursor.anchorMax = result;
                panCursor.anchoredPosition = Vector2.zero;
                cVal = result;
            }
        }
        public void MouseDownOnElement(BaseEventData pointer)
        {
            if ((pointer as PointerEventData).button != 0) return;
            mouseDownOnElem = true;
        }
    }

}