using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace VSMC {

    public class EditModeManager : MonoBehaviour
    {
        public static EditModeManager main;
        
        [NonSerialized]
        public VSEditMode cEditMode = VSEditMode.None;

        //Events allow anything to hook onto this.
        private UnityEvent<VSEditMode> OnModeSelectedEvent;
        private UnityEvent<VSEditMode> OnModeDeselectedEvent;

        [Header("Unity Refs")]
        public Image[] selectedButtonImages;
        public Color deselectedModeColor;
        public Color selectedModeColor;
        public GameObject[] blockModeChangeIfAnyAreActive;
        public GameObject[] objectsToDeactivateUponModeChange;

        private void Awake()
        {
            //Just create events and singleton instance.
            main = this;
            OnModeSelectedEvent = new UnityEvent<VSEditMode>();
            OnModeDeselectedEvent = new UnityEvent<VSEditMode>();
            foreach (Image i in selectedButtonImages)
            {
                i.color = deselectedModeColor;
            }
        }

        /// <summary>
        /// Called when a new mode is selected.
        /// </summary>
        public void OnModeSelected(VSEditMode selMode)
        {
            if (selMode == VSEditMode.None) return;
            OnModeSelectedEvent.Invoke(selMode);
            selectedButtonImages[(int)selMode].color = selectedModeColor;
        }

        /// <summary>
        /// Called when a new mode is selected.
        /// </summary>
        public void OnModeDeselected(VSEditMode deselMode)
        {
            if (deselMode == VSEditMode.None) return;
            OnModeDeselectedEvent.Invoke(deselMode);
            selectedButtonImages[(int)deselMode].color = deselectedModeColor;
        }

        /// <summary>
        /// Register a listener for when a new mode is selected.
        /// </summary>
        public static void RegisterForOnModeSelect(UnityAction<VSEditMode> action)
        {
            main.OnModeSelectedEvent.AddListener(action);
        }

        /// <summary>
        /// Register a listener for when a mode is deselected.
        /// </summary>
        public static void RegisterForOnModeDeselect(UnityAction<VSEditMode> action)
        {
            main.OnModeDeselectedEvent.AddListener(action);
        }

        /// <summary>
        /// Just calls the other SelectMode function, but this can be used on buttons.
        /// </summary>
        /// <param name="mode"></param>
        public void SelectMode(int mode)
        {
            SelectMode((VSEditMode)mode, true);
        }

        /// <summary>
        /// Selects a new mode, and deselects the old one. Also calls relevant events.
        /// Returns false if there is an active gameobject blocking the mode change.
        /// </summary>
        /// <param name="mode"></param>
        public bool SelectMode(VSEditMode mode, bool shouldCheckForBlockedObjects = false)
        {
            if (ShapeLoader.main.shapeHolder.cLoadedShape == null) return true;
            if (mode == cEditMode) return true;
            if (shouldCheckForBlockedObjects)
            {
                foreach (GameObject g in blockModeChangeIfAnyAreActive)
                {
                    if (g.activeSelf) return false;
                }
            }
            VSEditMode deselMode = cEditMode;
            cEditMode = mode;
            OnModeDeselected(deselMode);
            OnModeSelected(cEditMode);
            foreach (GameObject g in objectsToDeactivateUponModeChange)
            {
                g.SetActive(false);
            }
            return true;
        }

    }
}