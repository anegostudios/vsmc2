using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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
    public Image dragAndDropImageInside;
    public Image dragAndDropImageAbove;
    public Image dragAndDropImageBelow;
    public Image dragAndDropSelection;
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
        dragAndDropImageAbove.GetComponent<RectTransform>().offsetMin = new Vector2(parentCount * 16, -1);
        dragAndDropImageBelow.GetComponent<RectTransform>().offsetMin = new Vector2(parentCount * 16, 0);
        parentedObject.SetActive(parentCount != 0);
        elementName.text = element.Name;

        //This is important for the reparenting menu. We cannot bulk copy over IDs, so by setting the name it'll definitely be copied.
        gameObject.name = element.elementUID.ToString();

        //Set element buttons
        hideShowButton.sprite = element.renderInEditor ? hierarchyManager.ShownSprite : hierarchyManager.HiddenSprite;
        minMaxButton.sprite = element.minimizeFromThisObject ? hierarchyManager.ExpandChildrenSprite : hierarchyManager.CollapseChildrenSprite;
        if (element.Children == null || element.Children.Length == 0)
        {
            minMaxButton.enabled = false;
        }

        //Register object selections
        ObjectSelector.main.RegisterForObjectSelectedEvent(OnElementSelected);
        ObjectSelector.main.RegisterForObjectDeselectedEvent(OnElementDeselected);

        if (ObjectSelector.main.IsAnySelected()) OnElementSelected(ObjectSelector.main.GetCurrentlySelected());

        //Trying to set the element name width using the editor is awful, so this manually sets it after a single frame.
        Invoke("ResolveTextSize", 0.1f);
    }
    
    public void MatchesSearch(string s)
    {
        if (elementName.text.Contains(s, System.StringComparison.CurrentCultureIgnoreCase))
        {
            gameObject.SetActive(true);
            parentedObject.SetActive(false);
            emptySpace.SetActive(false);
            minMaxButton.gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
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
        elem.renderInEditor = !elem.renderInEditor;
        hideShowButton.sprite = elem.renderInEditor ? hierarchyManager.ShownSprite : hierarchyManager.HiddenSprite;
        ShapeElementRegistry.main.GetShapeElementByUID(elementUID).RecalculateHiddenStatus();
        if (EditModeManager.main.cEditMode == VSEditMode.Texture)
        {
            UVLayoutManager.main.RefreshAllUVSpaces();
        }
    }

    public void OnDragStart(BaseEventData data)
    {
        if ((data as PointerEventData).button != PointerEventData.InputButton.Left) return;
        hierarchyManager.BeginDragOfItem(this);
    }

    public void OnDragEnd(BaseEventData data)
    {
        if ((data as PointerEventData).button != PointerEventData.InputButton.Left) return;
        hierarchyManager.EndDragOfItem(true);
    }

    public void OnScroll(BaseEventData data)
    {
        hierarchyManager.hierarchyScroll.OnScroll((PointerEventData)data);
    }

    public int GetUID()
    {
        return elementUID;
    }

}
