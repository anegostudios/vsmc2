using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class GenericHierarchyManager : MonoBehaviour
{

    /// <summary>
    /// The entry prefab. This should have a <see cref="GenericHierarchyItemEntry"/> attached.
    /// </summary>
    public GameObject elementPrefab;

    /// <summary>
    /// The parent transform that the prefab should be instantiated into.
    /// </summary>
    public RectTransform hierarchyParent;
    
    /// <summary>
    /// Used to pass scroll events from the elements into the hierarchy
    /// </summary>
    public ScrollRect hierarchyScroll;

    /// <summary>
    /// The input field for filtering the hierarchy results.
    /// </summary>
    public TMP_InputField searchValue;

    protected GenericHierarchyItemEntry cSelected;
    protected bool isSearching = false;
    protected List<GenericHierarchyItemEntry> currentItems;

    /// <summary>
    /// Creates the entries. Make sure to first delete, then create and repopulate the currentItems list.
    /// </summary>
    public abstract void StartCreatingEntries();

    /// <summary>
    /// Filters the search bar. Searching removes any hierarchy, so we have to rebuild the entries when there is no search.
    /// </summary>
    public void OnSearchUpdated(string s)
    {
        if (s == "")
        {
            StartCreatingEntries();
            return;
        }
        else
        {
            foreach (var v in currentItems)
            {
                v.MatchesSearch(s);
            }
        }
    }

    /// <summary>
    /// Selects an item in the hierarchy.
    /// </summary>
    public void SelectItem(GenericHierarchyItemEntry item)
    {
        DeselectCurrentItem();
        cSelected = item;
        cSelected.OnEntrySelected();
        OnAnySelected(cSelected);
    }

    public abstract void OnAnySelected(GenericHierarchyItemEntry item);
    
    /// <summary>
    /// Deselects the current item selected, if one exists.
    /// </summary>
    public void DeselectCurrentItem()
    {
        if (cSelected == null) return;
        cSelected.OnEntryDeselected();
        cSelected = null;
    }

    /// <summary>
    /// Will calculate if needed and minimize the element and repeats for each of its children.
    /// </summary>
    public void DetermineIfElementIsMinimized(GenericHierarchyItemEntry item, bool? isParentMinimized = null)
    {
        GenericHierarchyItemEntry p = item;
        bool minimize = false;
        if (isParentMinimized == true)
        {
            minimize = true;
        }
        else
        {
            p = p.parent;
            while (p != null)
            {
                if (p.shouldMinimize)
                {
                    minimize = true;
                    break;
                }
                p = p.parent;
            }
        }
        item.gameObject.SetActive(!minimize);
        foreach (var v in item.children)
        {
            DetermineIfElementIsMinimized(v, minimize || item.shouldMinimize);
        }
    }

    public GenericHierarchyItemEntry GetSelected()
    {
        return cSelected;
    }



    public IEnumerator JumpScrollviewTo(GenericHierarchyItemEntry item)
    {
        yield return new WaitForEndOfFrame(); //Wait for the recttransforms to process any new or removed entries.
        float v = 1 + (item.GetComponent<RectTransform>().localPosition.y / (hierarchyParent.sizeDelta.y - (hierarchyParent.parent as RectTransform).rect.height));
        hierarchyScroll.verticalNormalizedPosition = Mathf.Clamp01(v);
    }

}
