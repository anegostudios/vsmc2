using UnityEngine;
using UnityEngine.UI;

public class MatchHeightForRectTransforms : MonoBehaviour
{

    RectTransform myRect;
    public RectTransform setRectHeightOf;
    public float sizeOffset;

    private void Start()
    {
        myRect = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        setRectHeightOf.sizeDelta = new Vector2(setRectHeightOf.sizeDelta.x, myRect.sizeDelta.y + sizeOffset);
    }
}
