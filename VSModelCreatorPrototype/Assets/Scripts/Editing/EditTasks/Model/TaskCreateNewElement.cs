using UnityEngine;

namespace VSMC
{
    /// <summary>
    /// Create a new element.
    /// </summary>
    public class TaskCreateNewElement : IEditTask
    {
        public int parentUID = -1;
        public ShapeElement createdElement;

        public TaskCreateNewElement(ShapeElement parent)
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
            createdElement = new ShapeElement();
            ShapeElementRegistry.main.UnregisterShapeElement(createdElement);
            if (parent != null)
            {
                parentUID = parent.elementUID;
                createdElement.SetParent(parent);
            }

            //We will tesselate the object and create its game object, but will move it immediately to the deletion state.
            ShapeTesselator.TesselateShapeElements(new ShapeElement[] { createdElement });
            ShapeTesselator.ResolveMatricesForShapeElementAndChildren(createdElement);
            ShapeLoader.main.shapeHolder.CreateShapeElementGameObject(createdElement);

            ShapeLoader.main.shapeHolder.SendElementToDeletionLimbo(createdElement);
            createdElement.RemoveParent();

        }

        public override void DoTask()
        {
            //The element has already been created in the constructor. We just need to register it here and make it available.
            //Set the parent of the new element.
            if (parentUID != -1)
            {
                ShapeElement parent = ShapeElementRegistry.main.GetShapeElementByUID(parentUID);
                createdElement.SetParent(parent);
            }
            else
            {
                ShapeLoader.main.shapeHolder.cLoadedShape.AddRootShapeElement(createdElement);
            }

            //Add the element back into the registry.
            ShapeElementRegistry.main.ReregisterShapeElement(createdElement);

            //Restore the gameobject.
            ShapeLoader.main.shapeHolder.RestoreElementFromDeletionLimbo(createdElement);

            //We recreate the entire element list for easy usage.
            ElementHierarchyManager.ElementHierarchy.StartCreatingElementPrefabs(ShapeLoader.main.shapeHolder.cLoadedShape);

            //Finally, select the object to make it noticable.
            ObjectSelector.main.SelectObject(createdElement.gameObject.gameObject, false, false);
        }

        public override void UndoTask()
        {
            //Essentially hide the newly created element, making it easier to restore.
            ShapeLoader.main.shapeHolder.SendElementToDeletionLimbo(createdElement);
            ShapeElementRegistry.main.UnregisterShapeElement(createdElement);
            if (parentUID != -1)
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
            //Don't merge creation tasks.
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
            return "Create Element";
        }

    }
}