using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace VSMC
{
    public class SetAPParentOverlayUIEntry : MonoBehaviour
    {
        public GameObject emptySpace;
        public GameObject parentedObject;
        public TMP_Text elementName;
        public int elemUID;
        SetAPParentOverlay overlay;

        public void Initialize(ShapeElement elem, int parentCount, SetAPParentOverlay overlay)
        {
            this.overlay = overlay;
            this.elemUID = elem.elementUID;
            elementName.text = elem.Name;
            emptySpace.GetComponent<LayoutElement>().preferredWidth = (parentCount - 1) * 16;
            parentedObject.SetActive(parentCount != 0);
            GetComponent<Image>().color = Color.clear;
            Invoke("ResolveTextSize", 0.1f);
        }

        void ResolveTextSize()
        {
            elementName.GetComponent<LayoutElement>().minWidth = elementName.textBounds.size.x;
        }

        public void OnClick()
        {
            overlay.OnElementClicked(this);
        }
    }
}