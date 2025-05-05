using System;
using System.Collections.Generic;
using System.Numerics;

namespace VSMC
{
    public class AnimationFrame
    {
        /// <summary>
        /// The frame number.
        /// </summary>
        public int FrameNumber;

        /// <summary>
        /// The transformations for the root element of the frame.
        /// </summary>
        public List<ElementPose> RootElementTransforms = new List<ElementPose>();


        public AnimationFrame()
        {
        }

    }
}