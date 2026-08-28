using UnityEngine;
using VSMC;

public class TaskReorderElement : TaskReparentElement
{


    public enum ReorderValue
    {
        before,
        after
    }

    int siblingIndexBefore;
    int siblingIndexAfter;

    public TaskReorderElement(int elemToReparentID, ReorderValue reorderValue, int relativeToUID) :
    base(elemToReparentID, ShapeElementRegistry.main.GetShapeElementByUID(relativeToUID).ParentElement == null ? -1 : ShapeElementRegistry.main.GetShapeElementByUID(relativeToUID).ParentElement.elementUID, true)
    {
        updateHierarchy = false;
        ShapeElement eToMove = ShapeElementRegistry.main.GetShapeElementByUID(elemToReparentID);
        if (eToMove.ParentElement == null)
        {
            siblingIndexBefore = ShapeHolder.CurrentLoadedShape.Elements.IndexOf(eToMove);
        }
        else
        {
            siblingIndexBefore = eToMove.ParentElement.Children.IndexOf(eToMove);
        }

        ShapeElement sibling = ShapeElementRegistry.main.GetShapeElementByUID(relativeToUID);
        if (sibling.ParentElement == null)
        {
            siblingIndexAfter = ShapeHolder.CurrentLoadedShape.Elements.Remove(eToMove).IndexOf(sibling);
        }
        else
        {
            siblingIndexAfter = sibling.ParentElement.Children.Remove(eToMove).IndexOf(sibling);
        }
        if (reorderValue == ReorderValue.after) siblingIndexAfter++;
    }

    public override void DoTask()
    {
        base.DoTask();
        //Move the parent, and then set the sibling index.
        ShapeElement eToMove = ShapeElementRegistry.main.GetShapeElementByUID(elemToReparentID);
        if (eToMove.ParentElement == null)
        {
            ShapeHolder.CurrentLoadedShape.Elements = ShapeHolder.CurrentLoadedShape.Elements.Remove(eToMove).InsertAt(eToMove, siblingIndexAfter);
        }
        else
        {
            eToMove.ParentElement.Children = eToMove.ParentElement.Children.Remove(eToMove).InsertAt(eToMove, siblingIndexAfter);
        }
        ElementHierarchyManager.ElementHierarchy.StartCreatingElementPrefabs(ShapeHolder.CurrentLoadedShape);
    }

    public override void UndoTask()
    {
        base.UndoTask();
        //Change parent back, then revert back to position.
        ShapeElement eToMove = ShapeElementRegistry.main.GetShapeElementByUID(elemToReparentID);
        if (eToMove.ParentElement == null)
        {
            ShapeHolder.CurrentLoadedShape.Elements = ShapeHolder.CurrentLoadedShape.Elements.Remove(eToMove).InsertAt(eToMove, siblingIndexBefore);
        }
        else
        {
            eToMove.ParentElement.Children = eToMove.ParentElement.Children.Remove(eToMove).InsertAt(eToMove, siblingIndexBefore);
        }
        ElementHierarchyManager.ElementHierarchy.StartCreatingElementPrefabs(ShapeHolder.CurrentLoadedShape);
    }

    public override string GetTaskName()
    {
        return "Reorder Element";
    }

}
