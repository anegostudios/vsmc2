using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace VSMC {
    public class TimelineManager : MonoBehaviour
    {

        public RectTransform timelineRectTransform;
        public RectTransform framelineHolderForWidth;
        public KeyframeSelector kfSelector = new KeyframeSelector();

        /// <summary>
        /// The current number of pixels for one column in the timeline.
        /// </summary>
        public int pixelsPerColumn = 16;


        [Header("Element List")]
        public Transform elementListHolder;
        public GameObject elementListEntryPrefab;

        [Header("Timeline List")]
        public Transform timelineListHolder;
        public GameObject timelineListEntryPrefab;
        public Color timelineListHeaderRowColor;
        public Color timelineListMainRowColor;
        public GameObject translationPrefab;
        public GameObject rotationPrefab;

        [Header("Scroll View Whole")]
        public ScrollRect scrollrect;
        public RectTransform wholeContentRect;

        public void OnNewAnimationSelected(Animation animation)
        {
            //Clear previous entries...
            foreach (Transform t in elementListHolder)
            {
                Destroy(t.gameObject);
            }
            foreach (Transform t in timelineListHolder)
            {
                Destroy(t.gameObject);
            }

            //Get a list of all the elements used in this animation...
            Dictionary<ShapeElement, List<AnimationKeyFrameElement>> animatedElems = new Dictionary<ShapeElement, List<AnimationKeyFrameElement>>();
            foreach (AnimationKeyFrame kf in animation.KeyFrames)
            {
                foreach (KeyValuePair<string, AnimationKeyFrameElement> elem in kf.Elements)
                {
                    ShapeElement se = ShapeElementRegistry.main.GetShapeElementByName(elem.Key);
                    if (!animatedElems.ContainsKey(se))
                    {
                        animatedElems[se] = new List<AnimationKeyFrameElement>();
                    }
                    animatedElems[se].Add(elem.Value);
                }
            }

            kfSelector.Reinitialize();

            foreach (KeyValuePair<ShapeElement, List<AnimationKeyFrameElement>> animatedElem in animatedElems)
            {
                GameObject h = Instantiate(timelineListEntryPrefab, timelineListHolder);
                h.GetComponent<Image>().color = timelineListHeaderRowColor;
                GameObject t = Instantiate(timelineListEntryPrefab, timelineListHolder);
                t.GetComponent<Image>().color = timelineListMainRowColor;
                GameObject r = Instantiate(timelineListEntryPrefab, timelineListHolder);
                r.GetComponent<Image>().color = timelineListMainRowColor;
                Instantiate(elementListEntryPrefab, elementListHolder).GetComponent<TimelineElementCollapsable>().Initialize(this, animatedElem.Key.Name, t, r);

                foreach (AnimationKeyFrameElement kf in animatedElem.Value)
                {
                    int frame = kf.Frame;
                    float posOffset = frame * 32 + 8;
                    if (kf.PositionSet)
                    {
                        GameObject pkf = Instantiate(translationPrefab, t.transform);
                        pkf.GetComponent<RectTransform>().localPosition = new Vector3(posOffset, 0);
                        pkf.GetComponent<TimelineKeyFrameElementMarker>().Initialize(this, framelineHolderForWidth, frame, kf, 0, kfSelector);
                    }
                    if (kf.RotationSet)
                    {
                        GameObject rkf = Instantiate(rotationPrefab, r.transform);
                        rkf.GetComponent<RectTransform>().localPosition = new Vector3(posOffset, 0);
                        rkf.GetComponent<TimelineKeyFrameElementMarker>().Initialize(this, framelineHolderForWidth, frame, kf, 1, kfSelector);
                    }
                }

            }

        }

        public void SnapTo(RectTransform target)
        {
            Canvas.ForceUpdateCanvases();

            Vector2 prefPos = (Vector2)scrollrect.transform.InverseTransformPoint(wholeContentRect.GetComponent<RectTransform>().position)
                    - (Vector2)scrollrect.transform.InverseTransformPoint(target.position);
            prefPos.x = wholeContentRect.anchoredPosition.x;
            prefPos.y -= pixelsPerColumn * 6;

            wholeContentRect.GetComponent<RectTransform>().anchoredPosition = prefPos;

        }

        public void OnKFEMarkerMoved(int moveAmount)
        {
            List<TimelineKeyFrameElementMarker> sel = kfSelector.currentSelectedKeyframeMarkers;
            foreach (TimelineKeyFrameElementMarker marker in sel)
            {
                if (marker.assosciatedKFE.Frame + moveAmount < 0)
                {
                    //Frame out of bounds. Reset the markers and do nothing.
                    foreach (TimelineKeyFrameElementMarker marker2 in sel)
                    {
                        marker2.ResetPosition();
                    }
                    return;
                }
            }
            //Now we should be valid...
            TaskMoveMultiKeyFrameElementsFrames moveKfeTask =
            new TaskMoveMultiKeyFrameElementsFrames(AnimationSelector.main.GetCurrentlySelected(),
                                                    sel, moveAmount);
            moveKfeTask.DoTask();
            UndoManager.main.CommitTask(moveKfeTask);
            //Gods.PrayTo(); 
        }

    }
}