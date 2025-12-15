using TMPro;
using UnityEngine;

namespace VSMC
{
    /// <summary>
    /// To avoid an insanely huge animation manager class, this manages all the UI and UI events for animation.
    /// </summary>
    public class AnimationEditorUIElements : MonoBehaviour
    {
        public AnimationEditorManager animEditor;

        [Header("Animation Details - Misc")]
        public TMP_InputField animName;
        public GameObject test2;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {

        }

        public void UpdateUIForAnimationDetails(Animation cSel)
        {

        }

        public void UpdateUIForShapeElementDetails(Animation cSel, ShapeElement elem)
        {

        }


        #region UI Events

        public void OnFrameSliderChangeValue(float newValue)
        {
            animEditor.SetAnimationFrame((int)newValue);
        }

        #endregion

    }
}