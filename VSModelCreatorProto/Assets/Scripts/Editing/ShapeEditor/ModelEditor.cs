using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.Rendering.DebugUI;

namespace VSMC
{
/// <summary>
/// This controls everything to do with editing base ShapeElements properties.
/// This includes position, rotation, scale, rotation origin, as well as creation, deletion, and renaming.
/// UI Controls and interactions for this class are split into <see cref="ModelEditorUIElements"/>.
/// </summary>
public class ModelEditor : MonoBehaviour
{
    [Header("Unity References")]
    public CameraController cameraController;
    public ObjectSelector objectSelector;
    public ElementHierarchyManager elementHierarchyManager;
    public ReparentElementOverlay reparentElementOverlay;
    public SetStepparentElementOverlay stepParentOverlay;

    [Header("UI References")]
    public ModelEditorUIElements uiElements;

    private void Start()
    {
        objectSelector.RegisterForObjectSelectedEvent(OnObjectSelected);
        objectSelector.RegisterForObjectDeselectedEvent(OnObjectDeselcted);
        EditModeManager.RegisterForOnModeSelect(OnEditModeSelect);
        EditModeManager.RegisterForOnModeDeselect(OnEditModeDeselect);
        uiElements.HideAllUIElements();
        UndoManager.RegisterForAnyActionDoneOrUndone(OnAnyAction);
    }

    public void OnAnyAction()
    {
        if (EditModeManager.main.cEditMode != VSEditMode.Model) return;
        uiElements.RefreshSelectionValues();
    }

    private void OnObjectSelected(GameObject cSelected)
    {
        if (EditModeManager.main.cEditMode != VSEditMode.Model) return;
        uiElements.OnElementSelected(cSelected.GetComponent<ShapeElementGameObject>());
        uiElements.ShowAllUIElements();
    }

    private void OnObjectDeselcted(GameObject deSelected)
    {
        if (EditModeManager.main.cEditMode != VSEditMode.Model) return;
        uiElements.HideAllUIElements();
    }

    public void SetSize(EnumAxis axis, float value)
    {
        if (!objectSelector.IsAnySelected()) return;
        ShapeElement cElem = objectSelector.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element;
        TaskSetElementSize ssTask = new TaskSetElementSize(cElem, axis, value);
        ssTask.DoTask();
        UndoManager.main.CommitTask(ssTask);
    }

    public void SetPosition(EnumAxis axis, float value)
    {
        if (!objectSelector.IsAnySelected()) return;
        ShapeElement cElem = objectSelector.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element;

        TaskSetElementPosition spTask = new TaskSetElementPosition(cElem, axis, value, !Input.GetKey(KeyCode.LeftControl));
        spTask.DoTask();
        UndoManager.main.CommitTask(spTask);
    }
    

    public void SetRotationOrigin(EnumAxis axis, float value)
    {
        if (!objectSelector.IsAnySelected()) return;
        ShapeElement cElem = objectSelector.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element;

        TaskSetElementRotationOrigin soTask = new TaskSetElementRotationOrigin(cElem, axis, value);
        soTask.DoTask();
        UndoManager.main.CommitTask(soTask);
    }

    public void SetRotation(EnumAxis axis, float value)
    {
        if (!objectSelector.IsAnySelected()) return;
        ShapeElement cElem = objectSelector.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element;

        TaskSetElementRotation srTask = new TaskSetElementRotation(cElem, axis, value);
        srTask.DoTask();
        UndoManager.main.CommitTask(srTask);
    }

    public string SetClimateColorMap(string value)
    {
        if (!objectSelector.IsAnySelected()) return "";
        ShapeElement cElem = objectSelector.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element;

        TaskSetClimateColorMap srTask = new TaskSetClimateColorMap(cElem, value);
        srTask.DoTask();
        UndoManager.main.CommitTask(srTask);
        return cElem.ClimateColorMap;
    }

    public string SetSeasonColorMap(string value)
    {
        if (!objectSelector.IsAnySelected()) return "";
        ShapeElement cElem = objectSelector.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element;

        TaskSetSeasonColorMap srTask = new TaskSetSeasonColorMap(cElem, value);
        srTask.DoTask();
        UndoManager.main.CommitTask(srTask);
        return cElem.SeasonColorMap;
    }

    public EnumRenderPass SetRenderPass(EnumRenderPass value)
    {
        if (!objectSelector.IsAnySelected()) return EnumRenderPass.Opaque;
        ShapeElement cElem = objectSelector.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element;

        TaskSetRenderPass srTask = new TaskSetRenderPass(cElem, value);
        srTask.DoTask();
        UndoManager.main.CommitTask(srTask);
        return (EnumRenderPass)(cElem.RenderPass);
    }

