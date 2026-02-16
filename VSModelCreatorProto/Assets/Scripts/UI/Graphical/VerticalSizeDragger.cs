using Riten.Native.Cursors;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class VerticalSizeDragger : MonoBehaviour
{
    public CanvasScaler mainCanvasScaler;
    public bool fromTop;
    public LayoutElement parentLayoutElement;
    public float screenHeightMaxMultiplier = 0.7f;
    public UIBehaviour[] elementsToDisableWhilstMoving;
    float dragStartMousePos;
    float dragstartPrefPos;
    bool isCurrentlyDrag;

    private void Update()
    {
        if (isCurrentlyDrag)
        {
            if (!Input.GetMouseButton(0))
            {
                isCurrentlyDrag = false;
                foreach (UIBehaviour c in elementsToDisableWhilstMoving)
                {
                    c.enabled = true;
                }
                return;
            }

            float diff = (Input.mousePosition.y - dragStartMousePos) / mainCanvasScaler.scaleFactor;
            if (fromTop) diff *= -1;
            parentLayoutElement.preferredHeight = Mathf.Min(dragstartPrefPos + diff, (Screen.height * screenHeightMaxMultiplier) / mainCanvasScaler.scaleFactor);
        }
    }

    public void OnBeginDrag()
    {
        if (isCurrentlyDrag) return;
        isCurrentlyDrag = true;
        dragStartMousePos = Input.mousePosition.y;
        dragstartPrefPos = parentLayoutElement.GetComponent<RectTransform>().rect.height;
        foreach (UIBehaviour c in elementsToDisableWhilstMoving)
        {
            c.enabled = false;
        }
    }

    public void OnCursorEnter()
    {
        NativeCursor.SetCursor(NTCursors.ResizeVertical);
    }

    public void OnCursorLeave()
    {
        NativeCursor.ResetCursor();
    }
}
