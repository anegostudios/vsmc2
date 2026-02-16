using UnityEngine;
using UnityEngine.EventSystems;

namespace VSMC
{
    /// <summary>
    /// This is an interface component that is used to detect appropriate events when the object scene is clicked on or otherwise interacted with.
    /// Instances are managed by the <see cref="SceneRaycastManager"/> which will determine whether the instance should receive the event.
    /// </summary>
    public abstract class ISceneRaycaster : MonoBehaviour
    {

        /// <summary>
        /// Called when a mouse button is pressed down in the scene view.
        /// Should return true if the event is handled.
        /// </summary>
        /// <param name="mouseClickScenePositionForCamera">The mouse position local to the bottom left of the scene image.</param>
        /// <param name="data">The passed event data.</param>
        public abstract bool OnSceneViewMouseDown(Vector2 mouseClickScenePositionForCamera, PointerEventData data);

        /// <summary>
        /// Called when a mouse wheel is scrolled in the scene view.
        /// Should return true if the event is handled.
        /// </summary>
        /// <param name="data">The passed event data.</param>
        public abstract bool OnSceneViewMouseScroll(PointerEventData data);

        /// <summary>
        /// Called when a mouse button is released after being presed down in the scene view.
        /// Use with caution. There is always the chance that the current mouse position is not inside the scene view at this point.
        /// Sometimes it may be better to check in the update frame for a mouse up event.
        /// Should return true if the event is handled.
        /// </summary>
        /// <param name="data">The passed event data.</param>
        public abstract bool OnSceneViewMouseUp(PointerEventData data);

        public virtual void OnUpdateOverSceneView(Vector2 mouseScenePositionForCamera)
        {

        }

    }
}