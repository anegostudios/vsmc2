using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VSMC {
    public class TimelineManager : MonoBehaviour
    {

        public RectTransform timelineRectTransform;

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
                        Instantiate(translationPrefab, t.transform).GetComponent<RectTransform>().localPosition = new Vector3(posOffset, 0);
                    }
                    if (kf.RotationSet)
                    {
                        Instantiate(rotationPrefab, r.transform).GetComponent<RectTransform>().localPosition = new Vector3(posOffset, 0);
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

    }
}