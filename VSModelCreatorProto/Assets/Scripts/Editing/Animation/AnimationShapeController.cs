using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace VSMC
{
    public class AnimationShapeController
    {

        public ClientAnimator animator;
        public IShapeHolder shapeHolder;

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

        public AnimationShapeController(IShapeHolder shapeHolder)
        {
            this.shapeHolder = shapeHolder;
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
            RunningAnimation anim = animator.GetAnimationState(cSelectedAnimation.Code);
            if (anim == null) return;
            anim.Iterations = 0;
            anim.CurrentFrame = (int)anim.CurrentFrame;
        }

        public void SetSelectedAnimation(string code)
        {
            animator.ResetAllAnimationsForVSMC();
            cSelectedAnimation = shapeHolder.GetShape().Animations.FirstOrDefault(x => x.Code.Equals(code));
            ProgressAnimation(1);
            SetFrame(0, true);
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

        public void RecreateAnimationJointsAndSetup(bool resetJoints)
        {
            Shape s = shapeHolder.GetShape();

            s.InitForAnimations("root");
            
            if (s.Animations == null) s.Animations = new Animation[0];
            animator = ClientAnimator.Create(s.Animations, s.Elements, s.JointsById);
            //Load the animations themselves.
            allAnimations = new Dictionary<string, AnimationMetaData>();
            foreach (var anim in s.Animations)
            {
                AnimationMetaData meta = new AnimationMetaData(anim.Name, anim.Code);
                allAnimations.Add(anim.Code, meta);
                anim.PrevNextKeyFrameByFrame = null;
            }

            shapeHolder.ReparentGameObjectsByJoints();

        }

        public void ResetAllAnimations()
        {
            animator.ResetAllAnimationsForVSMC();
        }

        #endregion

        #region Transform Utilities
        /// <summary>
        /// Gets the origin point for this animation shape. This will change depending on what it is currently attached to.
        /// </summary>
        /// <returns></returns>
        public Matrix4x4 GetOriginMatrix()
        {
            if (shapeHolder is AdditionalObjectHolder att)
            {
                ShapeElement step = null; //att.GetShape().Elements[0].StepParentElement;
                if (step != null)
                {
                    return AnimationEditorManager.main.animationShapeControllers[0].animator.Matrices[step.JointId] * step.meshData.storedMatrix;
                }
            }
            return Matrix4x4.identity;
        }

        public Transform GetJoint(int i)
        {
            return shapeHolder.jointParents[i];
        }

        public void ZeroAllJoints()
        {
            for (int i = 0; i < animator.MaxJointId; i++)
            {
                Transform joint = GetJoint(i);
                joint.localPosition = Vector3.zero;
                joint.localRotation = Quaternion.identity;
                joint.localScale = Vector3.one;
            }
        }

        public void IdentityAllJoints()
        {
            for (int i = 0; i < animator.MaxJointId; i++)
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
            for (int i = 0; i < animator.MaxJointId; i++)
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