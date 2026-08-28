using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VSMC
{

    public class ElementHierarchyManager : MonoBehaviour
    {
        public static ElementHierarchyManager ElementHierarchy;

        public GameObject elementPrefab;
        public GameObject elementDragAndDropHolderPrefab;
        public RectTransform hierarchyParent;
        public bool isMainElementHierarchy = false;
        public ScrollRect hierarchyScroll;
        public TMP_InputField searchValue;

        public float elemHeight;
        public float elemSpacing;

        [Header("Sprites for Elements")]
        public Sprite CollapseChildrenSprite;
        public Sprite ExpandChildrenSprite;
        public Sprite HiddenSprite;
        public Sprite ShownSprite;

        //Runtime
        Dictionary<int, GameObject> uiElementsByUID = new Dictionary<int, GameObject>();
        List<Image> elementDragAndDropHolders = new List<Image>();
        ElementHierarchyItemPrefab currentlyDragging;
        int cDragValue = -1;
        bool isSearching;

        private void Awake()
        {
            if (isMainElementHierarchy) ElementHierarchy = this;
        }

        void Start()
        {
            UndoManager.RegisterForAnyActionDoneOrUndone(OnAnyAction);
            isSearching = false;
        }

        void Update()
        {
            if (currentlyDragging != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(hierarchyParent, Input.mousePosition, GetComponent<Camera>(), out Vector2 pos);

                //Y0 is very top of list.
                float y = -pos.y;
                y -= elemSpacing;
                float totSpacing = elemHeight;
                int v = Mathf.FloorToInt(y / (totSpacing / 3f));
                SetCurrentDragValue(v);
            }
        }

        void SetCurrentDragValue(int newValue)
        {
            if (cDragValue != -1)
            {
                //Remove hover over previous.
                elementDragAndDropHolders.Where(x => x.gameObject.activeInHierarchy).ElementAt(cDragValue).enabled = false;
            }
            if (newValue < 0 || newValue >= elementDragAndDropHolders.Count(e => e.gameObject.activeInHierarchy)) newValue = -1;
            cDragValue = newValue;
            if (cDragValue != -1)
            {
                //Hover over...
                elementDragAndDropHolders.Where(x => x.gameObject.activeInHierarchy).ElementAt(cDragValue).enabled = true;
            }
        }

        public void StartCreatingElementPrefabs(Shape shape)
        {
            //Remove existing elements.
            foreach (Transform t in hierarchyParent)
            {
                Destroy(t.gameObject);
            }
            uiElementsByUID.Clear();
            elementDragAndDropHolders.Clear();
            cDragValue = -1;
            

            foreach (ShapeElement elem in shape.Elements)
            {
                CreateElementPrefabs(elem, 0);
                DetermineIfElementIsMinimized(elem);
                //We also want to calculate whether the element is hidden here.
                elem.RecalculateHiddenStatus();
            }
            elementDragAndDropHolders.Add(Instantiate(elementDragAndDropHolderPrefab, hierarchyParent).GetComponent<Image>());
        }

        public void OnSearchUpdated(string s)
        {
            if (s == "")
            {
                StartCreatingElementPrefabs(ShapeHolder.CurrentLoadedShape);
                return;
            }
            else
            {
                foreach (var v in uiElementsByUID.Values)
                {
                    v.GetComponent<ElementHierarchyItemPrefab>().MatchesSearch(s);
                }
            }
        }

        /// <summary>
        /// Recursively creates the UI elements based on children.
        /// </summary>
        private void CreateElementPrefabs(ShapeElement parent, int pCount)
        {
            GameObject elemUI = GameObject.Instantiate(elementPrefab, hierarchyParent);
            elemUI.GetComponent<ElementHierarchyItemPrefab>().InitializePrefab(parent, pCount, this);
            elementDragAndDropHolders.Add(elemUI.GetComponent<ElementHierarchyItemPrefab>().dragAndDropImageAbove);
            elementDragAndDropHolders.Add(elemUI.GetComponent<ElementHierarchyItemPrefab>().dragAndDropImageInside);
            elementDragAndDropHolders.Add(elemUI.GetComponent<ElementHierarchyItemPrefab>().dragAndDropImageBelow);
            uiElementsByUID.Add(parent.elementUID, elemUI);
            if (parent?.Children != null)
            {
                foreach (ShapeElement child in parent.Children)
                {
                    CreateElementPrefabs(child, pCount + 1);
                }
            }
        }

        public void DetermineIfElementIsMinimized(ShapeElement elem)
        {
            EndDragOfItem(false);
            uiElementsByUID[elem.elementUID].SetActive(!elem.ShouldMinimizeInUI());
            if (elem.Children != null)
            {
                foreach (ShapeElement child in elem.Children)
                {
                    DetermineIfElementIsMinimized(child);
                }
            }
        }

        public ElementHierarchyItemPrefab GetElementHierarchyItem(ShapeElement element)
        {
            if (!uiElementsByUID.ContainsKey(element.elementUID))
            {
                Debug.LogError("Trying to access element hierarchy UI element when one does not exist.");
                return null;
            }
            return uiElementsByUID[element.elementUID].GetComponent<ElementHierarchyItemPrefab>();
        }

        public void OnAnyAction()
        {
            //Stop drag.
            EndDragOfItem(false);
        }

        public bool BeginDragOfItem(ElementHierarchyItemPrefab item)
        {
            if (EditModeManager.main.cEditMode != VSEditMode.Model)
            {
                InfoLogger.main.LogText("Reordering elements is only allow in model mode!");
                return false;
            }
            if (isSearching) return false;
            currentlyDragging = item;
            currentlyDragging.dragAndDropSelection.gameObject.SetActive(true);
            return true;
        }

        public void EndDragOfItem(bool doReparent)
        {
            if (doReparent) CommitDraggingChange();
            SetCurrentDragValue(-1);
            if (currentlyDragging != null)
            {
                currentlyDragging.dragAndDropSelection.gameObject.SetActive(false);
                currentlyDragging = null;
            }
        }


        public void CommitDraggingChange()
        {
            if (cDragValue == -1) return;
            ShapeElement toReparent = ShapeElementRegistry.main.GetShapeElementByUID(currentlyDragging.GetUID());
            Image i = elementDragAndDropHolders.Where(x => x.gameObject.activeInHierarchy).ElementAt(cDragValue);

            //Easy way is to just compare the image with what is in the appropriate holder.
            ElementHierarchyItemPrefab draggedOnto = i.GetComponentInParent<ElementHierarchyItemPrefab>();
            if (draggedOnto != null)
            {
                if (i == draggedOnto.dragAndDropImageAbove)
                {
                    TestForAndDoReorder(toReparent, ShapeElementRegistry.main.GetShapeElementByUID(draggedOnto.GetUID()), TaskReorderElement.ReorderValue.before);
                }
                else if (i == draggedOnto.dragAndDropImageBelow)
                {
                    TestForAndDoReorder(toReparent, ShapeElementRegistry.main.GetShapeElementByUID(draggedOnto.GetUID()), TaskReorderElement.ReorderValue.after);
                }
                else
                {
                    TestForAndDoReparent(toReparent, ShapeElementRegistry.main.GetShapeElementByUID(draggedOnto.GetUID()));
                }
            }
            else //Bottom-most, put at end of list.
            {
                TestForAndDoReparent(toReparent, null);
            }
        }

        public void TestForAndDoReparent(ShapeElement toReorder, ShapeElement parent)
        {

            if (toReorder == parent) return;
            //Debug.Log("Move element " + toReparent.Name + " to " + reorder.ToString() + " " + ShapeElementRegistry.main.GetShapeElementByUID(draggedOnto.GetUID()).Name);
            if (parent != null && !IsValidForReparent(toReorder, parent))
            {
                InfoLogger.main.LogText("Cannot move this element here!");
                return;
            }

            TaskReparentElement reTask = new TaskReparentElement(toReorder.elementUID, parent == null ? -1 : parent.elementUID, true);
            reTask.DoTask();
            UndoManager.main.CommitTask(reTask);
            InfoLogger.main.LogText("Successfully moved element.");
        }

        public void TestForAndDoReorder(ShapeElement toReorder, ShapeElement sibling, TaskReorderElement.ReorderValue reorderValue)
        {
            if (toReorder == sibling) return;
            if (sibling.ParentElement != null)
            {
                if (!IsValidForReparent(toReorder, sibling.ParentElement))
                {
                    InfoLogger.main.LogText("Cannot move this element here!");
                    return;
                }
            }
            TaskReorderElement reTask = new TaskReorderElement(toReorder.elementUID, reorderValue, sibling.elementUID);
            reTask.DoTask();
            UndoManager.main.CommitTask(reTask);
            InfoLogger.main.LogText("Successfully moved element.");
        }

        public bool IsValidForReparent(ShapeElement elem, ShapeElement newParent)
        {
            List<ShapeElement> toCheck = new List<ShapeElement>() { elem };

            while (toCheck.Count > 0)
            {
                //g.name is set to the exact ID of the selected object.
                if (toCheck[0].elementUID == newParent.elementUID)
                {
                    return false;
                }
                if (toCheck[0].Children != null) toCheck.AddRange(toCheck[0].Children);
                toCheck.RemoveAt(0);
            }
            return true;
        }


    }
}