using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
            
            //editPulleys.transform.position = cSelected.transform.position;
            //editPulleys.transform.rotation = cSelected.transform.rotation;
            //editPulleys.SetActive(true);
            uiElements.OnElementSelected(cSelected.GetComponent<ShapeElementGameObject>());
        }

        private void OnObjectDeselcted(GameObject deSelected)
        {
            if (EditModeManager.main.cEditMode != VSEditMode.Model) return;
            
            editPulleys.gameObject.SetActive(false);
        }

        public void OnUpdateOverSceneView(Vector2 mouseScenePositionForCamera)
        {
            //base.OnUpdateOverSceneView(mouseScenePositionForCamera);
            /*if (isXGizmoDown)
            {
                //Okay, so far we have a "perceived translation" value, which is (in pixels) the increase (or decrease) that the object should be moved by.
                //We want to move that amount along the appropriate axis, and then convert back into world space. I think.
                Vector2 diff = mouseScenePositionForCamera - sceneMousePosOnGizmoDown;
                Vector2 relDiff = diff * xGizmoPositiveDirection;
                float relDiffFloat = (relDiff.x + relDiff.y);
                SetPosition(EnumAxis.X, (float)(cXVal + (relDiffFloat * (1 / 16f))));
            }*/
        }

        public bool OnSceneViewMouseDown(Vector2 mouseClickScenePositionForCamera, PointerEventData data)
        {
            //return false;
            //Temp removed. Come back to later.
            /*
            if (data.button != 0 || !objectSelector.IsAnySelected()) return false;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(mouseClickScenePositionForCamera), out RaycastHit hit, float.MaxValue, LayerMask.GetMask("Edit Pulleys")))
            {
                ShapeElement cElem = objectSelector.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element;
                if (hit.collider.gameObject.name.StartsWith("X"))
                {
                    isXGizmoDown = true;
                    sceneMousePosOnGizmoDown = mouseClickScenePositionForCamera;
                    Vector3 screenSpaceOfObject = Camera.main.WorldToScreenPoint(cElem.gameObject.transform.position);
                    distFromCam = screenSpaceOfObject.z;
                    Vector3 screenSpaceOfPointOfMovement = Camera.main.WorldToScreenPoint(cElem.gameObject.transform.position + cElem.gameObject.transform.right);
                    xGizmoPositiveDirection = (screenSpaceOfPointOfMovement - screenSpaceOfObject).normalized;
                    cXVal = cElem.From[0];
                    Debug.Log("X Gizmo down with a positive direction of " + xGizmoPositiveDirection);
                }
                return true;
            }
            */
            return false;
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