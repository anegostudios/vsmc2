using UnityEngine;

namespace VSMC
{
    public class AnimationHierarchyManager : MonoBehaviour
    {

        public static AnimationHierarchyManager AnimationHierarchy;

        public GameObject animEntryPrefab;
        public Transform hierarchyParent;

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

            foreach (Animation anim in s.Animations)
            {
                CreateAnimEntry(anim);
            }
        }

        private void CreateAnimEntry(Animation anim)
        {

        }
    }
}