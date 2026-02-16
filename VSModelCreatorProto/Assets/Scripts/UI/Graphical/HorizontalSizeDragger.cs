using Riten.Native.Cursors;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class HorizontalSizeDragger : MonoBehaviour
{

    public CanvasScaler mainCanvasScaler;
    public bool fromRightHandSide;
    public LayoutElement parentLayoutElement;
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
                //Re-enable performance-intensive components.
                foreach (UIBehaviour c in elementsToDisableWhilstMoving)
                {
                    c.enabled = true;
                }
                return;
            }

            float diff = (Input.mousePosition.x - dragStartMousePos) / mainCanvasScaler.scaleFactor;
            if (fromRightHandSide) diff *= -1;
            parentLayoutElement.preferredWidth = Mathf.Min(dragstartPrefPos + diff, (Screen.width) / mainCanvasScaler.scaleFactor);

        }
    }

    public void OnBeginDrag()
    {
        if (isCurrentlyDrag) return;
        isCurrentlyDrag = true;
        //Disable performance-intensive components.
        dragStartMousePos = Input.mousePosition.x;
        dragstartPrefPos = parentLayoutElement.GetComponent<RectTransform>().rect.width;
        foreach (UIBehaviour c in elementsToDisableWhilstMoving)
        {
            c.enabled = false;
        }
    }

    public void OnCursorEnter()
    {
        NativeCursor.SetCursor(NTCursors.ResizeHorizontal);
    }

    public void OnCursorLeave()
    {
        NativeCursor.ResetCursor();
    }

}
