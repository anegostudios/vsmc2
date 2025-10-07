using System.Collections.Generic;
using System;
using UnityEngine;
using Newtonsoft.Json;

namespace VSMC
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ShapeElement
    {
        //These properties should be saved/loaded in JSON files.
        #region JSON Properties
        /// <summary>
        /// The name of the ShapeElement
        /// </summary>
        [JsonProperty]
        public string Name;

        [JsonProperty]
        public double[] From;
        [JsonProperty]
        public double[] To;

        /// <summary>
        /// Whether or not the shape element is shaded.
        /// </summary>
        [JsonProperty]
        public bool Shade = true;

        [JsonProperty]
        public bool GradientShade = false;

        /// <summary>
        /// The faces of the shape element by name (will normally be null except during object deserialization: use FacesResolved instead!)
        /// Marked as private to stop accidental usage.
        /// </summary>
        [JsonProperty]
        private Dictionary<string, ShapeElementFace> Faces;

        /// <summary>
        /// The origin point for rotation.0
        /// </summary>
        [JsonProperty]
        public double[] RotationOrigin = new double[3];

        /// <summary>
        /// The forward vertical rotation of the shape element.
        /// </summary>
        [JsonProperty]
        public double RotationX;

        /// <summary>
        /// The forward vertical rotation of the shape element.
        /// </summary>
        [JsonProperty]
        public double RotationY;

        /// <summary>
        /// The left/right tilt of the shape element
        /// </summary>
        [JsonProperty]
        public double RotationZ;

        /// <summary>
        /// How far away are the left/right sides of the shape from the center
        /// </summary>
        [JsonProperty]
        public double ScaleX = 1;

        /// <summary>
        /// How far away are the top/bottom sides of the shape from the center
        /// </summary>
        [JsonProperty]
        public double ScaleY = 1;

        /// <summary>
        /// How far away are the front/back sides of the shape from the center.
        /// </summary>
        [JsonProperty]
        public double ScaleZ = 1;

        [JsonProperty]
        public string ClimateColorMap = null;
        [JsonProperty]
        public string SeasonColorMap = null;
        [JsonProperty]
        public short RenderPass = -1;
        [JsonProperty]
        public short ZOffset = 0;

        /// <summary>
        /// Set this to true to disable randomDrawOffset and randomRotations on this specific element (e.g. used for the ice element of Coopers Reeds in Ice)
        /// </summary>
        [JsonProperty]
        public bool DisableRandomDrawOffset;

        /// <summary>
        /// The child shapes of this shape element
        /// </summary>
        [JsonProperty]
        public ShapeElement[] Children;

        /// <summary>
        /// The attachment points for this shape.
        /// </summary>
        [JsonProperty]
        public AttachmentPoint[] AttachmentPoints;

        /// <summary>
        /// The "remote" parent for this element
        /// </summary>
        [JsonProperty]
        public string StepParentName;

        /// <summary>
        /// Is this object shown in the editor?
        /// </summary>
        [JsonProperty]
        public bool renderInEditor = true;
        #endregion

        //These properties will be set at runtime, and do not need to be saved to a JSON file.
        #region Runtime Properties
        /// <summary>
        /// The unique ID for this element.
        /// </summary>
        public int elementUID = -1;
        
        /// <summary>
        /// The stored mesh data for this element.
        /// </summary>
        public MeshData meshData;

        /// <summary>
        /// The game object for this element.
        /// </summary>
        public ShapeElementGameObject gameObject;

        /// <summary>
        /// An array holding the faces of this shape element in BlockFacing order: North, East, South, West, Up, Down.  May be null if not present or not enabled.
        /// </summary>
        public ShapeElementFace[] FacesResolved = new ShapeElementFace[6];

        /// <summary>
        /// The parent element reference for this shape.
        /// </summary>
        public ShapeElement ParentElement;

        /// <summary>
        /// The id of the joint attached to the parent element.
        /// </summary>
        public int JointId;

        /// <summary>
        /// The cached inverse model transform for the element.
        /// </summary>
        public Matrix4x4 inverseModelTransform = Matrix4x4.zero;

        /// <summary>
        /// Should this object be minimized in the element hierarchy?
        /// </summary>
        public bool minimizeFromThisObject = false;
        #endregion

        //These functions will interact with the ShapeAccessors and Tesellators to regenerate a shape game object.
        #region Runtime Editing Functions

        [JsonConstructor]
        public ShapeElement(bool noparams = true)
        {}

        /// <summary>
        /// Create a completely new element. This will register it with a UID, but you will need to use SetParent to set an appropriate parent.
        /// </summary>
        public ShapeElement()
        {
            From = new double[] { 0, 0, 0 };
            To = new double[] { 1, 1, 1 };
            Name = "New Object";
            int nameCheckCount = 1;
            while (ShapeElementRegistry.main.GetShapeElementByName(Name) != null)
            {
                Name = "New Object (" + ++nameCheckCount+ ")";
            }
            ResolveReferencesAndUIDs();
            Faces = new Dictionary<string, ShapeElementFace>();
            for (int i = 0; i < 6; i++)
            {
                Faces.Add(FaceNames[i], new ShapeElementFace()
                {
                    Enabled = true,
                    Uv = new float[] { 0, 0, 1, 1 },
                    Texture = "#texture"
                });
            }
        }

        /// <summary>
        /// Sets the parent for this element, and also adds this element to the parents child array.
        /// You will likely need to recalculate transforms after doing this.
        /// </summary>
        /// <param name="parent"></param>
        public void SetParent(ShapeElement parent)
        {
            RemoveParent();
            this.ParentElement = parent;
            if (parent == null) return;

            //Children array cannot be added to easily, so we essentially clone it with this at the end.
            //parent.Children?.Append(this) had a panic attack when I did it, so it's a more primitive method.
            ShapeElement[] newChildren = new ShapeElement[parent.Children == null ? 1 : parent.Children.Length + 1];
            parent.Children?.CopyTo(newChildren, 0);
            newChildren[newChildren.Length - 1] = this;
            parent.Children = newChildren;
        }

        /// <summary>
        /// Removes this object's parents, and removes it from its parent array.
        /// </summary>
        public void RemoveParent()
        {
            if (ParentElement == null) return;
            ParentElement.Children = ParentElement.Children.Remove(this);
            ParentElement = null;
        }

        /// <summary>
        /// Recreates and applies the transforms for this shape element and its children.
        /// </summary>
        public void RecreateTransforms()
        {
            //Retesselate the shapes, and then reapply the transforms for the object.
            ShapeTesselator.ResolveMatricesForShapeElementAndChildren(this);
            gameObject.ReapplyTransformsFromMeshData(true);
        }

        /// <summary>
        /// Recreates and applies the objects mesh. Has no effect on children.
        /// </summary>
        public void RecreateObjectMesh()
        {
            //Retesselate the shapes, and then reapply the meshes.
            ShapeTesselator.RecreateMeshesForShapeElement(this);
            gameObject.RegenerateMeshFromMeshData();
        }

        /// <summary>
        /// Recreates and applies both the transforms and object mesh. Will also alter children's transforms.
        /// </summary>
        public void RecreateObjectMeshAndTransforms()
        {
            ShapeTesselator.RecreateMeshesForShapeElement(this);
            ShapeTesselator.ResolveMatricesForShapeElementAndChildren(this);
            gameObject.RegenerateMeshFromMeshData();
            gameObject.ReapplyTransformsFromMeshData(true);
        }
        #endregion

        /// <summary>
        /// Walks the element tree and collects all parents, starting with the root element
        /// </summary>
        /// <returns></returns>
        public List<ShapeElement> GetParentPath()
        {
            List<ShapeElement> path = new List<ShapeElement>();
            ShapeElement parentElem = this.ParentElement;
            while (parentElem != null)
            {
                path.Add(parentElem);
                parentElem = parentElem.ParentElement;
            }
            path.Reverse();
            return path;
        }

        public List<string> GetNamesOfThisAndAllChildren()
        {
            List<string> names = new List<string>();
            AddNamesOfThisAndChildren(names);
            return names;
        }

        void AddNamesOfThisAndChildren(List<string> names)
        {
            names.Add(Name);
            if (Children != null)
            {
                foreach (ShapeElement child in Children)
                {
                    child.AddNamesOfThisAndChildren(names);
                }
            }
        }

        public List<ShapeElement> GetThisAndAllChildrenRecursively()
        {
            List<ShapeElement> allElements = new List<ShapeElement>();
            AddThisAndChildrenToList(allElements);
            return allElements;
        }

        void AddThisAndChildrenToList(List<ShapeElement> allElements)
        {
            allElements.Add(this);
            if (Children != null)
            {
                foreach (ShapeElement child in Children)
                {
                    child.AddThisAndChildrenToList(allElements);
                }
            }
        }

        /// <summary>
        /// Should this object be hidden in the editor? Will return yes if this or any parents are set to hide.
        /// </summary>
        /// <returns></returns>
        public bool ShouldHideFromView()
        {
            if (!renderInEditor) return true;
            if (ParentElement == null) return false;
            return ParentElement.ShouldHideFromView();
        }

        /// <summary>
        /// Should this object be completely hidden in the hierarchy? Will return yes only if a parent is set to hide.
        /// </summary>
        public bool ShouldMinimizeInUI()
        {
            if (ParentElement == null) return false;
            if (ParentElement.minimizeFromThisObject) return true;
            return ParentElement.ShouldMinimizeInUI();
        }

        public void RecalculateHiddenStatus()
        {
            //There is a more efficient way of doing this by caching the stored hidden status, but this is more robust and easier to code.
            //After testing - It's actually pretty quick.
            if (ShouldHideFromView() && gameObject != null)
            {
                gameObject.gameObject.SetActive(false);
            }
            else if (gameObject != null)
            {
                gameObject.gameObject.SetActive(true);
            }
            if (Children != null)
            {
                foreach (ShapeElement child in Children)
                {
                    child.RecalculateHiddenStatus();
                }
            }
        }

        /// <summary>
        /// Counts the depth of parents this object has.
        /// </summary>
        public int CountParents()
        {
            int count = 0;
            ShapeElement parentElem = this.ParentElement;
            while (parentElem != null)
            {
                count++;
                parentElem = parentElem.ParentElement;
            }
            return count;
        }

        public void CacheInverseTransformMatrix()
        {
            if (inverseModelTransform == Matrix4x4.zero)
            {
                inverseModelTransform = GetInverseModelMatrix();
            }
        }

        /// <summary>
        /// Returns the full inverse model matrix (includes all parent transforms).
        /// Converted from source to use Unity matrix.
        /// </summary>
        /// <returns></returns>
        public Matrix4x4 GetInverseModelMatrix()
        {
            List<ShapeElement> elems = GetParentPath();

            Matrix4x4 modelTransform = Matrix4x4.identity;

            for (int i = 0; i < elems.Count; i++)
            {
                ShapeElement elem = elems[i];
                Matrix4x4 localTransform = elem.GetLocalTransformMatrix(0);
                modelTransform = modelTransform * localTransform;
            }

            modelTransform = modelTransform * GetLocalTransformMatrix(0);

            return modelTransform.inverse;
        }

        internal void ResolveReferencesAndUIDs()
        {
            //Debug.Log("Resolving references...");
            elementUID = ShapeElementRegistry.main.AddShapeElement(this);
            var Children = this.Children;
            if (Children != null)
            {
                for (int i = 0; i < Children.Length; i++)
                {
                    ShapeElement child = Children[i];
                    child.ParentElement = this;
                    child.ResolveReferencesAndUIDs();
                }
            }

            /*
            var AttachmentPoints = this.AttachmentPoints;
            if (AttachmentPoints != null)
            {
                for (int i = 0; i < AttachmentPoints.Length; i++)
                {
                    AttachmentPoints[i].ParentElement = this;
                }
            }
            */
        }


        static ElementPose noTransform = new ElementPose();


        /// <summary>
        /// Sets up a local transform matrix for the shape element. Mostly copied from game code.
        /// Uses animVersion 0, and using Matrix4x4 and Unity vectors.
        /// </summary>
        public Matrix4x4 GetLocalTransformMatrix(int animVersion = 1, Matrix4x4? output = null, ElementPose tf = null)
        {
            if (tf == null) tf = noTransform;
            if (output == null) output = Matrix4x4.identity;

            ShapeElement elem = this;
            Vector3 origin = new Vector3();

            //Setup rotation origin.
            if (elem.RotationOrigin != null)
            {
                origin.x = (float)elem.RotationOrigin[0] / 16;
                origin.y = (float)elem.RotationOrigin[1] / 16;
                origin.z = (float)elem.RotationOrigin[2] / 16;
            }

            // R = rotate, S = scale, T = translate
            // Version 0: R * S * T
            // Version 1: T * S * R
            
            //Force anim version of 0.


            if (animVersion != -1)
            {
                output *= Matrix4x4.Translate(origin);

                //Vector3 rotation = new Vector3();
                if (elem.RotationX + tf.degX + tf.degOffX != 0)
                {
                    output *= Matrix4x4.Rotate(Quaternion.Euler(new Vector3((float)(elem.RotationX + tf.degX + tf.degOffX), 0, 0)));
                }
                if (elem.RotationY + tf.degY + tf.degOffY != 0)
                {
                    output *= Matrix4x4.Rotate(Quaternion.Euler(new Vector3(0, (float)(elem.RotationY + tf.degY + tf.degOffY), 0)));
                }
                if (elem.RotationZ + tf.degZ + tf.degOffZ != 0)
                {
                    output *= Matrix4x4.Rotate(Quaternion.Euler(new Vector3(0, 0, (float)(elem.RotationZ + tf.degZ + tf.degOffZ))));
                }

                //Rotation. May be source of problems, as base game splits this to X, then Y, then Z. Should be fine though.
                //Turns out it is absolutely not fine. Do them individually.
                //output *= Matrix4x4.Rotate(Quaternion.Euler(rotation.x, rotation.y, rotation.z));

                //Scale...
                output *= Matrix4x4.Scale(new Vector3((float)elem.ScaleX * tf.scaleX, (float)elem.ScaleY * tf.scaleY, (float)elem.ScaleZ * tf.scaleZ));

                //Translation.
                output *= Matrix4x4.Translate(new Vector3(
                    (float)elem.From[0] / 16 + tf.translateX,
                    (float)elem.From[1] / 16 + tf.translateY,
                    (float)elem.From[2] / 16 + tf.translateZ
                ));

                output *= Matrix4x4.Translate(-origin);
            }
            return output.Value;
        }


        public void SetJointIdRecursive(int jointId)
        {
            this.JointId = jointId;
            var Children = this.Children;
            if (Children != null)
            {
                for (int i = 0; i < Children.Length; i++)
                {
                    Children[i].SetJointIdRecursive(jointId);
                }
            }
        }

        public void CacheInverseTransformMatrixRecursive()
        {
            CacheInverseTransformMatrix();

            var Children = this.Children;
            if (Children != null)
            {
                for (int i = 0; i < Children.Length; i++)
                {
                    Children[i].CacheInverseTransformMatrixRecursive();
                }
            }
        } 


        /// <summary>
        /// Resolves the face indices and the texture codes for the element and its children.
        /// </summary>
        public void ResolveFacesAndTextures(List<LoadedTexture> textures)
        {

            FacesResolved = new ShapeElementFace[6];
            if (Faces != null)
            {
                foreach (var val in Faces)
                {
                    ShapeElementFace f = val.Value;
                    f.ResolveTexture(textures);
                    if (!f.Enabled) continue;
                    BlockFacing facing = BlockFacing.FromFirstLetter(val.Key);
                    FacesResolved[facing.index] = f;
                }
            }

            //Check all faces exist.
            for (int i = 0; i < 6; i++)
            {
                if (FacesResolved[i] == null)
                {
                    FacesResolved[i] = new ShapeElementFace();
                    FacesResolved[i].Enabled = false;
                }
            }

            if (Children != null)
            {
                foreach (ShapeElement child in Children)
                {
                    child.ResolveFacesAndTextures(textures);
                }
            }
        }

        /// <summary>
        /// Names for each face - Matched to <see cref="FaceEnum"/>.
        /// </summary>
        static string[] FaceNames = new string[] { "north", "east", "south", "west", "up", "down" };

        public void ResolveBeforeSerialization()
        {
            //Resolve each independent face.
            if (FacesResolved != null)
            {
                foreach (ShapeElementFace face in FacesResolved)
                {
                    face.ResolveBeforeSerialization();
                }
            }

            //Now resolve the actual face array. This is based on face names.
            if (FacesResolved != null)
            {
                Faces = new Dictionary<string, ShapeElementFace>(6);
                for (int i = 0; i < 6; i++)
                {
                    if (FacesResolved[i].Enabled)
                    {
                        Faces[FaceNames[i]] = FacesResolved[i];
                    }
                }
            }

            //Now resolve child elements.
            if (Children != null)
            {
                foreach (ShapeElement child in Children)
                {
                    child.ResolveBeforeSerialization();
                }
            }
        }

        /// <summary>
        /// A specific function to rotate a vector only by this objects rotation values.
        /// Used when working with rotation origins.
        /// </summary>
        public Vector3 RotateFromWorldToLocalForThisObjectsRotation(Vector3 world)
        {
            return Quaternion.Euler((float)RotationX, (float)RotationY, (float)RotationZ) * world;
        }

        /// <summary>
        /// Multiplies a vector by the inverse of the *parents* final rotation. Useful for getting the rotation that is truly local to this object.
        /// </summary>
        public Vector3 RotateFromWorldToLocalBasedOnParentRotation(Vector3 world)
        {
            if (ParentElement != null)
            {
                return ParentElement.meshData.storedMatrix.inverse.rotation * world;
            }
            return world;
        }

        public Vector3 RotateFromLocalToWorldForThisObjectsRotation(Vector3 local)
        {
            return Quaternion.Inverse(Quaternion.Euler((float)RotationX, (float)RotationY, (float)RotationZ)) * local;
        }
    }
}