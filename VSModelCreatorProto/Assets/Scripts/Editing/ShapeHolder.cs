using System.Collections.Generic;
using UnityEngine;

namespace VSMC
{
    /// <summary>
    /// Stores the currently loaded shape and its gameobject references.
    /// </summary>
    public class ShapeHolder : IShapeHolder
    {
        public static Shape CurrentLoadedShape;

        [Tooltip("Limbo for shape elements.")]
        public Transform deletedElementParent;

        public Material customMaterial;

        /// <summary>
        /// Load a shape and creates its model in the noJoint parent.
        /// </summary>
        /// <param name="loadedShape"></param>
        public void OnShapeLoaded(Shape loadedShape, bool alreadyResolved = false)
        {
            if (loadedShape.isBackdropOrAttachmentShape)
            {
                loadedShape.ResolveReferencesAndUIDs(true);
                CreateAllShapeElementGameObjects(loadedShape);
                return;
            }
            
            if (CurrentLoadedShape != null)
            {
                Debug.LogError("Trying to load a shape when one is already active. Remember to call UnloadCurrentShape first!");
            }
            if (!alreadyResolved)
            {
                loadedShape.ResolveReferencesAndUIDs(false);
            }
            CurrentLoadedShape = loadedShape;
            CreateAllShapeElementGameObjects(loadedShape);
        }

        /// <summary>
        /// Creates all the game objects for an entire shape.
        /// </summary>
        public void CreateAllShapeElementGameObjects(Shape shape, bool tesselateFirst = true)
        {
            if (tesselateFirst)
            {
                ShapeTesselator.TesselateShapeElements(shape.Elements);
            }

            foreach (ShapeElement elem in shape.Elements)
            {
                ShapeTesselator.ResolveAllLocalMatricesForRootShapeElement(elem);
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
            ObjectSelector.main.DeselectAll();
            foreach (Transform child in noJointParent.transform)
            {
                Destroy(child.gameObject);
            }
            foreach (List<Transform> joints in jointParentsForRootElements.Values)
            {
                foreach (Transform joint in joints)
                {
                    foreach (Transform child in joint.transform)
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
            CurrentLoadedShape = null;
        }

        public override void ReparentGameObjectsByJoints()
        {
            jointParentsForRootElements = new Dictionary<ShapeElement, List<Transform>>();
            foreach (ShapeElement e in GetShape().Elements)
            {
                ReparentRootGameObjectsByJoints(e);
            }
        }

        public void ReparentRootGameObjectsByJoints(ShapeElement root)
        {
            List<ShapeElement> elements = new List<ShapeElement>();
            elements.Add(root);
            jointParentsForRootElements.Add(root, new List<Transform>());
            while (elements.Count > 0)
            {
                ShapeElement cElem = elements[0];
                elements.RemoveAt(0);
                if (cElem.Children != null) elements.AddRange(cElem.Children);

                //Create joint IDs if needed.
                while (jointParentsForRootElements[root].Count <= cElem.JointId + 1)
                {
                    GameObject newJoint = new GameObject("Joint " + jointParentsForRootElements[root].Count);
                    newJoint.transform.parent = jointParentParent;
                    jointParentsForRootElements[root].Add(newJoint.transform);
                }
                cElem.gameObject.transform.SetParent(jointParentsForRootElements[root][cElem.JointId], false);
            }
        }

        public override void ReparentGameObjectsToNoJoints()
        {
            List<ShapeElement> elements = new List<ShapeElement>();
            elements.AddRange(CurrentLoadedShape.Elements);
            while (elements.Count > 0)
            {
                ShapeElement cElem = elements[0];
                elements.RemoveAt(0);
                if (cElem.Children != null) elements.AddRange(cElem.Children);

                cElem.gameObject.transform.SetParent(noJointParent, false);
            }
        }

        /// <summary>
        /// Sends an object to a semi-deleted state parent. Will also deselect the object.
        /// </summary>
        /// <param name="elem"></param>
        public void SendElementToDeletionLimbo(ShapeElement elem, bool doChildren = false)
        {
            //When an object is deleted, we definitely don't want it to remain selected.
            ObjectSelector.main.DeselectObject(elem.gameObject.gameObject, false);
            
            elem.gameObject.transform.SetParent(deletedElementParent, true);

            if (doChildren && elem.Children != null)
            {
                foreach (ShapeElement child in  elem.Children)
                {
                    SendElementToDeletionLimbo(child, true);
                }
            }
        }

        /// <summary>
        /// Restores an object that's in the semi-deleted state parent.
        /// </summary>
        /// <param name="elem"></param>
        public void RestoreElementFromDeletionLimbo(ShapeElement elem, bool doChildren = false)
        {
            elem.gameObject.transform.SetParent(noJointParent, true);
            if (doChildren && elem.Children != null)
            {
                foreach (ShapeElement child in elem.Children)
                {
                    RestoreElementFromDeletionLimbo(child, true);
                }
            }
        }

        public override Shape GetShape()
        {
            return CurrentLoadedShape;
        }
    }
}