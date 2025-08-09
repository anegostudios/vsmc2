using UnityEngine;
using UnityEngine.EventSystems;

namespace VSMC
{
    public class ShapeModelEditor : MonoBehaviour
    {
        [Header("Unity References")]
        public CameraController cameraController;
        public GameObject editPulleys;
        public ObjectSelector objectSelector;
        public ElementHierarchyManager elementHierarchyManager;

        [Header("UI References")]
        public ShapeEditorUIElements uiElements;

        private void Start()
        {
            objectSelector.RegisterForObjectSelectedEvent(OnObjectSelected);
            objectSelector.RegisterForObjectDeselectedEvent(OnObjectDeselcted);
            EditModeManager.RegisterForOnModeSelect(OnEditModeSelect);
            EditModeManager.RegisterForOnModeDeselect(OnEditModeDeselect);
        }

        private void OnObjectSelected(GameObject cSelected)
        {
            if (EditModeManager.main.cEditMode != VSEditMode.Model) return;
            uiElements.OnElementSelected(cSelected.GetComponent<ShapeElementGameObject>());
        }

        private void OnObjectDeselcted(GameObject deSelected)
        {
            if (EditModeManager.main.cEditMode != VSEditMode.Model) return;
            
            editPulleys.gameObject.SetActive(false);
        }

        public void SetSize(EnumAxis axis, float value)
        {
            if (!objectSelector.IsAnySelected()) return;
            ShapeElement cElem = objectSelector.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element;
            TaskSetElementSize ssTask = new TaskSetElementSize(cElem, axis, value);
            ssTask.DoTask();
            UndoManager.main.CommitTask(ssTask);
        }

        public void SetPosition(EnumAxis axis, float value, int translationUid = 0)
        {
            if (!objectSelector.IsAnySelected()) return;
            ShapeElement cElem = objectSelector.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element;

            TaskSetElementPosition spTask = new TaskSetElementPosition(cElem, axis, value, translationUid);
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
        }

        void OnEditModeDeselect(VSEditMode desel)
        {
            if (desel != VSEditMode.Model) return;
        }

        public void CreateNewShapeElement()
        {
            if (!objectSelector.IsAnySelected()) return;
            ShapeElement cElem = objectSelector.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element;

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

            foreach (Animation anim in ShapeLoader.main.shapeHolder.cLoadedShape.Animations)
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

            return newName;
        }
    }
}