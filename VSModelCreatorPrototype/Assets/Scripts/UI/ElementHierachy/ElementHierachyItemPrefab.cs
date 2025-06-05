using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VSMC;

public class ElementHierachyItemPrefab : MonoBehaviour
{

    static bool AlternateColor = false;
    int elementUID;
    Color defaultColor;
    public GameObject emptySpace;
    public GameObject minMaxButton;
    public GameObject hideShowButton;
    public GameObject parentedObject;
    public TMP_Text elementName;

    public void InitializePrefab(ShapeElement element, int parentCount)
    {
        elementUID = element.elementUID;
        Color c = GetComponent<Image>().color;
        GetComponent<Image>().color = new Color(c.r, c.g, c.b, AlternateColor ? 0.15f : 0.25f);
        defaultColor = GetComponent<Image>().color;
        AlternateColor = !AlternateColor;
        emptySpace.GetComponent<LayoutElement>().preferredWidth = parentCount * 16;
        parentedObject.SetActive(parentCount != 0);
        elementName.text = element.Name;

        //Register object selections
        ObjectSelector.main.RegisterForObjectSelectedEvent(OnElementSelected);
        ObjectSelector.main.RegisterForObjectDeselectedEvent(OnElementDeselected);

        //Trying to set the element name width is awful, so this manually sets it after a single frame.
        Invoke("ResolveTextSize", 0.1f);
    }

    void ResolveTextSize()
    {
        elementName.GetComponent<LayoutElement>().minWidth = elementName.textBounds.size.x;
    }

    void OnElementSelected(GameObject sel)
    {
        if (sel.GetComponent<ShapeElementGameObject>().element.elementUID == elementUID)
        {
            GetComponent<Image>().color = new Color(1, 0.75f, 0);
        }
    }

    void OnElementDeselected(GameObject desel)
    {
        if (desel.GetComponent<ShapeElementGameObject>().element.elementUID == elementUID)
        {
            GetComponent<Image>().color = defaultColor;
        }
    }

    public void OnElementNameClicked()
    {
        ObjectSelector.main.SelectFromUIElement(this);
    }

    public int GetUID()
    {
        return elementUID;
    }

}
