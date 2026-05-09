using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace VSMC
{
    public class StepParentElementOverlayUIEntry : MonoBehaviour
    {
        public GameObject emptySpace;
        public GameObject parentedObject;
        public TMP_Text elementName;
        SetStepparentElementOverlay overlay;

        public void Initialize(ShapeElement elem, int parentCount, SetStepparentElementOverlay overlay)
        {
            this.overlay = overlay;
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