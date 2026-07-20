using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InnerScrollRect : ScrollRect
{

    public ScrollRect parentScrollRect;

    protected override void Start()
    {
        parentScrollRect = transform.parent.GetComponentInParent<ScrollRect>();
        if (parentScrollRect == null)
        {
            Debug.LogError("Could not find parent scroll rect for innerscrollrect " + gameObject.name);
        }
    }

    public override void OnScroll(PointerEventData data)
    {
        if (data.scrollDelta.y > Mathf.Epsilon && verticalScrollbar.value >= 1 ||
            data.scrollDelta.y < -Mathf.Epsilon && verticalScrollbar.value <= 0 ||
            !verticalScrollbar.gameObject.activeSelf)
        {
            parentScrollRect.OnScroll(data);
            return;
        }
        base.OnScroll(data);
    }

}