    public short SetZOffset(short value)
    {
        if (!objectSelector.IsAnySelected()) return 0;
        ShapeElement cElem = objectSelector.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element;

        TaskSetZOffset srTask = new TaskSetZOffset(cElem, value);
        srTask.DoTask();
        UndoManager.main.CommitTask(srTask);
        return cElem.ZOffset;
    }

    void OnEditModeSelect(VSEditMode sel)
    {
        if (sel != VSEditMode.Model) return;
        ShapeLoader.main.shapeHolder.ReparentGameObjectsToNoJoints();
        if (!objectSelector.IsAnySelected())
        {
            OnObjectDeselcted(null);
        }
        else
        {
            OnObjectSelected(objectSelector.GetCurrentlySelected());
        }
    }

    void OnEditModeDeselect(VSEditMode desel)
    {
        if (desel != VSEditMode.Model) return;
    }

    public void CreateNewShapeElement()
    {
        ShapeElement cElem = null;
        if (objectSelector.IsAnySelected())
        {
            cElem = objectSelector.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element;
        }

        TaskCreateNewElement cnTask = new TaskCreateNewElement(cElem);
        cnTask.DoTask();
        UndoManager.main.CommitTask(cnTask);
    }

    public void DeleteSelectedShapeElement()
    {
        if (!objectSelector.IsAnySelected()) return;
        ShapeElement cElem = objectSelector.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element;

        TaskDeleteElement deTask = new TaskDeleteElement(cElem);
        deTask.DoTask();
        UndoManager.main.CommitTask(deTask);
    }

    /// <summary>
    /// Renames an element, if possible. Returns either the new name if success, or the old name if failed.
    /// </summary>
    public string RenameElement(string newName)
    {
        //Need to rename the element, but then swap all names in the shape animations too...
        if (!objectSelector.IsAnySelected()) return "";
        ShapeElement cElem = objectSelector.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element;
        TaskRenameElement renameTask = new TaskRenameElement(cElem, newName);
        renameTask.DoTask();
        UndoManager.main.CommitTask(renameTask);
        return renameTask.newName;
    }

    public void CopyElement()
    {
        if (!objectSelector.IsAnySelected()) return;
        ShapeElement cElem = objectSelector.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element;
        TaskCopyElement copyTask = new TaskCopyElement(cElem);
        copyTask.DoTask();
        UndoManager.main.CommitTask(copyTask);
    }

    public void OpenReparentMenu()
    {
        if (!objectSelector.IsAnySelected()) return;
        reparentElementOverlay.OpenOverlay(objectSelector.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element);
    }

    public void ReparentElement(int elemToReparentUID, int newParentUID)
    {
        TaskReparentElement reTask = new TaskReparentElement(elemToReparentUID, newParentUID);
        reTask.DoTask();
        UndoManager.main.CommitTask(reTask);
    }

    public void OpenStepParentMenu()
    {
        if (!objectSelector.IsAnySelected()) return;
        stepParentOverlay.OpenOverlay(objectSelector.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element);
    }

    public void SetStepParentElement(int elemToSetStepparent, string stepParentCode)
    {
        ShapeElement toChange = ShapeElementRegistry.main.GetShapeElementByUID(elemToSetStepparent);
        ShapeElement t = ShapeElementRegistry.main.GetShapeElementByName(stepParentCode);
        if (t != null)
        {
            //We know that the current selected is a root elem, so we can speed up the check by just finding the topmost parent.
            while (t.ParentElement != null)
            {
                t = t.ParentElement;
            }
            if (t == toChange)
            {
                //No.
                uiElements.RefreshSelectionValues();
                return;
            }
        }
        TaskSetElementStepparent setStepparentTask = new TaskSetElementStepparent(toChange, stepParentCode);
        setStepparentTask.DoTask();
        UndoManager.main.CommitTask(setStepparentTask);
    }

    public void SetStepParentElement(string stepParentCode)
    {
        //Ugh. Need to ensure that the new step parent is not a child of the selected.
        ShapeElement t = ShapeElementRegistry.main.GetShapeElementByName(stepParentCode);
        if (t != null)
        {
            //We know that the current selected is a root elem, so we can speed up the check by just finding the topmost parent.
            while (t.ParentElement != null)
            {
                t = t.ParentElement;
            }
            if (t == objectSelector.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element)
            {
                //No.
                InfoLogger.main.LogText("Cannot set step-parent as selected object has a child with the given code.");
                uiElements.RefreshSelectionValues();
                return;
            }
        }
        TaskSetElementStepparent setStepparentTask = new TaskSetElementStepparent(objectSelector.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element, stepParentCode);
        setStepparentTask.DoTask();
        UndoManager.main.CommitTask(setStepparentTask);
    }
}
}