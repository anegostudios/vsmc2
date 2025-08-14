using System.Collections.Generic;
using UnityEngine;

namespace VSMC
{
    /// <summary>
    /// Stores the currently loaded shape and its gameobject references.
    /// </summary>
    public class ShapeHolder : MonoBehaviour
    {
        public Shape cLoadedShape;

        [Header("Unity References")]
        public GameObject shapeElementPrefab;
        public Transform noJointParent;
        public List<Transform> jointParents;

        [Tooltip("The parent of the joint parents.")]
        public Transform jointParentParent;

        [Tooltip("Limbo for shape elements.")]
        public Transform deletedElementParent;

        /// <summary>
        /// Load a shape and creates its model in the noJoint parent.
        /// </summary>
        /// <param name="loadedShape"></param>
        public void OnShapeLoaded(Shape loadedShape)
        {
            if (cLoadedShape != null)
            {
                Debug.LogError("Trying to load a shape when one is already active. Remember to call UnloadCurrentShape first!");
            }
            cLoadedShape = loadedShape;
            CreateAllShapeElementGameObjects(cLoadedShape);
        }

        /// <summary>
        /// Creates all the game objects for an entire shape.
        /// </summary>
        public void CreateAllShapeElementGameObjects(Shape shape, bool tesselateFirst = true)
        {
            if (tesselateFirst) 
            {
                ShapeTesselator.TesselateShape(shape); 
            }

            foreach (ShapeElement elem in shape.Elements)
            {
                CreateShapeElementGameObject(elem);
            }
        }

        /// <summary>
        /// Creates a gameobject for a specific element (and its children, optional).
        /// </summary>
        public GameObject CreateShapeElementGameObject(ShapeElement elem, bool createChildren = true)
        {
            GameObject ch = GameObject.Instantiate(shapeElementPrefab, noJointParent);
            ch.GetComponent<ShapeElementGameObject>().InitializeElement(elem);
            if (createChildren && elem.Children != null)
            {
                foreach (ShapeElement child in elem.Children)
                {
                    CreateShapeElementGameObject(child);
                }
            }
            return ch;
        }

        public void UnloadCurrentShape()
        {
            foreach (Transform child in noJointParent.transform)
            {
                Destroy(child.gameObject);
            }
            foreach (Transform joint in jointParents)
            {
                foreach (Transform child in joint.transform)
                {
                    Destroy(child.gameObject);
                }
            }
            cLoadedShape = null;
        }

        public void ReparentGameObjectsByJoints()
        {
            List<ShapeElement> elements = new List<ShapeElement>();
            elements.AddRange(cLoadedShape.Elements);
            while (elements.Count > 0)
            {
                ShapeElement cElem = elements[0];
                elements.RemoveAt(0);
                if (cElem.Children != null) elements.AddRange(cElem.Children);

                //Create joint IDs if needed.
                while (jointParents.Count <= cElem.JointId + 1)
                {
                    GameObject newJoint = new GameObject("Joint " + jointParents.Count);
                    newJoint.transform.parent = jointParentParent;
                    jointParents.Add(newJoint.transform);
                }
                cElem.gameObject.transform.parent = jointParents[cElem.JointId];
            }
        }

        /// <summary>
        /// Sends an object to a semi-deleted state parent. Will also deselect the object.
        /// </summary>
        /// <param name="elem"></param>
        public void SendElementToDeletionLimbo(ShapeElement elem)
        {
            //When an object is deleted, we definitely don't want it to remain selected.
            ObjectSelector.main.DeselectObject(elem.gameObject.gameObject, false);
            
            elem.gameObject.transform.SetParent(deletedElementParent, true);
        }

        /// <summary>
        /// Restores an object that's in the semi-deleted state parent.
        /// </summary>
        /// <param name="elem"></param>
        public void RestoreElementFromDeletionLimbo(ShapeElement elem)
        {
            elem.gameObject.transform.SetParent(noJointParent, true);
        }

    }
}