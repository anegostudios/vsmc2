using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.EventSystems;

namespace VSMC
{
    public class ShapeEditor : ISceneRaycaster
    {
        [Header("Unity References")]
        public CameraController cameraController;
        public GameObject editPulleys;
        public ObjectSelector objectSelector;

        [Header("UI References")]
        public ShapeEditorUIElements uiElements;

        private void Start()
        {
            objectSelector.RegisterForObjectSelectedEvent(OnObjectSelected);
            objectSelector.RegisterForObjectDeselectedEvent(OnObjectDeselcted);
        }

        private void OnObjectSelected(GameObject cSelected)
        {
            foreach (LineRenderer lines in cSelected.GetComponentsInChildren<LineRenderer>())
            {
                lines.enabled = true;
            }
            editPulleys.transform.position = cSelected.transform.position;
            editPulleys.transform.rotation = cSelected.transform.rotation;
            editPulleys.SetActive(true);
            uiElements.OnElementSelected(cSelected.GetComponent<ShapeElementGameObject>());
        }

        private void OnObjectDeselcted(GameObject deSelected)
        {
            foreach (LineRenderer lines in deSelected.GetComponentsInChildren<LineRenderer>())
            {
                lines.enabled = false;
            }
            editPulleys.gameObject.SetActive(false);
        }

        public override bool OnSceneViewMouseDown(Vector2 mouseClickScenePositionForCamera, PointerEventData data)
        {
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

        public override bool OnSceneViewMouseScroll(PointerEventData data)
        {
            return false;
        }

        public override bool OnSceneViewMouseUp(PointerEventData data)
        {
            return false;
        }
    }
}