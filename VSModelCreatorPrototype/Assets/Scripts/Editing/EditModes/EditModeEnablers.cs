using System.Collections.Generic;
using UnityEngine;

namespace VSMC {
    public class EditModeEnablers : MonoBehaviour
    {

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