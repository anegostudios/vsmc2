using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace VSMC
{
    public class AnimationHierarchyManager : GenericHierarchyManager
    {

        void Start()
        {
            AnimationSelector.main.RegisterForOnAnimationSelected(OnAnimationSelected);
            AnimationSelector.main.RegisterForOnAnimationDeselected(OnAnimationDeselected);
        }

        public void OnAnimationSelected(Animation anim, bool manualClick)
        {
            GetUIEntryFromAnimationCode(anim.Code)?.OnEntrySelected(!manualClick);
        }
        
        public void OnAnimationDeselected(Animation anim)
        {
            GetUIEntryFromAnimationCode(anim.Code)?.OnEntryDeselected();
        }

        public override void StartCreatingEntries()
        {
            string cSelCode = null;
            if (cSelected != null) cSelCode = (cSelected.storedData as Animation).Code;

            //Remove existing elements.
            foreach (Transform t in hierarchyParent)
            {
                Destroy(t.gameObject);
            }
            cSelected = null;
            isSearching = false;
            currentItems = new List<GenericHierarchyItemEntry>();

            SortedDictionary<string, List<Animation>> categoriesToAnimations = new SortedDictionary<string, List<Animation>>();
            List<Animation> noCategoryAnimations = new List<Animation>();

            foreach (Animation anim in ShapeHolder.CurrentLoadedShape.Animations)
            {
                if (anim.CategoryInEditor != null && anim.CategoryInEditor != "")
                {
                    if (!categoriesToAnimations.ContainsKey(anim.CategoryInEditor))
                    {
                        categoriesToAnimations.Add(anim.CategoryInEditor, new List<Animation>());
                    }
                    categoriesToAnimations[anim.CategoryInEditor].Add(anim);
                }
                else
                {
                    noCategoryAnimations.Add(anim);
                }
            }

            foreach (var kvp in categoriesToAnimations)
            {
                GameObject categoryUI = GameObject.Instantiate(elementPrefab, hierarchyParent);
                GenericHierarchyItemEntry catEntry = categoryUI.GetComponent<GenericHierarchyItemEntry>();
                catEntry.Initialize(kvp.Key, null, 0, true, null, this);
                catEntry.replaceSelectionWithCollapse = true;
                catEntry.removeOnSearch = true;
                currentItems.Add(catEntry);
                foreach (Animation a in kvp.Value)
                {
                    CreateAnimEntry(a, catEntry);
                }
            }
            foreach (Animation a in noCategoryAnimations)
            {
                CreateAnimEntry(a);
            }

            //Highlight prev if it exists. No need to reselect as that is handled by AnimationSelector.
            if (cSelCode != null)
            {
                GenericHierarchyItemEntry entry = GetUIEntryFromAnimationCode(cSelCode);
                if (entry != null)
                {
                    entry.OnEntrySelected(false);
                }
            }
        }

        public override void OnAnySelected(GenericHierarchyItemEntry item)
        {
            AnimationSelector.main.Select(item.storedData as Animation, true);
        }

        public GenericHierarchyItemEntry GetUIEntryFromAnimationCode(string animCode)
        {
            return currentItems.FirstOrDefault(x => x.storedData != null && (x.storedData as Animation).Code == animCode);
        }

        private void CreateAnimEntry(Animation anim, GenericHierarchyItemEntry category = null)
        {
            GameObject animUI = GameObject.Instantiate(elementPrefab, hierarchyParent);
            GenericHierarchyItemEntry entry = animUI.GetComponent<GenericHierarchyItemEntry>();
            entry.Initialize(anim.Name, anim, category == null ? 0 : 1, false, category, this);
            currentItems.Add(entry);
        }
    }
}