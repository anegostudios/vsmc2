using UnityEngine;

namespace VSMC
{
    public class ShapeEditor : MonoBehaviour
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

        public void OnSceneRaycast()
        {

            //if no valid raycast...
            //cameraController.SceneViewMouseDown
        }

    }
}