using UnityEngine;
using UnityEngine.EventSystems;

namespace VSMC
{
    public class ShapeEditor : ISceneRaycaster
    {
        [Header("Unity References")]
        public CameraController cameraController;
        public GameObject editPulleys;
        public ObjectSelector objectSelector;

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