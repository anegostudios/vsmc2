using Unity.VisualScripting;
namespace VSMC
{

/// <summary>
/// An undoable task that will set the Z offset of a shape element.
/// Used by the UI controls.
/// </summary>
public class TaskSetZOffset : IEditTask
{
    public int elementUID;
    public short oldZOffset;
    public short newZOffset;

    /// <summary>
    /// Creates a new Z offset task for a ShapeElement.
    /// </summary>
    public TaskSetZOffset(ShapeElement elem, short newZOffset)
    {
        this.elementUID = elem.elementUID;
        this.oldZOffset = this.newZOffset;
        this.newZOffset = newZOffset;
    }

    public override void DoTask()
    {
        ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elementUID);
        elem.ZOffset = newZOffset;
    }

    public override void UndoTask()
    {
        ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elementUID);
        elem.ZOffset = oldZOffset;
    }

    public override bool MergeTasksIfPossible(IEditTask nextTask)
    {
        // Keep the original oldZOffset, but the next task's newZOffset.
        if (nextTask is TaskSetZOffset t2)
        {
            if (elementUID == t2.elementUID)
            {
                newZOffset = t2.newZOffset;
                return true;
            }
        }
        return false;
    }

    public override long GetSizeOfTaskInBytes()
    {
        return sizeof(int) + sizeof(short) * 2;
    }

    public override VSEditMode GetRequiredEditMode()
    {
        return VSEditMode.Model;
    }

    public override string GetTaskName()
    {
        return "Set Z offset";
    }
}
}
