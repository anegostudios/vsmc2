using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace VSMC
{
    public class AnimationRootShapeElementController
    {

        public ClientAnimator animator;
        public IShapeHolder shapeHolder;
        public ShapeElement rootElement;

        /// <summary>
        /// Every animation by code.
        /// </summary>
        public Dictionary<string, AnimationMetaData> allAnimations;

        /// <summary>
        /// Creates a dictionary containing the currently selected animation's metadata.
        /// </summary>
        public Dictionary<string, AnimationMetaData> activeAnimations
        {
            get
            {
                if (cSelectedAnimation == null) return new Dictionary<string, AnimationMetaData>();
                return new Dictionary<string, AnimationMetaData>(allAnimations.Where(x => x.Key == cSelectedAnimation.Code));
            }
        }

        /// <summary>
        /// The current selected animation to play.
        /// </summary>
        public Animation cSelectedAnimation;

        public List<int> jointIDsForController = new List<int>();

        public AnimationRootShapeElementController(IShapeHolder shapeHolder, ShapeElement rootElement)
        {
            this.shapeHolder = shapeHolder;
            this.rootElement = rootElement;
        }


        #region Playback Functions

        /// <summary>
        /// Progresses the animation by the given time.
        /// Returns false if the animation cancelled or did not play for any reason.
        /// </summary>
        public bool ProgressAnimation(float dt)
        {
            if (animator == null || animator.anims == null || cSelectedAnimation == null)
            {
                IdentityAllJoints();
                return false;
            }
            RunningAnimation anim = animator.GetAnimationState(cSelectedAnimation.Code);
            if (cSelectedAnimation.KeyFrames.Length == 0)
            {
                IdentityAllJoints();
                return false;
            }
            animator.OnFrame(activeAnimations, dt);

            //If the current animation has stopped, force the playback to stop too.
            //Unsure how to handle this for non-main animations.
            if (!anim.Active || !anim.Running)
            {
                return false;
            }
            SetAllJointsToAnimatorMatrices();
            return true;
        }
    
        /// <summary>
        /// Sets the frame of the current animation.
        /// If the frame cannot be set for any reason, this returns false.
        /// </summary>
        public bool SetFrame(int frame, bool forceRefresh = false)
        {
            if (animator == null || animator.anims == null || cSelectedAnimation == null)
            {
                IdentityAllJoints();
                return false;
            }
            RunningAnimation anim = animator.GetAnimationState(cSelectedAnimation.Code);

            //No need to update anything if the current frame is the same.
            if (anim.CurrentFrame == frame && !forceRefresh) return true;
            anim.CurrentFrame = frame;
            anim.Iterations = 0;
            return true;
        }

        public void ResetCurrentAnimationFrame()
        {
            if (animator == null || animator.anims == null || cSelectedAnimation == null)
            {
                IdentityAllJoints();
                return;
            }
            RunningAnimation anim = animator.GetAnimationState(cSelectedAnimation.Code);
            if (anim == null) return;
            anim.Iterations = 0;
            anim.CurrentFrame = (int)anim.CurrentFrame;
        }

        public void SetSelectedAnimation(string code)
        {
            animator.ResetAllAnimationsForVSMC();
            code = GetMostRelevantAnimationCode(code);
            Debug.Log("Setting animation for " + rootElement.GetElementCodeInShape().ToString() + " to " + code);
            cSelectedAnimation = shapeHolder.GetShape().Animations.FirstOrDefault(x => x.Code.Equals(code));
            ProgressAnimation(1);
            SetFrame(0, true);
        }

        public string GetMostRelevantAnimationCode(string code)
        {
            //Find the correct code; if there is a bypass.
            ShapeElement rootElem = this.rootElement;
            while (rootElem != null)
            {
                LoadedAdditionalShape las = MultiShapeManager.main.GetLoadedAdditionalShapeFromRootElementCode(rootElem.GetElementCodeInShape());
                if (las == null) return code;
                if (las != null && las.data.animationPlaybackMapping.ContainsKey(code))
                {
                    return las.data.animationPlaybackMapping[code];
                }
                if (rootElem.OriginPoint == null || !rootElem.OriginPoint.DependsOnOtherShapeElement())
                {
                    rootElem = null;
                }
                else
                {
                    rootElem = rootElem.OriginPoint.GetShapeElementThisDependsOn().GetRootElement();
                }
            }
            return code;
        }

        public void DeselectAnimation()
        {
            animator.ResetAllAnimationsForVSMC();
            cSelectedAnimation = null; 
            ProgressAnimation(1);
            SetFrame(0, true);
        }

        #endregion

        #region Setup

        public void RecreateAnimationJointsAndSetup(bool resetJoints, bool alreadyInitialized)
        {
            Shape s = shapeHolder.GetShape();

            if (!alreadyInitialized)
            {
                s.InitForAnimations(rootElement.parentShape.shapeLoadedFromPath);
                if (s.Animations == null) s.Animations = new Animation[0];
            }
         
            animator = ClientAnimator.Create(s.Animations, s.Elements, s.JointsById);
            //Load the animations themselves.
            allAnimations = new Dictionary<string, AnimationMetaData>();
            foreach (var anim in s.Animations)
            {
                AnimationMetaData meta = new AnimationMetaData(anim.Name, anim.Code);
                allAnimations.Add(anim.Code, meta);
                anim.PrevNextKeyFrameByFrame = null;
            }

            if (!alreadyInitialized)
            {
                shapeHolder.ReparentGameObjectsByJoints();
            }

            FindRelevantJointsInRootElement(rootElement);
        }

        private void FindRelevantJointsInRootElement(ShapeElement rootElement)
        {
            jointIDsForController = new List<int>();
            List<ShapeElement> temp = new List<ShapeElement>();
            temp.Add(rootElement);
            while (temp.Count > 0)
            {
                ShapeElement e = temp[0];
                temp.RemoveAt(0);
                if (!jointIDsForController.Contains(e.JointId))
                {
                    jointIDsForController.Add(e.JointId);
                }
                if (e.Children != null)
                {
                    temp.AddRange(e.Children);
                }
            }
        }

        public void ResetAllAnimations()
        {
            animator.ResetAllAnimationsForVSMC();
            ProgressAnimation(0);
        }

        #endregion

        #region Transform Utilities
        /// <summary>
        /// Gets the origin point for this animation shape. This will change depending on what it is currently attached to.
        /// </summary>
        /// <returns></returns>
        public Matrix4x4 GetOriginMatrix()
        {
            if (rootElement.OriginPoint != null)
            {
                return rootElement.OriginPoint.GetAnimationOriginMatrix();
            }
            return Matrix4x4.identity;
        }

        public Transform GetJoint(int i)
        {
            return shapeHolder.jointParentsForRootElements[rootElement][i];
        }

        public void ZeroAllJoints()
        {
            foreach (int i in jointIDsForController)
            {
                Transform joint = GetJoint(i);
                joint.localPosition = Vector3.zero;
                joint.localRotation = Quaternion.identity;
                joint.localScale = Vector3.one;
            }
        }

        public void IdentityAllJoints()
        {
            foreach (int i in jointIDsForController)
            {
                Transform joint = GetJoint(i);
                //Flip animation matrices too.
                Matrix4x4 flipZ = Matrix4x4.Scale(new Vector3(1f, 1f, -1f));
                Matrix4x4 m = flipZ * GetOriginMatrix() * flipZ;

                joint.localPosition = m.GetPosition();
                joint.localRotation = m.rotation;
                joint.localScale = m.lossyScale;
            }
        }

        public void SetAllJointsToAnimatorMatrices()
        {
            foreach (int i in jointIDsForController)
            {
                Transform joint = GetJoint(i);

                //Flip animation matrices too.
                Matrix4x4 flipZ = Matrix4x4.Scale(new Vector3(1f, 1f, -1f));
                Matrix4x4 m = flipZ * (GetOriginMatrix() * animator.Matrices[i]) * flipZ;

                joint.localPosition = m.GetPosition();
                joint.localRotation = m.rotation;
                joint.localScale = m.lossyScale;
            }
        }

        #endregion


    }
}