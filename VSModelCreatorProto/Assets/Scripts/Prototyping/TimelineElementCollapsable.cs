using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VSMC;

/// <summary>
/// This is used to control when a timeline element is collapsed or expanded, and to hide or show the appropriate keyframes too.
/// </summary>
public class TimelineElementCollapsable : MonoBehaviour
{

    public LayoutElement layoutForThisElement;
    public TMP_Text nameText;
    public Image elemNameImage;
    public Image collapseButtonImage;

    public Color unselColor;
    public Color selColor;

    public Sprite collapseIcon;
    public Sprite expandIcon;

    public GameObject[] elemsToExpandAndCollapse;
    public float expandedHeight;
    public float collapsedHeight;

    TimelineManager manager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    public void Initialize(TimelineManager manager, string elemName, params GameObject[] linkedCollapses)
    {
        this.manager = manager;
        nameText.text = elemName;
        elemsToExpandAndCollapse = elemsToExpandAndCollapse.Concat(linkedCollapses).ToArray();
        ObjectSelector.main.RegisterForObjectSelectedEvent(OnAnyElementSelected);
        ObjectSelector.main.RegisterForObjectDeselectedEvent(OnAnyElementDeselected);
        elemNameImage.color = unselColor;
        OnElementCollapsed();
    }

    public void OnCollapseButtonClick()
    {
        if (collapseButtonImage.sprite == collapseIcon)
        {
            OnElementCollapsed();
        }
        else
        {
            OnElementExpanded();
        }
    }

    public void OnElementEntryClicked()
    {
        OnSelectedThisElement();
    }

    public void OnElementExpanded()
    {
        layoutForThisElement.minHeight = expandedHeight;
        foreach (GameObject g in elemsToExpandAndCollapse)
        {
            g.SetActive(true);
        }
        collapseButtonImage.sprite = collapseIcon;
    }

    public void OnElementCollapsed()
    {
        layoutForThisElement.minHeight = collapsedHeight;
        foreach (GameObject g in elemsToExpandAndCollapse)
        {
            g.SetActive(false);
        }
        collapseButtonImage.sprite = expandIcon;
    }

    public void OnSelectedThisElement()
    {
        ObjectSelector.main.SelectObject(ShapeElementRegistry.main.GetShapeElementByName(nameText.text).gameObject.gameObject, false);
    }

    public void OnAnyElementSelected(GameObject selected)
    {
        if (selected.GetComponent<ShapeElementGameObject>().element.Name != nameText.text) return;
        elemNameImage.color = selColor;

        //Expand the element when selected, if it is not already expanded.
        if (collapseButtonImage.sprite != collapseIcon)
        {
            OnElementExpanded();
        }

        manager.SnapTo(GetComponent<RectTransform>());
    }

    public void OnAnyElementDeselected(GameObject deselected)
    {
        if (deselected.GetComponent<ShapeElementGameObject>().element.Name != nameText.text) return;
        elemNameImage.color = unselColor;
    }
}
