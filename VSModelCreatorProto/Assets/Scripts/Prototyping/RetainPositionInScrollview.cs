using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

/// <summary>
/// Mainly used for the animation timeline - This allows elements in a scroll view to keep their position on one or both axes, whilst still being a child.
/// </summary>
public class RetainPositionInScrollview : MonoBehaviour
{
    public RectTransform contentRect;
    public bool retainHorizontalPosition;
    public bool retainVerticalPosition;
    Vector2 initialLocalPosition;
    RectTransform myRect;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myRect = GetComponent<RectTransform>();
        initialLocalPosition = myRect.anchoredPosition;
        GetComponentInParent<ScrollRect>().onValueChanged.AddListener(ScrollPositionChanged);
    }

    void ScrollPositionChanged(Vector2 newPos)
    {
        myRect.anchoredPosition = initialLocalPosition - new Vector2(retainHorizontalPosition ? contentRect.anchoredPosition.x : 0, 
                                                                     retainVerticalPosition ? contentRect.anchoredPosition.y : 0);
    }

}
