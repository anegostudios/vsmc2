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

            TaskSetElementPosition spTask = new TaskSetElementPosition(cElem, axis, value);
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
            string oldName = cElem.Name;

            //Check for name set fails.
            if (ShapeElementRegistry.main.GetShapeElementByName(newName) != null) return oldName;
            if (newName.Length < 1) return oldName;

            cElem.Name = newName;
            cElem.gameObject.name = newName;

            //Rename element in UI.
            elementHierarchyManager.GetElementHierarchyItem(cElem).elementName.text = newName;

            if (ShapeHolder.CurrentLoadedShape.Animations != null)
            {
                foreach (Animation anim in ShapeHolder.CurrentLoadedShape.Animations)
                {
                    foreach (AnimationKeyFrame keyFrame in anim.KeyFrames)
                    {
                        if (keyFrame.Elements.ContainsKey(oldName))
                        {
                            keyFrame.Elements[newName] = keyFrame.Elements[oldName];
                            keyFrame.Elements.Remove(oldName);
                        }
                    }
                }
            }

            return newName;
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
    }
}