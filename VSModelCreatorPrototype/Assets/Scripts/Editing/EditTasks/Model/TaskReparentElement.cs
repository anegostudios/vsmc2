using UnityEngine;
using VSMC;

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
        ShapeElement newParent = ShapeElementRegistry.main.GetShapeElementByUID(newParentID);
        
        child.ParentElement = newParent;
        if (newParent.Children == null) newParent.Children = new ShapeElement[] { child };
        else
        {
            newParent.Children = newParent.Children.Append(child);
        }

        //If old parent exists...
        if (oldParentID != -1)
        {
            ShapeElement oldParent = ShapeElementRegistry.main.GetShapeElementByUID(oldParentID);
            oldParent.Children = oldParent.Children.Remove(child);
            oldParent.RecreateObjectMeshAndTransforms();
        }

        newParent.RecreateObjectMeshAndTransforms();
    }

    public override void UndoTask()
    {
        ShapeElement child = ShapeElementRegistry.main.GetShapeElementByUID(elemToReparentID);
        ShapeElement newParent = ShapeElementRegistry.main.GetShapeElementByUID(newParentID);

        if (oldParentID != -1)
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
        else
        {
            child.ParentElement = null;
            child.RecreateObjectMeshAndTransforms();
        }
        newParent.RecreateObjectMeshAndTransforms();

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
