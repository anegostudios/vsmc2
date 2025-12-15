using System.Collections.Generic;
using UnityEngine;

namespace VSMC
{
    public class AnimationHierarchyManager : MonoBehaviour
    {

        public static AnimationHierarchyManager AnimationHierarchy;

        public GameObject animEntryPrefab;
        public Transform hierarchyParent;

        Dictionary<string, GameObject> uiElementsByAnimCode = new Dictionary<string, GameObject>();

        private void Awake()
        {
            AnimationHierarchy = this;
        }

        public void StartCreatingAnimationEntries(Shape s)
        {
            foreach (Transform t in hierarchyParent)
            {
                Destroy(t.gameObject);
            }
            uiElementsByAnimCode.Clear();

            foreach (Animation anim in s.Animations)
            {
                CreateAnimEntry(anim);
            }
        }

        private void CreateAnimEntry(Animation anim)
        {
            GameObject animUI = GameObject.Instantiate(animEntryPrefab, hierarchyParent);
            animUI.GetComponent<AnimationEntryPrefab>().InitializePrefab(anim.Name, anim.Code);
            uiElementsByAnimCode.Add(anim.Code, animUI);
        }


        public AnimationEntryPrefab GetAnimationHierarchyItem(Animation anim)
        {
            if (!uiElementsByAnimCode.ContainsKey(anim.Code))
            {
                Debug.LogError("Trying to access animation hierarchy UI element when one does not exist.");
                return null;
            }
            return uiElementsByAnimCode[anim.Code].GetComponent<AnimationEntryPrefab>();
        }
    }
}