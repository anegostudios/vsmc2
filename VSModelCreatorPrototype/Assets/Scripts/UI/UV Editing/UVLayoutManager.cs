using UnityEngine;

namespace VSMC
{
    /// <summary>
    /// Manages all the UV layout malarkey.
    /// </summary>
    public class UVLayoutManager : MonoBehaviour
    {
        public static UVLayoutManager main;
        public UVImageLayout imageLayout;
        public UVSpace[] uvSpaces;

        private void Awake()
        {
            main = this;
        }

        public void RefreshAllUVSpaces()
        {
            ShapeElementGameObject cSel = ObjectSelector.main.GetCurrentlySelected().GetComponent<ShapeElementGameObject>();
            for (int i = 0; i < uvSpaces.Length; i++) 
            {
                uvSpaces[i].SetTexture(cSel.element.FacesResolved[i].GetLoadedTexture());
                uvSpaces[i].SetUVPosition(cSel.element.FacesResolved[i].Uv);
            }
        }

        public void OnSelectedFacesChanged(bool[] selFaces)
        {
            for (int i = 0; i < selFaces.Length && i < uvSpaces.Length; i++)
            {
                uvSpaces[i].gameObject.SetActive(selFaces[i]);
            }

            imageLayout.ReorganizeElements();
        }

    }
}