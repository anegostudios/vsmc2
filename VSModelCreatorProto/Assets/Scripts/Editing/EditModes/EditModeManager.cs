using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VSMC {

    /// <summary>
    /// Controls the selection, events, and callbacks of edit modes, as well as the selection UI.
    /// </summary>
    public class EditModeManager : MonoBehaviour
    {
        public static EditModeManager main;
        
        [NonSerialized]
        public VSEditMode cEditMode = VSEditMode.None;

        //Events allow anything to hook onto this.
        private UnityEvent<VSEditMode> OnModeSelectedEvent;
        private UnityEvent<VSEditMode> OnModeDeselectedEvent;

        [Header("Unity Refs")]
        public TMP_Dropdown editModeDropdown;
        public GameObject[] blockModeChangeIfAnyAreActive;
        public GameObject[] objectsToDeactivateUponModeChange;

        private void Awake()
        {
            //Just create events and singleton instance.
            main = this;
            OnModeSelectedEvent = new UnityEvent<VSEditMode>();
            OnModeDeselectedEvent = new UnityEvent<VSEditMode>();
        }

        void Start()
        {
            
        }

        /// <summary>
        /// Called when a new mode is selected.
        /// </summary>
        public void OnModeSelected(VSEditMode selMode)
        {
            if (selMode == VSEditMode.None) return;
            OnModeSelectedEvent.Invoke(selMode);
            editModeDropdown.SetValueWithoutNotify((int)selMode);
            //InfoLogger.main.LogText("Selected edit mode: " + selMode.ToString());
        }

        /// <summary>
        /// Called when a new mode is selected.
        /// </summary>
        public void OnModeDeselected(VSEditMode deselMode)
        {
            if (deselMode == VSEditMode.None) return;
            OnModeDeselectedEvent.Invoke(deselMode);
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
        public bool SelectMode(VSEditMode mode, bool shouldCheckForBlockedObjects = false, bool forceRefresh = false)
        {
            if (ShapeHolder.CurrentLoadedShape == null) return true;
            if (mode == cEditMode && !forceRefresh) return true;
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

        void Update()
        {
            CheckForKeybinds();
        }

        void CheckForKeybinds()
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                int sel = -1;
                if (Input.GetKeyDown(KeyCode.Alpha0)) sel = 0;
                else if (Input.GetKeyDown(KeyCode.Alpha1)) sel = 1;
                else if (Input.GetKeyDown(KeyCode.Alpha2)) sel = 2;
                else if (Input.GetKeyDown(KeyCode.Alpha3)) sel = 3;
                if (sel != -1 && (EventSystem.current.currentSelectedGameObject?.GetComponent<TMP_InputField>() == null))
                {
                    SelectMode(sel);
                }   
            }
        }

    }
}