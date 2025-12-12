using UnityEngine;

namespace VSMC
{
    /// <summary>
    /// An undoable task that duplicates an element and its children.
    /// </summary>
    public class TaskCopyElement : IEditTask
    {
        public int copyUID = -1;
        public ShapeElement createdElement;

        public TaskCopyElement(ShapeElement toCopy)
        {
            /*
             * This may seem a bit of a weird implementation.
             * As soon as the task is defined, we create the shape element, assign it a UID, set the parent, and create the gameobject.
             * However, we immediately remove it from the registry and parent as we don't want it being used at this point.
             * 
             * When 'DoTask' is called, we truly register the object with the previously given UID.
             * We also reassign its parent element, and put the element game object into view.
             * 
             * On 'UndoTask', we remove the UID and remove the parent.
             * This results in us being in the exact same state before 'DoTask', giving us the ability to easily redo.
             */
            createdElement = toCopy.CopyThisElement();
            copyUID = toCopy.elementUID;

        }

        public override void DoTask()
        {
            ShapeElement orig = ShapeElementRegistry.main.GetShapeElementByUID(copyUID);
            //The element has already been created in the constructor. We just need to register it here and make it available.
            //Set the parent of the new element.
            if (orig.ParentElement != null)
            {
                createdElement.SetParent(orig.ParentElement);
            }
            else
            {
                ShapeLoader.main.shapeHolder.cLoadedShape.AddRootShapeElement(createdElement);
            }

            //Add the element back into the registry.
            ShapeElementRegistry.main.ReregisterShapeElement(createdElement, true);

            //Restore the gameobject.
            ShapeLoader.main.shapeHolder.RestoreElementFromDeletionLimbo(createdElement, true);

            //We recreate the entire element list for easy usage.
            ElementHierarchyManager.ElementHierarchy.StartCreatingElementPrefabs(ShapeLoader.main.shapeHolder.cLoadedShape);

            //Finally, select the object to make it noticable.
            ObjectSelector.main.SelectObject(createdElement.gameObject.gameObject, false, false);
        }

        public override void UndoTask()
        {
            //Essentially hide the newly created element, making it easier to restore.
            ShapeLoader.main.shapeHolder.SendElementToDeletionLimbo(createdElement, true);
            ShapeElementRegistry.main.UnregisterShapeElement(createdElement, true);
            if (createdElement.ParentElement != null)
            {
                createdElement.RemoveParent();
            }
            else
            {
                ShapeLoader.main.shapeHolder.cLoadedShape.RemoveRootShapeElement(createdElement);
            }
            //We recreate the entire element list for easy usage.
            ElementHierarchyManager.ElementHierarchy.StartCreatingElementPrefabs(ShapeLoader.main.shapeHolder.cLoadedShape);
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            //Obviously not.
            return false;
        }

        public override long GetSizeOfTaskInBytes()
        {
            return 8 + sizeof(int);
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Model;
        }

        public override string GetTaskName()
        {
            return "Duplicate Element";
        }

    }
}