using System.Collections.Generic;
using UnityEngine;

namespace VSMC {

    /// <summary>
    /// Allows the enabling/disabling of elements based on the selected <see cref="VSEditMode"/>.
    /// </summary>
    public class EditModeEnablers : MonoBehaviour
    {
        /// <summary>
        /// A serializable array of gameobjects that can be listed in the Unity Editor.
        /// (Cannot use a 2D array, but can use an array of a type that contains an array :^) )
        /// </summary>
        [System.Serializable]
        public class EnableList
        {
            public GameObject[] editModeObjects;
        }

        [Header("Unity References")]
        public EnableList[] enabledObjectsForEditModeID;

        private void Start()
        {
            //Disable all...
            foreach (EnableList i in enabledObjectsForEditModeID)
            {
                foreach (GameObject g in i.editModeObjects)
                {
                    g.SetActive(false);
                }
            }
            EditModeManager.RegisterForOnModeSelect(OnEditModeSelected);
            EditModeManager.RegisterForOnModeDeselect(OnEditModeDeselected);
        }

        void OnEditModeSelected(VSEditMode sel)
        {
            foreach (GameObject g in enabledObjectsForEditModeID[(int)sel].editModeObjects)
            {
                g.SetActive(true);
            }
        }

        void OnEditModeDeselected(VSEditMode desel)
        {
            foreach (GameObject g in enabledObjectsForEditModeID[(int)desel].editModeObjects)
            {
                g.SetActive(false);
            }
        }

    }
}