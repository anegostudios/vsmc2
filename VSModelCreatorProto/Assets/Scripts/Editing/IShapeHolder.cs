using System.Collections.Generic;
using UnityEngine;

namespace VSMC
{
    /// <summary>
    /// Base class for the various shape holder types.
    /// 
    /// </summary>
    public abstract class IShapeHolder : MonoBehaviour
    {

        [Header("Unity References")]
        public GameObject shapeElementPrefab;
        public Transform noJointParent;
        public Dictionary<ShapeElement, List<Transform>> jointParentsForRootElements = new Dictionary<ShapeElement, List<Transform>>();

        [Tooltip("The parent of the joint parents.")]
        public Transform jointParentParent;


        public abstract Shape GetShape();
        public abstract void ReparentGameObjectsToNoJoints();
        public abstract void ReparentGameObjectsByJoints();

    }
}