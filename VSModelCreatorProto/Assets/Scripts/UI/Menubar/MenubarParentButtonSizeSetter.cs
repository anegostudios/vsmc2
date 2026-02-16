using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Place this on the 'LeftElements' gameobject - The one with the HorizontalLayoutGroup.
/// </summary>
[RequireComponent(typeof(HorizontalLayoutGroup))]
public class MenubarParentButtonSizeSetter : MonoBehaviour
{
    public int paddingSize;
    public bool isOnRight;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //This needs to be run after the text components have been generated.
        Invoke("SetButtonSize", 0.1f);
        
    }

    void SetButtonSize()
    {
        foreach (Transform t in transform)
        {
            RectTransform cRect = t.GetComponent<RectTransform>();
            cRect.sizeDelta = new Vector2(Mathf.CeilToInt(t.GetComponentInChildren<TMP_Text>().textBounds.size.x) + (paddingSize * 2), cRect.sizeDelta.y);
        }
        if (isOnRight)
        {
            GetComponent<RectTransform>().localPosition -= new Vector3(GetComponentInParent<Canvas>().rootCanvas.GetComponent<RectTransform>().sizeDelta.x % 1, 0, 0);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }
}
