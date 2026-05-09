using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace VSMC
{
    public class AssetPathUIEntry : MonoBehaviour
    {

        public TMP_Text priority;
        public TMP_Text path;
        public Image selectedHighlight;
        AssetPathManager pathManager;

        public void Initialize(string path, AssetPathManager manager, int priority)
        {
            this.path.text = path;
            selectedHighlight.enabled = false;
            this.pathManager = manager;
            this.priority.text = priority.ToString();
        }

        public void SelectFromClick()
        {
            pathManager.SelectAssetPath(this);
        }

    }
}