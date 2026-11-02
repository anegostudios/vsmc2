using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VSMC;

public class MultiShapeUIListEntry : MonoBehaviour
{

    [Header("Unity Refs")]
    public TMP_Text shapeName;
    public Image bgImage;
    public GameObject indentObject;
    public Image isEnabledIcon;
    public Image parentEntryCollapseIcon;

    [Header("Runtime Properties")]
    MultiShapeManager manager;
    MultiShapeUIElements ui;
    public Shape shape;
    public ShapeElement rootElem;
    public bool isParentEntry;
    public bool isCurrentlyExpanded;
    public bool isCurrentlyEnabled;
    public string storedFilepath;

    /// <summary>
    /// Initialize as a selectable entry.
    /// </summary>
    public void Initialize(MultiShapeManager manager, MultiShapeUIElements ui, Shape shape, ShapeElement rootElem, bool currentlyEnabled)
    {
        this.manager = manager;
        this.ui = ui;
        this.shape = shape;
        this.rootElem = rootElem;
        indentObject.SetActive(true);
        isEnabledIcon.gameObject.SetActive(true);
        parentEntryCollapseIcon.gameObject.SetActive(false);
        shapeName.text = rootElem.Name;
        isParentEntry = false;
        ResolveEnabledState(currentlyEnabled);
    }

    /// <summary>
    /// Initialize the UI as a parent filepath entry.
    /// </summary>
    public void Initialize(MultiShapeManager manager, MultiShapeUIElements ui, string filepath, bool currentlyExpanded)
    {
        this.manager = manager;
        this.ui = ui;
        shapeName.text = filepath;
        this.storedFilepath = filepath;
        indentObject.SetActive(false);
        isEnabledIcon.gameObject.SetActive(false);
        isParentEntry = true;
        ResolveExpandedState(currentlyExpanded);
    }

    /// <summary>
    /// Called by the UI event.
    /// </summary>
    public void OnClicked()
    {
        if (isParentEntry)
        {
            //Collapse/Expand Shapes
            ResolveExpandedState(!isCurrentlyExpanded);
            ui.OnParentEntryCollapseStateChanged(this);
        }
        else
        {
            //Select
            ui.OnRootElementEntryClicked(this);
        }
    }

    public void OnEnabledClicked()
    {
        if (isParentEntry) return;
        ui.OnRootElementEnabledStateChanged(this);
    }

    /// <summary>
    /// Calls UI-Related stuff for when this entry has been selected.
    /// </summary>
    public void OnSelected()
    {
        bgImage.color = ui.selectedMultishapeColor;
    }

    /// <summary>
    /// Calls UI-Related stuff for when this entry has been deselected.
    /// </summary>
    public void OnDeselected()
    {
        bgImage.color = ui.deselectedMultishapeColor;
    }

    /// <summary>
    /// Sets the expanded state of the object. This doesn't collapse the child elements, though.
    /// </summary>
    public void ResolveExpandedState(bool isExpanded)
    {
        this.isCurrentlyExpanded = isExpanded;
        if (isExpanded)
        {
            parentEntryCollapseIcon.sprite = ui.parentExpandedSprite;
        }
        else
        {
            parentEntryCollapseIcon.sprite = ui.parentCollapsedSprite;
        }
    }

    public void ResolveEnabledState(bool isEnabled)
    {
        isCurrentlyEnabled = isEnabled;
        if (isEnabled)
        {
            isEnabledIcon.sprite = ui.entryEnabledSprite;
        }
        else
        {
            isEnabledIcon.sprite = ui.entryDisabledSprite;
        }
    }

    public IOriginPoint Origin
    {
        get
        {
            return rootElem.OriginPoint;
        }
    }

    public LoadedAdditionalShape GetAsLoadedAdditionalShape()
    {
        return manager.GetLoadedAdditionalShapeFromRootElementCode(rootElem.GetElementCodeInShape());
    }

}
