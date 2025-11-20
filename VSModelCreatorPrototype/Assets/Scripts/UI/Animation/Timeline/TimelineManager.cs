using UnityEngine;

namespace VSMC {
    public class TimelineManager : MonoBehaviour
    {

        public RectTransform timelineRectTransform;

        /// <summary>
        /// The current number of pixels for one column in the timeline.
        /// </summary>
        public int pixelsPerColumn = 16;

        public int ClickPositionToFrameIndex(float clickPos)
        {
            return (int)(clickPos / pixelsPerColumn);
        }

        public int FrameIndexToPosition(int frameIndex)
        {
            return (frameIndex * pixelsPerColumn) + (pixelsPerColumn / 2);
        }

        /// <summary>
        /// This should happen:
        ///  - When a new animation is viewed.
        ///  - When the number of frames is changed. 
        ///  - When the pixels per column is changed.
        /// </summary>
        public void RecalculateWholeTimeline(Animation currentAnimation)
        {
            float tlWidth = currentAnimation.QuantityFrames * pixelsPerColumn;
            
        }
    }
}