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
        foreach (TMP_Text t in GetComponentsInChildren<TMP_Text>(true))
        {
            t.richText = true;
            int p = Mathf.CeilToInt(t.fontSize * 0.6f * 2) / 2;
            t.text = "<mspace="+p+"px>" + t.text + "</mspace>";
        }
    }

    void SetButtonSize()
    {
        LayoutElement pLayout = GetComponentInParent<LayoutElement>();
        foreach (Transform t in transform)
        {
            RectTransform cRect = t.GetComponent<RectTransform>();
            cRect.sizeDelta = new Vector2(Mathf.CeilToInt(t.GetComponentInChildren<TMP_Text>().textBounds.size.x) + (paddingSize * 2), cRect.sizeDelta.y);
        }
        //Set the children's heights to the same as the menubar itself.
        foreach (LayoutElement e in GetComponentsInChildren<LayoutElement>(true))
        {
            e.preferredHeight = pLayout.preferredHeight;
            e.minHeight = pLayout.minHeight;
        }
        if (isOnRight)
        {
            GetComponent<RectTransform>().localPosition -= new Vector3(GetComponentInParent<Canvas>().rootCanvas.GetComponent<RectTransform>().sizeDelta.x % 1, 0, 0);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }
}
