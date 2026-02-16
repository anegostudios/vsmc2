using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VSMC
{
    /// <summary>
    /// This is the entry point to any animation features. 
    /// Most animation classes will interact with this in one way or another.
    /// </summary>
    public class AnimationEditorManager : MonoBehaviour
    {
        /// <summary>
        /// Static reference for the tasks to use.
        /// </summary>
        public static AnimationEditorManager main;

        [Header("Unity References")]
        public ShapeHolder shapeHolder;
        public AnimationEditorUIElements UI;

        bool isInAnimatorMode = false;

        /// <summary>
        /// Every animation by code.
        /// </summary>
        Dictionary<string, AnimationMetaData> allAnimations;

        /// <summary>
        /// Creates a dictionary containing the currently selected animation's metadata.
        /// </summary>
        Dictionary<string, AnimationMetaData> activeAnimations
        {
            get
            {
                if (cSelectedAnimation == null) return new Dictionary<string, AnimationMetaData>();
                return new Dictionary<string, AnimationMetaData>(allAnimations.Where(x => x.Key == cSelectedAnimation.Code)); 
            }
        }

        ClientAnimator animator;
        bool shouldPlayAnimations = false;

        public int cFrame;
        public Animation cSelectedAnimation;

        void Awake()
        {
            main = this;  
        }

        private void Start()
        {
            EditModeManager.RegisterForOnModeSelect(OnEditModeSelect);
            EditModeManager.RegisterForOnModeDeselect(OnEditModeDeselect);
            AnimationSelector.main.RegisterForOnAnimationSelected(OnAnimationSelected);
            AnimationSelector.main.RegisterForOnAnimationDeselected(OnAnimationDeselected);
            ObjectSelector.main.RegisterForObjectSelectedEvent(OnObjectSelected);
            ObjectSelector.main.RegisterForObjectDeselectedEvent(OnObjectDeselected);
        }

        void OnEditModeSelect(VSEditMode select)
        {
            if (select != VSEditMode.Animation) return;
            cSelectedAnimation = null;
            cFrame = 0;
            OnAnimationDataChanged();

            for (int i = 0; i < animator.MaxJointId; i++)
            {
                Transform joint = shapeHolder.jointParents[i];

                //Flip animation matrices too.
                Matrix4x4 flipZ = Matrix4x4.Scale(new Vector3(1f, 1f, -1f));
                Matrix4x4 m = flipZ * animator.Matrices[i] * flipZ;

                joint.localPosition = m.GetPosition();
                joint.localRotation = m.rotation;
                joint.localScale = m.lossyScale;
            }

            isInAnimatorMode = true;
        }

        void OnEditModeDeselect(VSEditMode deselect)
        {
            if (deselect != VSEditMode.Animation) return;
            
            //All joints need zero-ing.
            for (int i = 0; i < shapeHolder.jointParents.Count; i++)
            {
                Transform joint = shapeHolder.jointParents[i];
                joint.localPosition = Vector3.zero;
                joint.localRotation = Quaternion.identity;
                joint.localScale = Vector3.one;
            }

            isInAnimatorMode = false;
        }

        private void Update()
        { 
            if (animator == null || !isInAnimatorMode || !shouldPlayAnimations) return;
            ProgressAnimation(Time.deltaTime);
        }

        private void OnObjectSelected(GameObject selected)
        {
            if (animator == null || !isInAnimatorMode || cSelectedAnimation == null) return;
            UI.RefreshUIForFrameElementSelection(cFrame);
        }
        
        private void OnObjectDeselected(GameObject deselected)
        {
            if (animator == null || !isInAnimatorMode || cSelectedAnimation == null) return;
            UI.RefreshUIForFrameElementSelection(cFrame);
        }

        public void ProgressAnimation(float dt)
        {
            if (animator == null || animator.anims == null || cSelectedAnimation == null) return;

            RunningAnimation anim = animator.GetAnimationState(cSelectedAnimation.Code);
            if (cSelectedAnimation.KeyFrames.Length == 0) 
            {
                for (int i = 0; i < animator.MaxJointId; i++)
                {
                    Transform joint = shapeHolder.jointParents[i];

                    //Flip animation matrices too.
                    Matrix4x4 flipZ = Matrix4x4.Scale(new Vector3(1f, 1f, -1f));
                    Matrix4x4 m = flipZ * Matrix4x4.identity * flipZ;

                    joint.localPosition = m.GetPosition();
                    joint.localRotation = m.rotation;
                    joint.localScale = m.lossyScale;
                }

                cFrame = (int)anim.CurrentFrame;
                UI.RefreshUIForFrameElementSelection(cFrame);
                return;
            }

            animator.OnFrame(activeAnimations, dt);

            //If the current animation has stopped, force the playback to stop too.
            if (!anim.Active || !anim.Running)
            {
                shouldPlayAnimations = false;
                ProgressAnimation(1);
                SetAnimationFrame(0);
            }

            cFrame = (int)anim.CurrentFrame;
            UI.RefreshUIForFrameElementSelection(cFrame);

            for (int i = 0; i < animator.MaxJointId; i++)
            {
                Transform joint = shapeHolder.jointParents[i];

                //Flip animation matrices too.
                Matrix4x4 flipZ = Matrix4x4.Scale(new Vector3(1f, 1f, -1f));
                Matrix4x4 m = flipZ * animator.Matrices[i] * flipZ;

                joint.localPosition = m.GetPosition();
                joint.localRotation = m.rotation;
                 joint.localScale = m.lossyScale;
            }
        }

        /// <summary>
        /// Sets the current frame of the current selected animation.
        /// </summary>
        public void SetAnimationFrame(int frame, bool forceRefresh = false)
        {
            if (animator == null || animator.anims == null || cSelectedAnimation == null) return;
            RunningAnimation anim = animator.GetAnimationState(cSelectedAnimation.Code);

            //No need to update anything if the current frame is the same.
            if (anim.CurrentFrame == frame && !forceRefresh) return;
            
            anim.CurrentFrame = frame;
            anim.Iterations = 0;
            ProgressAnimation(0);
        }

        public void ResetCurrentAnimationFrame()
        {
            if (animator == null || animator.anims == null || cSelectedAnimation == null) return;

            RunningAnimation anim = animator.GetAnimationState(cSelectedAnimation.Code);
            anim.Iterations = 0;
            anim.CurrentFrame = (int)anim.CurrentFrame;

            ProgressAnimation(0);
        }

        void OnAnimationSelected(Animation animation)
        {
            animator.ResetAllAnimationsForVSMC();
            cSelectedAnimation = animation;
            ProgressAnimation(1);
            SetAnimationFrame(0, true);
            UI.RefreshUIForChangeInAnimation();
        }

        void OnAnimationDeselected(Animation animation)
        {
            cSelectedAnimation = null;
            ProgressAnimation(1);
            SetAnimationFrame(0, true);
        }

        /// <summary>
        /// Toggle between playing and pausing the current selected animation.
        /// </summary>
        public void ToggleAnimationPlaying()
        {
            if (shouldPlayAnimations) PauseSelectedAnimation();
            else PlaySelectedAnimation();
        }

        /// <summary>
        /// Pauses playing the current selected animation.
        /// </summary>
        public void PauseSelectedAnimation()
        {
            if (animator == null || animator.anims == null || cSelectedAnimation == null) return;
            shouldPlayAnimations = false;

            SetAnimationFrame(cFrame);
            UI.SetIsPlayingButtonText();
        }

        /// <summary>s
        /// Starts playing the current selected animation.
        /// </summary>
        public void PlaySelectedAnimation()
        {
            if (animator == null || animator.anims == null || cSelectedAnimation == null) return;
            shouldPlayAnimations = true;

            //For some reason, the iteration of a new animation starts at -1 instead of 0, making one-time animations run twice.
            //We need to fix that.
            RunningAnimation anim = animator.GetAnimationState(cSelectedAnimation.Code);
            if (anim.Iterations != 0) anim.Iterations = 0;
            UI.SetIsPlayingButtonText();
        }

        public bool IsPlayingAnimation()
        {
            return shouldPlayAnimations;
        }

        public void RecreateAnimationJointsAndSetup(bool recreateAnimHierarchy)
        {
            PauseSelectedAnimation();
            Shape shape = ShapeHolder.CurrentLoadedShape;
            System.DateTime t1 = System.DateTime.Now;
            shape.InitForAnimations("root");
            shapeHolder.ReparentGameObjectsByJoints();
            
            if (shape.Animations == null)
            {
                shape.Animations = new Animation[0];
            }
            animator = ClientAnimator.Create(shape.Animations, shape.Elements, shape.JointsById);

            //Create animation lists
            allAnimations = new Dictionary<string, AnimationMetaData>();
            int animID = 0;


            foreach (var anim in shape.Animations)
            {
                AnimationMetaData meta = new AnimationMetaData(anim.Name, anim.Code);
                allAnimations.Add(anim.Code, meta);
                animID++;
            }

            if (recreateAnimHierarchy) 
            {
                AnimationHierarchyManager.AnimationHierarchy.StartCreatingAnimationEntries(shape); 
            }

            Debug.Log("Calculating " + shape.Animations.Length + " animations took " + (System.DateTime.Now - t1).TotalMilliseconds + "ms.");
        }
        
        /// <summary>
        /// Should be called whenever any frame is changed. This'll recreate the matrices for a whole animation.
        /// </summary>
        /// <param name="animToRecalculate"></param>
        public void RecalculateAnimationMatrices(Animation animToRecalculate)
        {
            //This will cause the next 'OnFrame' to recalculate all the matrices.
            if (animToRecalculate == null) return;
            animToRecalculate.PrevNextKeyFrameByFrame = null;
            ResetCurrentAnimationFrame();
        }

        /// <summary>
        /// If any animation data changes, such as name, code, etc, we're better off to recreate the entire anim setup.
        /// </summary>
        public void OnAnimationDataChanged()
        {
            RecreateAnimationJointsAndSetup(true);
            AnimationSelector.main.ReselectCurrent();
            animator.ResetAllAnimationsForVSMC();
            UI.RefreshUIForChangeInAnimation();
            ObjectSelector.main.ReselectCurrent();
        }

        /// <summary>
        /// When a keyframe is added (or removed), the entire joint system needs refreshing.
        /// This'll also reset the current animation frame.
        /// </summary>
        public void OnAnyKeyframeAddedOrRemoved()
        {
            Animation cSel = cSelectedAnimation;
            int frame = cFrame;
            RecreateAnimationJointsAndSetup(false);
            ResetCurrentAnimationFrame();
            SetAnimationFrame(frame);
            RecalculateAnimationMatrices(AnimationSelector.main.GetCurrentlySelected());
            UI.RefreshUIForChangeInKeyframes();
            ObjectSelector.main.ReselectCurrent();
        }

        /// <summary>
        /// Called whenever a keyframe is changed, but not added/removed. This'll recalculate all the animation details we need.
        /// </summary>
        public void OnAnyKeyframeChanged()
        {
            //Future me may think that this won't work - Due to undoing a position set may change a non-selected animation.
            //However, only the current animation playing has any matrices, so an unselected animation will refresh anyway.
            if (!AnimationSelector.main.IsAnySelected()) return;
            RecalculateAnimationMatrices(AnimationSelector.main.GetCurrentlySelected());
            UI.RefreshUIForFrameElementSelection(cFrame);
            
        }



    }
}