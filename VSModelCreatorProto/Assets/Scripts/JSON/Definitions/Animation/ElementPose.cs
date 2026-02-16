using System.Collections.Generic;
using UnityEngine;

namespace VSMC
{
    public class ElementPose
    {

        /// <summary>
        /// The element this positioning is for.
        /// </summary>
        public ShapeElement ForElement;

        /// <summary>
        /// The model matrix of this element.
        /// </summary>
        public Matrix4x4 AnimModelMatrix;

        public List<ElementPose> ChildElementPoses = new List<ElementPose>();

        public float degOffX, degOffY, degOffZ;

        public float degX, degY, degZ;
        public float scaleX = 1, scaleY = 1, scaleZ = 1;
        public float translateX, translateY, translateZ;

        public bool RotShortestDistanceX;
        public bool RotShortestDistanceY;
        public bool RotShortestDistanceZ;

        public void Clear()
        {
            degX = 0;
            degY = 0;
            degZ = 0;
            scaleX = 1;
            scaleY = 1;
            scaleZ = 1;
            translateX = 0;
            translateY = 0;
            translateZ = 0;
        }

        public float AngleDegDistance(float start, float end)
        {
            return ((((end - start) % 360) + 540) % 360) - 180;
        }

        /// <summary>
        /// Blends two element poses based on a lerp 'l'.
        /// </summary>
        public void Add(ElementPose tf, ElementPose tfNext, float l, float weight)
        {
            if (tf.RotShortestDistanceX)
            {
                float distX = AngleDegDistance(tf.degX, tfNext.degX);
                degX += (tf.degX + distX * l);
            }
            else
            {
                degX += (tf.degX * (1 - l) + tfNext.degX * l) * weight;
            }

            if (tf.RotShortestDistanceY)
            {
                float distY = AngleDegDistance(tf.degY, tfNext.degY);
                degY += (tf.degY + distY * l);
            }
            else
            {
                degY += (tf.degY * (1 - l) + tfNext.degY * l) * weight;
            }

            if (tf.RotShortestDistanceZ)
            {
                float distZ = AngleDegDistance(tf.degZ, tfNext.degZ);
                degZ += (tf.degZ + distZ * l);
            }
            else
            {
                degZ += (tf.degZ * (1 - l) + tfNext.degZ * l) * weight;
            }


            scaleX += ((tf.scaleX - 1) * (1 - l) + (tfNext.scaleX - 1) * l) * weight;
            scaleY += ((tf.scaleY - 1) * (1 - l) + (tfNext.scaleY - 1) * l) * weight;
            scaleZ += ((tf.scaleZ - 1) * (1 - l) + (tfNext.scaleZ - 1) * l) * weight;
            translateX += (tf.translateX * (1 - l) + tfNext.translateX * l) * weight;
            translateY += (tf.translateY * (1 - l) + tfNext.translateY * l) * weight;
            translateZ += (tf.translateZ * (1 - l) + tfNext.translateZ * l) * weight;
        }

        internal void SetMat(Matrix4x4 modelMatrix)
        {
            AnimModelMatrix = modelMatrix * Matrix4x4.identity;
        }

        public override string ToString()
        {
            return string.Format("translate: {0}/{1}/{2}, rotate: {3}/{4}/{5}, scale: {6}/{7}/{8}", translateX, translateY, translateZ, degX, degY, degZ, scaleX, scaleY, scaleZ);
        }
    }
}