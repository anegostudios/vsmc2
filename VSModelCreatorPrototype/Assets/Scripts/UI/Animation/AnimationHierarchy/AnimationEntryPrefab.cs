using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VSMC;

namespace VSMC
{
    public class AnimationEntryPrefab : MonoBehaviour
    {
        static bool AlternateColor = false;
        Color defaultColor;
        public string animCode;
        public TMP_Text animationName;

        public void InitializePrefab(string animName, string animID)
        {
            Color c = GetComponent<Image>().color;
            GetComponent<Image>().color = new Color(c.r, c.g, c.b, AlternateColor ? 0.15f : 0.25f);
            defaultColor = GetComponent<Image>().color;
            AlternateColor = !AlternateColor;
            animationName.text = animName;
            gameObject.name = animName;
            this.animCode = animID;

            AnimationSelector.main.RegisterForOnAnimationSelected(OnAnimationSelected);
            AnimationSelector.main.RegisterForOnAnimationDeselected(OnAnimationDeselected);

            //Trying to set the element name width using the editor is awful, so this manually sets it after a single frame.
            Invoke("ResolveTextSize", 0.1f);
        }

        void ResolveTextSize()
        {
            animationName.GetComponent<LayoutElement>().minWidth = Mathf.Max(animationName.textBounds.size.x, 160);
        }

        void OnAnimationSelected(Animation sel)
        {
            if (sel.Code == animCode)
            {
                GetComponent<Image>().color = new Color(1, 0.75f, 0);
            }
        }

        void OnAnimationDeselected(Animation desel)
        {
            if (desel.Code == animCode)
            {
                GetComponent<Image>().color = defaultColor;
            }
        }

        public void OnElementNameClicked()
        {
            AnimationSelector.main.SelectFromUIElement(this);
        }


    }
}
