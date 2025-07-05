using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace VSMC
{
    public class ShapeModelEditor : ISceneRaycaster
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
            
            editPulleys.transform.position = cSelected.transform.position;
            editPulleys.transform.rotation = cSelected.transform.rotation;
            editPulleys.SetActive(true);
            uiElements.OnElementSelected(cSelected.GetComponent<ShapeElementGameObject>());
        }

        private void OnObjectDeselcted(GameObject deSelected)
        {
            if (EditModeManager.main.cEditMode != VSEditMode.Model) return;
            
            editPulleys.gameObject.SetActive(false);
        }

        public override bool OnSceneViewMouseDown(Vector2 mouseClickScenePositionForCamera, PointerEventData data)
        {
            return false;
            //Temp removed. Come back to later.
            if (data.button != 0 || !objectSelector.IsAnySelected()) return false;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(mouseClickScenePositionForCamera), out RaycastHit hit, float.MaxValue, LayerMask.GetMask("Edit Pulleys")))
            {
                ShapeElement cElem = objectSelector.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element;
                if (hit.collider.gameObject.name.StartsWith("X"))
                {
                    Debug.Log("Inc X by 1");
                    cElem.From[0] += 1;
                    cElem.To[0] += 1;
                }
                else if (hit.collider.gameObject.name.StartsWith("Y"))
                {
                    Debug.Log("Inc Y by 1");
                    cElem.From[1] += 1;
                    cElem.To[1] += 1;
                }
                else if (hit.collider.gameObject.name.StartsWith("Z"))
                {
                    Debug.Log("Inc Z by 1");
                    cElem.From[2] += 1;
                    cElem.To[2] += 1;
                }
                
                ShapeTesselator.ResolveMatricesForShapeElementAndChildren(cElem);
                objectSelector.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().ReapplyTransformsFromMeshData(true);
                return true;
            }
            return false;

        }

        public void SetSize(EnumAxis axis, float value)
        {
            if (!objectSelector.IsAnySelected()) return;
            ShapeElement cElem = objectSelector.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element;
            //Add the size based on the axis to the 'to' array.
            int index = (int)axis;
            cElem.To[index] = cElem.From[index] + value;
            
            //Recreate the mesh from the selected object.
            RecreateObjectMeshAndTransforms();
        }

        public void SetSize(Vector3 value)
        {
            if (!objectSelector.IsAnySelected()) return;
            ShapeElement cElem = objectSelector.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element;
            //Get existing from value, then add to size to find to.
            Vector3 temp = new Vector3((float)cElem.From[0], (float)cElem.From[1], (float)cElem.From[2]);
            temp += value;
            cElem.To = new double[] { temp.x, temp.y, temp.z };

            //Recreate the mesh from the selected object.
            RecreateObjectMeshAndTransforms();
        }

        public void SetPosition(EnumAxis axis, float value)
        {
            if (!objectSelector.IsAnySelected()) return;
            ShapeElement cElem = objectSelector.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element;
            //Position needs to add to the 'from' and 'to' values whilst keeping the size the same.
            int index = (int)axis;
            double size = cElem.To[index] - cElem.From[index];
            cElem.From[index] = value;
            cElem.To[index] = cElem.From[index] + size;

            //Recreate the mesh from the selected object.
            RecreateObjectTransforms();
        }

        public void SetPosition(Vector3 value)
        {
            if (!objectSelector.IsAnySelected()) return;
            ShapeElement cElem = objectSelector.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element;
            //Position needs to add to the 'from' and 'to' values whilst keeping the size the same.
            Vector3 size = new Vector3(
                (float)(cElem.To[0] - cElem.From[0]),
                (float)(cElem.To[1] - cElem.From[1]),
                (float)(cElem.To[2] - cElem.From[2]));
            cElem.From = new double[] { value.x, value.y, value.z };
            cElem.To = new double[] { value.x + size.x, value.y + size.y, value.z + size.z };

            //Recreate the mesh from the selected object.
            RecreateObjectTransforms();
        }

        public void SetRotationOrigin(EnumAxis axis, float value)
        {
            if (!objectSelector.IsAnySelected()) return;
            ShapeElement cElem = objectSelector.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element;
            //Origin nice and easy.
            cElem.RotationOrigin[(int)axis] = value;

            //Recreate the mesh from the selected object.
            RecreateObjectTransforms();
        }

        public void SetRotationOrigin(Vector3 value)
        {
            if (!objectSelector.IsAnySelected()) return;
            ShapeElement cElem = objectSelector.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element;
            //Origin nice and easy.
            cElem.RotationOrigin = new double[] { value.x, value.y, value.z };

            //Recreate the mesh from the selected object.
            RecreateObjectTransforms();
        }

        public void SetRotation(EnumAxis axis, float value)
        {
            if (!objectSelector.IsAnySelected()) return;
            ShapeElement cElem = objectSelector.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element;

            if (axis == EnumAxis.X) cElem.RotationX = value;
            else if (axis == EnumAxis.Y) cElem.RotationY = value;
            else if (axis == EnumAxis.Z) cElem.RotationZ = value;

            //Recreate the mesh from the selected object.
            RecreateObjectTransforms();
        }

        public void SetRotation(Vector3 value)
        {
            if (!objectSelector.IsAnySelected()) return;
            ShapeElement cElem = objectSelector.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element;
            //Origin nice and easy.

            cElem.RotationX = value.x;
            cElem.RotationY = value.y;
            cElem.RotationZ = value.z;

            //Recreate the mesh from the selected object.
            RecreateObjectTransforms();
        }

        public void RecreateObjectTransforms()
        {
            if (!objectSelector.IsAnySelected()) return;
            ShapeElement cElem = objectSelector.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element;
            //Retesselate the shapes, and then reapply the transforms for the object.
            ShapeTesselator.ResolveMatricesForShapeElementAndChildren(cElem);
            objectSelector.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().ReapplyTransformsFromMeshData(true);
        }

        public void RecreateObjectMesh()
        {
            if (!objectSelector.IsAnySelected()) return;
            ShapeElement cElem = objectSelector.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element;
            //Retesselate the shapes, and then reapply the meshes.
            ShapeTesselator.RecreateMeshesForShapeElement(cElem);
            objectSelector.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().RegenerateMeshFromMeshData();
        }

        public void RecreateObjectMeshAndTransforms()
        {
            if (!objectSelector.IsAnySelected()) return;
            ShapeElement cElem = objectSelector.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element;
            //Retesselate the shapes, and then reapply the transforms for the object.
            ShapeTesselator.RecreateMeshesForShapeElement(cElem);
            ShapeTesselator.ResolveMatricesForShapeElementAndChildren(cElem);
            objectSelector.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().RegenerateMeshFromMeshData();
            objectSelector.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().ReapplyTransformsFromMeshData(true);
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
            /*
             * Okay, this function is quite a horrid bit of code.
             * I feel like it should be much more streamlined to add a new element.
             */
            if (!objectSelector.IsAnySelected()) return;
            ShapeElement cElem = objectSelector.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element;
            ShapeElement newElem = new ShapeElement()
            {
                From = new double[] { 0, 0, 0 },
                To = new double[] { 1, 1, 1 },
                ParentElement = cElem,
                Name = "New Object",
            };
            newElem.ResolveReferncesAndUIDs();
            newElem.FacesResolved = new ShapeElementFace[6];
            for (int i = 0; i < 6; i++)
            {
                newElem.FacesResolved[i] = new ShapeElementFace()
                {
                    Enabled = true,
                    Uv = new float[] { 0, 0, 1, 1}
                };
            }
            ShapeElement[] newChildren = new ShapeElement[cElem.Children == null ? 1 : cElem.Children.Length + 1];
            cElem.Children?.CopyTo(newChildren, 0);
            newChildren[newChildren.Length - 1] = newElem;
            cElem.Children = newChildren;
            ShapeTesselator.TesselateShapeElements(new ShapeElement[] { newElem }, ShapeLoader.main.shapeHolder.cLoadedShape.TextureSizeMultipliers);
            ShapeTesselator.ResolveMatricesForShapeElementAndChildren(newElem);
            ShapeLoader.main.shapeHolder.CreateShapeElementGameObject(newElem);
            elementHierarchyManager.StartCreatingElementPrefabs(ShapeLoader.main.shapeHolder.cLoadedShape);
            objectSelector.SelectObject(newElem.gameObject.gameObject, false, false);
        }

        public void DeleteSelectedShapeElement()
        {
            if (!objectSelector.IsAnySelected()) return;
            ShapeElement cElem = objectSelector.GetCurrentlySelected().GetComponent<ShapeElementGameObject>().element;
            List<string> removedElements = new List<string>();
            DeleteShapeElement(cElem, removedElements);

            //Now delete animation entries which were in removedElements.
            foreach (Animation anim in ShapeLoader.main.shapeHolder.cLoadedShape.Animations)
            {
                foreach (AnimationKeyFrame keyFrame in anim.KeyFrames)
                {
                    foreach (string s in removedElements)
                    {
                        if (keyFrame.Elements.ContainsKey(s))
                        {
                            keyFrame.Elements.Remove(s);
                        }
                    }
                }
            }

            //Only need to remove the parent. GC should clear the rest. Also will make undo code easier.
            cElem.ParentElement.Children = cElem.ParentElement.Children.Remove(cElem);

        }

        private void DeleteShapeElement(ShapeElement elem, List<string> removedElements)
        {
            /* Deletion is a little more complex.
             * We need to delete all children first, and the childrens children, so on, so call this recursively.
             * We need to remove the animation entries - This will be done after all the deletions.
             * We need to then remove the elements from the UI.
             * Then deregister in the element registry.
             * Then delete the gameobject.
             * And finally remove the entry from the shape - Also only done with the selected element, after deletions.
             */
            if (elem.Children != null)
            {
                foreach (ShapeElement child in elem.Children)
                {
                    DeleteShapeElement(child, removedElements);
                }
            }
            removedElements.Add(elem.Name);
            Destroy(elementHierarchyManager.GetElementHierarchyItem(elem).gameObject);
            ShapeElementRegistry.main.UnregisterShapeElement(elem);
            Destroy(elem.gameObject.gameObject);
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

        public override bool OnSceneViewMouseScroll(PointerEventData data) { return false; }
        public override bool OnSceneViewMouseUp(PointerEventData data) { return false; }
    }
}