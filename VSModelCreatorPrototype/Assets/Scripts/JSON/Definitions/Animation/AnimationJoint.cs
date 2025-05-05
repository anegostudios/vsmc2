using System.Collections.Generic;
using UnityEngine;

namespace VSMC
{
    public class AnimationJoint
    {
        
        /// <summary>
        /// The ID of the joint.
        /// </summary>
        public int JointId;

        /// <summary>
        /// The attached ShapeElement.
        /// </summary>
        public ShapeElement Element;

        /// <summary>
        /// Takes the transform and inverses it.
        /// </summary>
        /// <param name="frameModelTransform"></param>
        /// <returns></returns>
        public Matrix4x4 ApplyInverseTransform(Matrix4x4 frameModelTransform)
        {
            List<ShapeElement> elems = Element.GetParentPath();

            Matrix4x4 modelTransform = Matrix4x4.identity;

            for (int i = 0; i < elems.Count; i++)
            {
                ShapeElement elem = elems[i];
                Matrix4x4 localTransform = elem.GetLocalTransformMatrix(0);
                modelTransform = modelTransform * localTransform;
            }

            modelTransform = modelTransform * Element.GetLocalTransformMatrix(0);
            Matrix4x4 inverseTransformMatrix = modelTransform.inverse;

            return frameModelTransform * inverseTransformMatrix;
        }

        
    }
}