using UnityEngine;
using UnityEngine.UI;

public class ScrollbarConfig : UIConfig
{

    public bool useSmallSize;

    public Scrollbar scrollbar;
    public RectTransform scrollbarRect;
    public RectTransform slidingArea;
    public RectTransform incUpSpace;
    public RectTransform incDownSpace;

    public override void RefreshUIFromConfig(UIConfigManager config)
    {
        int size = useSmallSize ? config.smallScrollbarSizePx : config.scrollBarSizePx;
        if (scrollbar.direction == Scrollbar.Direction.LeftToRight || scrollbar.direction == Scrollbar.Direction.RightToLeft)
        {
            //If horizontal...
            scrollbarRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
            slidingArea.offsetMax = new Vector2(-size, 0);
            slidingArea.offsetMin = new Vector2(size, 0);
            incUpSpace.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
            incDownSpace.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
            return;
        }
        //if vertical...
        scrollbarRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
        slidingArea.offsetMax = new Vector2(0, -size);
        slidingArea.offsetMin = new Vector2(0, size);
        incUpSpace.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
        incDownSpace.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
    }

}
