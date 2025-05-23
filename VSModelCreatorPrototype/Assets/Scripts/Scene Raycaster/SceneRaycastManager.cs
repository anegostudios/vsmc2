using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

namespace VSMC {

    /*
     This class will decide what object receives mouse events and when.
     Some of this gets quite complex, mainly for LMB:
      - 3D edit controls always take priority for LMB.
      - If LMB down and:
        - The mouse moves, then the camera takes next priority.
        - The mouse does not move and a mouse up event is received, then the object selection takes priorty. 
    
     RMB and scroll wheel are fairly simple right now - Camera takes priority.
    */

    /// <summary>
    /// To allow more versatile window control, the entire scene is rendered to a flat texture and then used as a UI image.
    /// This means that there are a few more calculations needed when handling raycasts from the mouse pointer, since we need to take into account the UI position too.
    /// Thankfully, Unity has a fair few functions to help with this.
    /// </summary>
    public class SceneRaycastManager : MonoBehaviour
    {
        [Header("Unity References")]
        public RawImage sceneViewRawImage;
        public Camera sceneViewCamera;
        public CameraController cameraController;
        public ObjectSelector objectSelector;

        ISceneRaycaster[] sceneRaycastersByPriority;

        private void Start()
        {
            sceneRaycastersByPriority = new ISceneRaycaster[]
            {
                //Edit Controls
                cameraController,
                objectSelector
            };
        }

        /// <summary>
        /// Called by Unity's event system, when the mouse is released after being pressed on the scene image.
        /// </summary>
        /// <param name="data">Data passed in by the event.</param>
        public void OnMouseUpEvent(BaseEventData data)
        {
            PointerEventData pData = data as PointerEventData;
            for (int i = 0; i < sceneRaycastersByPriority.Length; i++)
            {
                if (sceneRaycastersByPriority[i].OnSceneViewMouseUp(pData))
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Called by Unity's event system, when the scene view image is clicked down on.
        /// </summary>
        /// <param name="data">Data passed in by the event.</param>
        public void OnMouseDownEvent(BaseEventData data)
        {
            PointerEventData pData = data as PointerEventData;
            Vector2 pos;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(sceneViewRawImage.GetComponent<RectTransform>(), pData.position, GetComponent<Camera>(), out pos))
            {
                pos += sceneViewRawImage.GetComponent<RectTransform>().rect.size / 2;
                for (int i = 0; i < sceneRaycastersByPriority.Length; i++)
                {
                    if (sceneRaycastersByPriority[i].OnSceneViewMouseDown(pos, pData))
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Called by Unity's event system, when the mouse wheel is scrolled over the scene image.
        /// </summary>
        /// <param name="data">Data passed in by the event.</param>
        public void OnMouseScrollEvent(BaseEventData data)
        {
            PointerEventData pData = data as PointerEventData;
            for (int i = 0; i < sceneRaycastersByPriority.Length; i++)
            {
                if (sceneRaycastersByPriority[i].OnSceneViewMouseScroll(pData))
                {
                    break;
                }
            }
        }
    }
}