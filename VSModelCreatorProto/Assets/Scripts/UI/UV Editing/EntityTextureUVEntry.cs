using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace VSMC 
{
    public class EntityTextureUVEntry : MonoBehaviour
    {

        public Image entryImage;
        public GameObject outline;
        public TMP_Text elemName;
        public float[] cUVs;
        private EntityTextureUVSpace space;
        public ShapeElement elem;
        public int faceIndex;
        private RectTransform rect;

        public void Initialize(EntityTextureUVSpace space, ShapeElement elem, int faceIndex, bool attemptName)
        {
            name = elem.Name + "/" + faceIndex;
            this.space = space;
            this.rect = GetComponent<RectTransform>();
            this.elem = elem;
            this.faceIndex = faceIndex;
            entryImage.color = space.GetColorForFace(faceIndex);
            if (attemptName)
            {
                elemName.text = elem.Name;
                elemName.gameObject.SetActive(true);
            }
            else
            {
                elemName.gameObject.SetActive(false);
            }
            SetUVPosition();
        }

        public void InitializeAsEmpty(ShapeElement elem, int faceIndex)
        {
            name = elem.Name + "/" + faceIndex;
            this.rect = GetComponent<RectTransform>();
            this.elem = elem;
            this.faceIndex = faceIndex;
            cUVs = new float[4];
            elemName.gameObject.SetActive(false);
        }

        public void UpdateElementSpace(EntityTextureUVSpace space, bool attemptName)
        {
            this.space = space;
            if (attemptName)
            {
                elemName.text = elem.Name;
                elemName.gameObject.SetActive(true);
            }
            else
            {
                elemName.gameObject.SetActive(false);
            }
            SetUVPosition();
        }

        public void SetUVPosition()
        {
            if (space == null) return;
            gameObject.SetActive(!elem.ShouldHideFromView() && elem.FacesResolved[faceIndex].Enabled);
            float[] uvs = elem.FacesResolved[faceIndex].Uv;
            cUVs = (float[])uvs.Clone();
            float[] perPixelUVs = new float[]
            {
                uvs[0] / space.grid.uvRect.width,
                uvs[1] / space.grid.uvRect.height,
                uvs[2] / space.grid.uvRect.width,
                uvs[3] / space.grid.uvRect.height
            };

            rect.anchorMin = new Vector2(Mathf.Min(perPixelUVs[0], perPixelUVs[2]), Mathf.Min(1 - perPixelUVs[3], 1 - perPixelUVs[1]));
            rect.anchorMax = new Vector2(Mathf.Max(perPixelUVs[0], perPixelUVs[2]), Mathf.Max(1 - perPixelUVs[3], 1 - perPixelUVs[1]));
            rect.anchoredPosition = Vector2.zero;
            Vector2 sizeDelta = Vector2.zero;
            //Add a tiny amount of space if the size is zero.
            if (rect.anchorMin.x == rect.anchorMax.x)
            {
                sizeDelta.x = 4;
            }
            if (rect.anchorMin.y == rect.anchorMax.y)
            {
                sizeDelta.y = 4;
            }
            rect.sizeDelta = sizeDelta;
        }

        public void OnSelect()
        {
            outline.SetActive(true);
            //Setting the sibling index to 0 allows the most recently selected objects to be selected first in the UV view.
            transform.SetSiblingIndex(0);
        }

        public void OnDeselect()
        {
            outline.SetActive(false);
        }

        public void ResolveFaceSelected(bool isSelected)
        {
            outline.GetComponent<Graphic>().CrossFadeColor(
                isSelected ? UVLayoutManager.main.selectedFaceOutlineColor : UVLayoutManager.main.deselectedFaceOutlineColor,
                0.1f, false, false);
            GetComponent<Canvas>().overrideSorting = isSelected ? true : false;
            if (isSelected) GetComponent<Canvas>().sortingOrder = 5;
        }

        public void SelectThisElementAndFace()
        {
            ObjectSelector.main.SelectObject(elem.gameObject.gameObject, false, false);
            TextureEditor.main.uiElements.SetOneFaceEnabled(faceIndex);
        }
    }
}