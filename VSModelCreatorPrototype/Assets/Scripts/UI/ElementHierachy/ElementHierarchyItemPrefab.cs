using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VSMC;

public class ElementHierarchyItemPrefab : MonoBehaviour
{

    static bool AlternateColor = false;
    int elementUID;
    Color defaultColor;
    public GameObject emptySpace;
    public Image minMaxButton;
    public Image hideShowButton;
    public GameObject parentedObject;
    public TMP_Text elementName;
    ElementHierarchyManager hierarchyManager;

    public void InitializePrefab(ShapeElement element, int parentCount, ElementHierarchyManager hierarchyManager)
    {
        this.hierarchyManager = hierarchyManager;
        elementUID = element.elementUID;
        Color c = GetComponent<Image>().color;
        GetComponent<Image>().color = new Color(c.r, c.g, c.b, AlternateColor ? 0.15f : 0.25f);
        defaultColor = GetComponent<Image>().color;
        AlternateColor = !AlternateColor;
        emptySpace.GetComponent<LayoutElement>().preferredWidth = parentCount * 16;
        parentedObject.SetActive(parentCount != 0);
        elementName.text = element.Name;

        //Set element buttons
        hideShowButton.sprite = element.hiddenFromView ? hierarchyManager.HiddenSprite : hierarchyManager.ShownSprite;
        minMaxButton.sprite = element.minimizeFromThisObject ? hierarchyManager.ExpandChildrenSprite : hierarchyManager.CollapseChildrenSprite;
        if (element.Children == null || element.Children.Length == 0)
        {
            minMaxButton.enabled = false;
        }

        //Register object selections
        ObjectSelector.main.RegisterForObjectSelectedEvent(OnElementSelected);
        ObjectSelector.main.RegisterForObjectDeselectedEvent(OnElementDeselected);

        //Trying to set the element name width using the editor is awful, so this manually sets it after a single frame.
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

    public void OnCollapseOrExpandClicked()
    {
        ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elementUID);
        elem.minimizeFromThisObject = !elem.minimizeFromThisObject;
        minMaxButton.sprite = elem.minimizeFromThisObject ? hierarchyManager.ExpandChildrenSprite : hierarchyManager.CollapseChildrenSprite;
        hierarchyManager.DetermineIfElementIsMinimized(elem);
    }

    public void OnShowOrHideClicked()
    {
        ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elementUID);
        elem.hiddenFromView = !elem.hiddenFromView;
        hideShowButton.sprite = elem.hiddenFromView ? hierarchyManager.HiddenSprite : hierarchyManager.ShownSprite;
        ShapeElementRegistry.main.GetShapeElementByUID(elementUID).RecalculateHiddenStatus();
    }

    public int GetUID()
    {
        return elementUID;
    }

}
