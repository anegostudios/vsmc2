using UnityEngine;
using UnityEngine.UI;

namespace VSMC
{
    /// <summary>
    /// A small addition to the Toggle UI element that allows a 'mixed' selection.
    /// </summary>
    public class MixedElementToggle : MonoBehaviour
    {
        public Toggle toggle;
        public GameObject mixed;

        private void Start()
        {
            toggle.onValueChanged.AddListener(x => { OnToggleChanged(); });
        }

        public void SetToggleValue(bool on, bool mixed)
        {
            if (mixed)
            {
                toggle.SetIsOnWithoutNotify(false);
                this.mixed.SetActive(true);
                return;
            }
            this.mixed.SetActive(false);
            toggle.SetIsOnWithoutNotify(on);
        }

        public void OnToggleChanged()
        {
            this.mixed.SetActive(false);
        }
    }
}