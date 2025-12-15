using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;


namespace VSMC
{
    public class AnimationSelector : MonoBehaviour
    {
        public static AnimationSelector main;

        UnityEvent<Animation> OnAnimationSelected;
        UnityEvent<Animation> OnAnimationDeselected;
        Animation cSelected;

        private void Awake()
        {
            main = this;
            cSelected = null;
            OnAnimationSelected = new UnityEvent<Animation>();
            OnAnimationDeselected = new UnityEvent<Animation>();
        }

        private void Start()
        {
            EditModeManager.RegisterForOnModeSelect(OnEditModeChange);
        }

        public bool IsAnySelected()
        {
            return cSelected != null;
        }

        public Animation GetCurrentlySelected()
        {
            return cSelected;
        }

        public void SelectFromUIElement(AnimationEntryPrefab animEntry)
        {
            Select(ShapeLoader.main.shapeHolder.cLoadedShape.Animations.First(x => { return x.Code == animEntry.animCode; }));
        }

        public void Select(Animation animation)
        {
            if (EditModeManager.main.cEditMode != VSEditMode.Animation)
            {
                return;
            }
            if (animation == cSelected) return;
            if (IsAnySelected()) DeselectCurrent();

            cSelected = animation;
            OnAnimationSelected.Invoke(animation);
        }

        public void DeselectCurrent()
        {
            if (cSelected == null) return;
            Animation deselected = cSelected;
            cSelected = null;
            OnAnimationDeselected.Invoke(deselected);
        }

        public void RegisterForOnAnimationSelected(UnityAction<Animation> toCall)
        {
            OnAnimationSelected.AddListener(toCall);
        }

        public void RegisterForOnAnimationDeselected(UnityAction<Animation> toCall)
        {
            OnAnimationDeselected.AddListener(toCall);
        }

        void OnEditModeChange(VSEditMode mode)
        {
            if (mode != VSEditMode.Animation)
            {
                DeselectCurrent();
            }
        }

    }
}