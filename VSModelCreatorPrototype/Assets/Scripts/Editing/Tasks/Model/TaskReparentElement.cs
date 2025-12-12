using UnityEngine;
using VSMC;

namespace VSMC
{
    /// <summary>
    /// An undoable task that changes the shape elements parent to another.
    /// </summary>
    public class TaskReparentElement : IEditTask
    {

        public int elemToReparentID;
        public int newParentID;
        public int oldParentID;
        public bool keepGlobalTransform;

        /// <summary>
        /// Reparents an element. CAUTION - This does NOT include a failsafe for reparenting an object to itself or its children.
        /// </summary>
        public TaskReparentElement(int elemToReparentID, int newParentID, bool keepGlobalTransform = true)
        {
            this.elemToReparentID = elemToReparentID;
            this.newParentID = newParentID;
            this.keepGlobalTransform = keepGlobalTransform;

            ShapeElement child = ShapeElementRegistry.main.GetShapeElementByUID(elemToReparentID);
            if (child.ParentElement == null)
            {
                oldParentID = -1;
            }
            else
            {
                oldParentID = child.ParentElement.elementUID;
            }
        }

        public override void DoTask()
        {
            ShapeElement child = ShapeElementRegistry.main.GetShapeElementByUID(elemToReparentID);

            //Remove old parent first.
            if (oldParentID == -1)
            {
                ShapeLoader.main.shapeHolder.cLoadedShape.RemoveRootShapeElement(child);
            }
            else
            {
                ShapeElement oldParent = ShapeElementRegistry.main.GetShapeElementByUID(oldParentID);
                oldParent.Children = oldParent.Children.Remove(child);
                oldParent.RecreateObjectMeshAndTransforms();
            }

            //Now add new parent.
            if (newParentID == -1)
            {
                ShapeLoader.main.shapeHolder.cLoadedShape.AddRootShapeElement(child);
                child.ParentElement = null;
                child.RecreateObjectMeshAndTransforms();
            }
            else
            {
                ShapeElement newParent = ShapeElementRegistry.main.GetShapeElementByUID(newParentID);
                child.ParentElement = newParent;
                if (newParent.Children == null) newParent.Children = new ShapeElement[] { child };
                else
                {
                    newParent.Children = newParent.Children.Append(child);
                }
                newParent.RecreateObjectMeshAndTransforms();
            }
            ElementHierarchyManager.ElementHierarchy.StartCreatingElementPrefabs(ShapeLoader.main.shapeHolder.cLoadedShape);
        }

        public override void UndoTask()
        {
            ShapeElement child = ShapeElementRegistry.main.GetShapeElementByUID(elemToReparentID);
            //ShapeElement newParent = ShapeElementRegistry.main.GetShapeElementByUID(newParentID);

            //Do the opposite - So remove the new parent first.
            if (newParentID == -1)
            {
                ShapeLoader.main.shapeHolder.cLoadedShape.RemoveRootShapeElement(child);
            }
            else
            {
                ShapeElement newParent = ShapeElementRegistry.main.GetShapeElementByUID(newParentID);
                newParent.Children = newParent.Children.Remove(child);
                newParent.RecreateObjectMeshAndTransforms();
            }

            //Now add the old parent back.
            if (oldParentID == -1)
            {
                ShapeLoader.main.shapeHolder.cLoadedShape.AddRootShapeElement(child);
                child.ParentElement = null;
                child.RecreateObjectMeshAndTransforms();
            }
            else
            {
                ShapeElement oldParent = ShapeElementRegistry.main.GetShapeElementByUID(oldParentID);

                child.ParentElement = oldParent;
                if (oldParent.Children == null) oldParent.Children = new ShapeElement[] { child };
                else
                {
                    oldParent.Children = oldParent.Children.Append(child);
                }
                oldParent.RecreateObjectMeshAndTransforms();
            }
            ElementHierarchyManager.ElementHierarchy.StartCreatingElementPrefabs(ShapeLoader.main.shapeHolder.cLoadedShape);

        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Model;
        }

        public override long GetSizeOfTaskInBytes()
        {
            return sizeof(int) * 3 + sizeof(bool);
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

        public override string GetTaskName()
        {
            return "Reparent Element";
        }

    }
}