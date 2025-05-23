using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace VSMC
{
    /// <summary>
    /// Responsible for handling what object is currently selected.
    /// Other classes can register for events for when an object is selected and deselected.
    /// </summary>
    public class ObjectSelector : ISceneRaycaster
    {
        UnityEvent<GameObject> OnObjectSelected;
        UnityEvent<GameObject> OnObjectDeSelected;
        GameObject cSelected;

        Vector2 storedMouseClickPosForObjectScrolling;
        RaycastHit[] storedRaycastHits;
        int storedRaycastHitCount;
        int scrollingObjectCounter;

        private void Awake()
        {
            //Allow object scrolling of 32 objects
            storedRaycastHits = new RaycastHit[32];
            OnObjectSelected = new UnityEvent<GameObject>();
            OnObjectDeSelected = new UnityEvent<GameObject>();
        }

        public bool IsAnySelected()
        {
            return cSelected != null;
        }

        public GameObject GetCurrentlySelected()
        {
            return cSelected;
        }

        public void RegisterForObjectSelectedEvent(UnityAction<GameObject> toCall)
        {
            OnObjectSelected.AddListener(toCall);
        }

        public void RegisterForObjectDeselectedEvent(UnityAction<GameObject> toCall)
        {
            OnObjectDeSelected.AddListener(toCall);
        }

        /// <summary>
        /// This should always be called alongside the camera mouse down.
        /// If the camera moves, mouseUp is never called. If the camera does not move, mouseUp is called and the object is selected.
        /// </summary>
        public override bool OnSceneViewMouseDown(Vector2 mouseClickScenePositionForCamera, PointerEventData data)
        {
            if (data.button != 0) return false;

            //Has a raycast already happened at this exact mouse position?
            //If so, then select the next object in the raycast.
            if (mouseClickScenePositionForCamera.Equals(storedMouseClickPosForObjectScrolling) && storedRaycastHitCount > 0)
            {
                scrollingObjectCounter = scrollingObjectCounter + 1;
                return true;
            }
            storedRaycastHitCount = Physics.RaycastNonAlloc(Camera.main.ScreenPointToRay(mouseClickScenePositionForCamera), storedRaycastHits, float.MaxValue, LayerMask.GetMask("SelectableObject"));
            if (storedRaycastHitCount == 0) return false;
            storedMouseClickPosForObjectScrolling = mouseClickScenePositionForCamera;
            scrollingObjectCounter = 0;

            //Order the raycast hits by distance and put them back in the stored array.
            List<RaycastHit> temp = new List<RaycastHit>();
            for (int i = 0; i < storedRaycastHitCount; i++)
            {
                temp.Add(storedRaycastHits[i]);
            }
            temp.OrderByDescending(x => x.distance);
            for (int i = 0; i < storedRaycastHitCount; i++)
            {
                storedRaycastHits[i] = temp[i];
            }
            return true;
        }

        public override bool OnSceneViewMouseScroll(PointerEventData data)
        {
            return false;
        }

        public override bool OnSceneViewMouseUp(PointerEventData data)
        {
            if (data.button != 0) return false;
            if (storedRaycastHitCount <= 0) return false;
            if (scrollingObjectCounter >= storedRaycastHitCount)
            {
                DeselectCurrentObject();
                storedRaycastHitCount = 0;
                scrollingObjectCounter = 0;
                return true;
            }
            SelectObject(storedRaycastHits[scrollingObjectCounter].collider.gameObject, true);
            return true;
        }

        public void SelectObject(GameObject newlySelected, bool deselectIfAlreadySelected = true)
        {
            if (cSelected == newlySelected)
            {
                if (deselectIfAlreadySelected)
                {
                    DeselectCurrentObject();
                }
                return;
            }
            DeselectCurrentObject();
            cSelected = newlySelected;
            OnObjectSelected.Invoke(cSelected);
        }

        public void DeselectCurrentObject()
        {
            if (cSelected == null) return;
            OnObjectDeSelected.Invoke(cSelected);
            cSelected = null;
        }
    }
}