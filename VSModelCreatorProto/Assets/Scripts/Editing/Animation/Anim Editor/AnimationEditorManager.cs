using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Unity.Collections;
using Unity.VisualScripting;
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
        //public ShapeHolder shapeHolder;
        public AnimationEditorUIElements UI;
        public bool isInAnimatorMode = false;

        AnimationShapeController backdropAnimationShape;
        AnimationShapeController editingAnimationShape;
        AnimationShapeController[] attachmentAnimationShapes;

        public AnimationShapeController[] animationShapeControllers;

        bool shouldPlayAnimations = false;

        public int cFrame;
        public Animation cSelectedAnimation;
        public float playbackSpeed = 1;

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

            //Load shape animators.
            List<AnimationShapeController> controllers = new List<AnimationShapeController>();
            editingAnimationShape = new AnimationShapeController(ShapeLoader.main.shapeHolder);
            controllers.Add(editingAnimationShape);
            if (BackdropManager.main.cActiveBackdrop != null)
            {
                backdropAnimationShape = new AnimationShapeController(BackdropManager.main.cActiveBackdrop.backdropHolder);
                controllers.Add(backdropAnimationShape);
            }
            List<AnimationShapeController> atts = new List<AnimationShapeController>();
            foreach (LoadedAdditionalShape att in AttachmentManager.main.GetEnabledAttachments())
            {
                atts.Add(new AnimationShapeController(att.attachmentHolder));
            }
            controllers.AddRange(atts);
            attachmentAnimationShapes = atts.ToArray();
            animationShapeControllers = controllers.ToArray();
            OnAnimationDataChanged();

            foreach (var cont in animationShapeControllers)
            {
                cont.SetAllJointsToAnimatorMatrices();
            }
            isInAnimatorMode = true;
        }

        void OnEditModeDeselect(VSEditMode deselect)
        {
            if (deselect != VSEditMode.Animation) return;

            //All joints need zero-ing.
            foreach (var cont in animationShapeControllers)
            {
                cont.ZeroAllJoints();
            }
            isInAnimatorMode = false;
        }

        private void Update()
        {
            if (animationShapeControllers == null || !isInAnimatorMode || !shouldPlayAnimations) return;
            ProgressAnimation(Mathf.Min(Time.deltaTime, 1 / 30f) * playbackSpeed); //Clamp to a max of 1 anim frame per application frame.
        }

        private void OnObjectSelected(GameObject selected)
        {
            if (animationShapeControllers == null || !isInAnimatorMode || cSelectedAnimation == null) return;
            UI.RefreshUIForFrameElementSelection(cFrame);
        }

        private void OnObjectDeselected(GameObject deselected)
        {
            if (animationShapeControllers == null || !isInAnimatorMode || cSelectedAnimation == null) return;
            UI.RefreshUIForFrameElementSelection(cFrame);
        }

        public void ProgressAnimation(float dt, bool stackOverflowFailsafe = false)
        {
            //Backdrop, followed by current, followed by attachments.
            if (backdropAnimationShape != null) backdropAnimationShape.ProgressAnimation(dt);
            bool didEditingPass = editingAnimationShape.ProgressAnimation(dt);
            foreach (var a in attachmentAnimationShapes)
            {
                a.ProgressAnimation(dt);
            }

            if (!didEditingPass)
            {
                shouldPlayAnimations = false;
                if (!stackOverflowFailsafe)
                {
                    ProgressAnimation(1, true);
                    SetAnimationFrame(0);
                }   
            }

            if (cSelectedAnimation != null)
            {
                var anim = editingAnimationShape.animator.GetAnimationState(cSelectedAnimation.Code);
                cFrame = (int)anim.CurrentFrame;
                UI.RefreshUIForFrameElementSelection(cFrame);
            }
        }

        /// <summary>
        /// Sets the current frame of the current selected animation.
        /// </summary>
        public void SetAnimationFrame(int frame, bool forceRefresh = false)
        {
            foreach (var a in animationShapeControllers)
            {
                a.SetFrame(frame, forceRefresh);
            }
            ProgressAnimation(0, true);
        }

        public void ResetCurrentAnimationFrame()
        {
            if (animationShapeControllers == null || cSelectedAnimation == null) return;

            foreach (var cont in animationShapeControllers)
            {
                cont.ResetCurrentAnimationFrame();
            }
            ProgressAnimation(0);
        }   

        void OnAnimationSelected(Animation animation)
        {
            foreach (AnimationShapeController t in animationShapeControllers)
            {
                t.SetSelectedAnimation(animation.Code);
            } 
            cSelectedAnimation = animation;
            UI.RefreshUIForChangeInAnimation();
        }

        void OnAnimationDeselected(Animation animation)
        {
            foreach (AnimationShapeController t in animationShapeControllers)
            {
                t.DeselectAnimation();
            }
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
            if (animationShapeControllers == null || cSelectedAnimation == null) return;
            shouldPlayAnimations = false;

            SetAnimationFrame(cFrame);
            UI.SetIsPlayingButtonText();
        }

        /// <summary>s
        /// Starts playing the current selected animation.
        /// </summary>
        public void PlaySelectedAnimation()
        {
            if (animationShapeControllers == null || cSelectedAnimation == null) return;
            shouldPlayAnimations = true;

            //For some reason, the iteration of a new animation starts at -1 instead of 0, making one-time animations run twice.
            //We need to fix that.
            foreach (AnimationShapeController t in animationShapeControllers)
            {
                RunningAnimation anim = t.animator.GetAnimationState(cSelectedAnimation.Code);
                if (anim != null && anim.Iterations != 0) anim.Iterations = 0;
            }
            UI.SetIsPlayingButtonText();
        }

        public bool IsPlayingAnimation()
        {
            return shouldPlayAnimations;
        }

        public void RecreateAnimationJointsAndSetup(bool recreateAnimHierarchy)
        {
            PauseSelectedAnimation();

            if (backdropAnimationShape != null) backdropAnimationShape.RecreateAnimationJointsAndSetup(false);
            editingAnimationShape.RecreateAnimationJointsAndSetup(false);
            foreach (AnimationShapeController t in attachmentAnimationShapes)
            {
                t.RecreateAnimationJointsAndSetup(true);
            }

            if (recreateAnimHierarchy) AnimationHierarchyManager.AnimationHierarchy.StartCreatingAnimationEntries(ShapeHolder.CurrentLoadedShape);
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

            foreach (var controller in animationShapeControllers)
            {
                controller.ResetAllAnimations();
            }
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

        public AnimationShapeController GetAnimationShapeControllerFromShape(Shape shape)
        {
            //This totally works...
            return null;
        }
    }
}