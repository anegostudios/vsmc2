using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ElementHierachyItemPrefab : MonoBehaviour
{

    static bool AlternateColor = false;
    public GameObject emptySpace;
    public GameObject minMaxButton;
    public GameObject hideShowButton;
    public GameObject parentedObject;
    public TMP_Text elementName;

    public void InitializePrefab(ShapeElementJSON element, int parentCount)
    {
        Color c = GetComponent<Image>().color;
        GetComponent<Image>().color = new Color(c.r, c.g, c.b, AlternateColor ? 0.15f : 0.25f); 
        emptySpace.GetComponent<LayoutElement>().preferredWidth = parentCount * 16;
        parentedObject.SetActive(parentCount != 0);
        elementName.text = element.Name;
    }

}
