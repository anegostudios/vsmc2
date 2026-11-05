using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using System.Drawing;
using System.IO;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.EventSystems;
using TMPro;

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

        AnimationRootShapeElementController[] editingAnimationShapesForRootObjects;
        AnimationRootShapeElementController[] attachmentAnimationShapes;

        public AnimationRootShapeElementController[] animationShapeControllersInOrder;

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
            ShapeLoader.RegisterForOnShapeSaveEvent(BeforeShapeSave);
        }

        void OnEditModeSelect(VSEditMode select)
        {
            if (select != VSEditMode.Animation) return;
            cSelectedAnimation = null;
            cFrame = 0;

            //Load shape animators.
            List<AnimationRootShapeElementController> controllers = new List<AnimationRootShapeElementController>();
            editingAnimationShapesForRootObjects = new AnimationRootShapeElementController[ShapeHolder.CurrentLoadedShape.Elements.Length];
            for (int i = 0; i < editingAnimationShapesForRootObjects.Length; i++)
            {
                editingAnimationShapesForRootObjects[i] = new AnimationRootShapeElementController(ShapeLoader.main.shapeHolder, ShapeHolder.CurrentLoadedShape.Elements[i]);
            }
            controllers.AddRange(editingAnimationShapesForRootObjects);
            foreach (var add in MultiShapeManager.main.registeredAdditionalShapes)
            {
                controllers.Add(new AnimationRootShapeElementController(add.attachmentHolder, add.loadedAdditionalShape.Elements[0]));
            }
            
            //Get the tesselation order of animation and reset the element transformations. This won't change.
            MultiShapeManager.main.RecalculateAndApplyTransformsForAllObjects(true);
            List<ShapeElement> tessOrder = MultiShapeManager.main.storedTessellationOrder;
            List<AnimationRootShapeElementController> animationShapeControllersInOrder = new List<AnimationRootShapeElementController>();

            for (int i = 0; i < tessOrder.Count; i++)
            {
                foreach (var cont in controllers)
                {
                    if (cont.rootElement == tessOrder[i])
                    {
                        animationShapeControllersInOrder.Add(cont);
                        break;
                    }
                }
            }
            this.animationShapeControllersInOrder = animationShapeControllersInOrder.ToArray();

            UI.OnAnimationModeSelected();

            OnAnimationDataChanged();

            foreach (var cont in animationShapeControllersInOrder)
            {
                cont.SetAllJointsToAnimatorMatrices();
            }
            isInAnimatorMode = true;
        }

        void OnEditModeDeselect(VSEditMode deselect)
        {
            if (deselect != VSEditMode.Animation) return;

            AnimationSelector.main.DeselectCurrent();

            //All joints need zero-ing.
            foreach (var cont in animationShapeControllersInOrder)
            {
                cont.ZeroAllJoints();
            }
            isInAnimatorMode = false;
        }

        private void Update()
        {
            if (animationShapeControllersInOrder == null || !isInAnimatorMode || !shouldPlayAnimations) return;
            ProgressAnimation(Mathf.Min(Time.deltaTime, 1 / 30f) * playbackSpeed); //Clamp to a max of 1 anim frame per application frame.
        }

        private void OnObjectSelected(GameObject selected)
        {
            if (animationShapeControllersInOrder == null || !isInAnimatorMode || cSelectedAnimation == null) return;
            UI.RefreshUIForFrameElementSelection(cFrame);
        }

        private void OnObjectDeselected(GameObject deselected)
        {
            if (animationShapeControllersInOrder == null || !isInAnimatorMode || cSelectedAnimation == null) return;
            UI.RefreshUIForFrameElementSelection(cFrame);
        }

        public void ProgressAnimation(float dt, bool stackOverflowFailsafe = false)
        {
            bool didEditingPass = true;
            foreach (var a in animationShapeControllersInOrder)
            {
                bool t = a.ProgressAnimation(dt);
                if (a.shapeHolder == ShapeLoader.main.shapeHolder)
                {
                    if (!t) didEditingPass = false;
                }
            }

            if (!didEditingPass)
            {
                shouldPlayAnimations = false;
                if (!stackOverflowFailsafe)
                {
                    ProgressAnimation(1, true);
                    SetAnimationFrame(0);
                }   
                if (UI.forceLoopToggle.isOn)
                {
                    PlaySelectedAnimation();
                }
            }

            if (cSelectedAnimation != null)
            {
                var anim = editingAnimationShapesForRootObjects[0].animator.GetAnimationState(cSelectedAnimation.Code);
                cFrame = (int)anim.CurrentFrame;
                UI.RefreshUIForFrameElementSelection(cFrame);
            }
        }

        /// <summary>
        /// Sets the current frame of the current selected animation.
        /// </summary>
        public void SetAnimationFrame(int frame, bool forceRefresh = false)
        {
            foreach (var a in animationShapeControllersInOrder)
            {
                a.SetFrame(frame, forceRefresh);
            }
            ProgressAnimation(0, true);
        }

        public void ResetCurrentAnimationFrame()
        {
            if (animationShapeControllersInOrder == null || cSelectedAnimation == null) return;

            foreach (var cont in animationShapeControllersInOrder)
            {
                cont.ResetCurrentAnimationFrame();
            }
            ProgressAnimation(0);
        }   

        void OnAnimationSelected(Animation animation, bool manualClick)
        {
            foreach (AnimationRootShapeElementController t in animationShapeControllersInOrder)
            {
                t.SetSelectedAnimation(animation.Code);
            }
            cSelectedAnimation = animation;
            if (cFrame >= animation.QuantityFrames)
            {
                SetAnimationFrame(0, true);
            }
            UI.RefreshUIForChangeInAnimation();
        }

        void OnAnimationDeselected(Animation animation)
        {
            foreach (AnimationRootShapeElementController t in animationShapeControllersInOrder)
            {
                t.DeselectAnimation();
            }
            cSelectedAnimation = null;
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
            if (animationShapeControllersInOrder == null || cSelectedAnimation == null) return;
            shouldPlayAnimations = false;

            SetAnimationFrame(cFrame);
            UI.SetIsPlayingButtonText();
        }

        /// <summary>s
        /// Starts playing the current selected animation.
        /// </summary>
        public void PlaySelectedAnimation()
        {
            if (animationShapeControllersInOrder == null || cSelectedAnimation == null) return;
            shouldPlayAnimations = true;

            //For some reason, the iteration of a new animation starts at -1 instead of 0, making one-time animations run twice.
            //We need to fix that.
            foreach (AnimationRootShapeElementController t in animationShapeControllersInOrder)
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
            List<AnimationRootShapeElementController> tempDone = new List<AnimationRootShapeElementController>();

            for (int i = 0; i < editingAnimationShapesForRootObjects.Length; i++)
            {
                tempDone.Add(editingAnimationShapesForRootObjects[i]);
                editingAnimationShapesForRootObjects[i].RecreateAnimationJointsAndSetup(true, i != 0);
            }

            for (int i = 0; i < animationShapeControllersInOrder.Length; i++)
            {
                if (!tempDone.Contains(animationShapeControllersInOrder[i]))
                {
                    animationShapeControllersInOrder[i].RecreateAnimationJointsAndSetup(true, false);
                }
            }

            if (recreateAnimHierarchy) UI.animationHierarchy.StartCreatingEntries();
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

            foreach (var controller in animationShapeControllersInOrder)
            {
                controller.ResetAllAnimations();
            }
            UI.RefreshUIForChangeInAnimation();
            if (ObjectSelector.main.IsAnySelected())
            {
                UI.RefreshUIForFrameElementSelection(cFrame);
            }
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
            if (ObjectSelector.main.IsAnySelected())
            {
                UI.RefreshUIForFrameElementSelection(cFrame); 
            }
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
            if (EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.GetComponentInChildren<TMP_InputField>() != null)
            {
                //Don't refresh the UI if we are still interacting with it.
                return;
            }
            UI.RefreshUIForFrameElementSelection(cFrame);
        }

        public AnimationRootShapeElementController GetAnimationShapeControllerFromRootShapeElement(ShapeElement rootElement)
        {
            return animationShapeControllersInOrder.FirstOrDefault(x => x.rootElement == rootElement);
        }

        public void BeforeShapeSave(Shape shape)
        {
            //Remove any empty keyframes.
            foreach (Animation a in shape.Animations)
            {
                a.RemoveAnyEmptyKeyFrames();
            }
        }

    }
}