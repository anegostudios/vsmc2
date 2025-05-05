using System.Collections.Generic;
using UnityEngine;

public class VSAnimationJoint
{
    /// <summary>
    /// The ID of the joint.
    /// </summary>
    public int JointId;

    /// <summary>
    /// The attached ShapeElement.
    /// </summary>
    public ShapeElementJSON Element;

    /// <summary>
    /// Takes the transform and inverses it.
    /// </summary>
    /// <param name="frameModelTransform"></param>
    /// <returns></returns>
    public float[] ApplyInverseTransform(float[] frameModelTransform)
    {
        List<ShapeElementJSON> elems = Element.GetParentPath();

        float[] modelTransform = Mat4f.Create();

        for (int i = 0; i < elems.Count; i++)
        {
            ShapeElementJSON elem = elems[i];
            float[] localTransform = elem.GetLocalTransformMatrix(0);
            Mat4f.Mul(modelTransform, modelTransform, localTransform);
        }

        Mat4f.Mul(modelTransform, modelTransform, Element.GetLocalTransformMatrix(0));

        float[] inverseTransformMatrix = Mat4f.Invert(Mat4f.Create(), modelTransform);

        return Mat4f.Mul(frameModelTransform, frameModelTransform, inverseTransformMatrix);
    }
}
