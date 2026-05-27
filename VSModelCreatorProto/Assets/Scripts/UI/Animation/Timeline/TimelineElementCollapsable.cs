using System.Collections.Generic;
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

    ShapeElement elem;
    string animCode;

    TimelineManager manager;

    public void Initialize(TimelineManager manager, string elemName, ShapeElement elem, string animCode, params GameObject[] linkedCollapses)
    {
        this.manager = manager;
        this.elem = elem;
        this.animCode = animCode;
        nameText.text = elemName;
        elemsToExpandAndCollapse = elemsToExpandAndCollapse.Concat(linkedCollapses).ToArray();
        ObjectSelector.main.RegisterForObjectSelectedEvent(OnAnyElementSelected);
        ObjectSelector.main.RegisterForObjectDeselectedEvent(OnAnyElementDeselected);
        elemNameImage.color = unselColor;
        if (manager.collapsedElements.TryGetValue(animCode, out List<ShapeElement> elems))
        {
            if (elems.Contains(elem))
            {
                OnElementCollapsed();
                return;
            }
        }
        OnElementExpanded();
    }

    public void OnCollapseButtonClick()
    {
        if (collapseButtonImage.sprite == collapseIcon)
        {
            OnElementCollapsed();

            //Add element to collapsed list.
            if (manager.collapsedElements.TryGetValue(animCode, out List<ShapeElement> elems))
            {
                if (!elems.Contains(elem))
                {
                    elems.Add(elem);
                }
            }
            else
            {
                manager.collapsedElements.Add(animCode, new List<ShapeElement>()
                {
                    elem
                });
            }
        }
        else
        {
            OnElementExpanded();

            //Remove element from collapsed list.
            if (manager.collapsedElements.TryGetValue(animCode, out List<ShapeElement> elems))
            {
                elems.Remove(elem);
            }
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
