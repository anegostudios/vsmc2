using System.Collections.Generic;
using System;
using UnityEngine;
using Newtonsoft.Json;

namespace VSMC
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ShapeElement
    {

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
        /// </summary>
        [JsonProperty]
        public Dictionary<string, ShapeElementFace> Faces;

        /// <summary>
        /// An array holding the faces of this shape element in BlockFacing order: North, East, South, West, Up, Down.  May be null if not present or not enabled.
        /// </summary>
        public ShapeElementFace[] FacesResolved = new ShapeElementFace[6];

        /// <summary>
        /// The origin point for rotation.
        /// </summary>
        [JsonProperty]
        public double[] RotationOrigin;

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
        /// The cached matrix for this elements transformation.
        /// </summary>
        public Matrix4x4 cachedMatrix;

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

        [Obsolete("Use SetJointIdRecursive instead.")]
        internal void SetJointId(int jointId)
        {
            SetJointIdRecursive(jointId);
        }

        internal void ResolveRefernces()
        {
            var Children = this.Children;
            if (Children != null)
            {
                for (int i = 0; i < Children.Length; i++)
                {
                    ShapeElement child = Children[i];
                    child.ParentElement = this;
                    child.ResolveRefernces();
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
        public void ResolveFacesAndTextures(Dictionary<string, string> textures)
        {
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
                if (Children != null)
                {
                    foreach (ShapeElement child in Children)
                    {
                        child.ResolveFacesAndTextures(textures);
                    }
                }
            }
        }
    }
}