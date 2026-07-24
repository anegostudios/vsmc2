using Unity.VisualScripting;
namespace VSMC
{

/// <summary>
/// An undoable task that will set the climate color map of a shape element.
/// Used by the UI controls.
/// </summary>
public class TaskSetClimateColorMap : IEditTask
{
    public int elementUID;
    public string oldClimateColorMap;
    public string newClimateColorMap;

    /// <summary>
    /// Creates a new climate color map task for a ShapeElement.
    /// </summary>
    public TaskSetClimateColorMap(ShapeElement elem, string newClimateColorMap)
    {
        this.elementUID = elem.elementUID;
        this.oldClimateColorMap = this.newClimateColorMap;
        this.newClimateColorMap = newClimateColorMap;
    }

    public override void DoTask()
    {
        ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elementUID);
        elem.ClimateColorMap = newClimateColorMap;
    }

    public override void UndoTask()
    {
        ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elementUID);
        elem.ClimateColorMap = oldClimateColorMap;
    }

    public override bool MergeTasksIfPossible(IEditTask nextTask)
    {
        // Keep the original oldClimateColorMap, but the next task's newClimateColorMap.
        if (nextTask is TaskSetClimateColorMap t2)
        {
            if (elementUID == t2.elementUID)
            {
                newClimateColorMap = t2.newClimateColorMap;
                return true;
            }
        }
        return false;
    }

    public override long GetSizeOfTaskInBytes()
    {
        // assume climate color map value size < 32 characters
        return sizeof(int) + sizeof(char) * 32 * 2;
    }

    public override VSEditMode GetRequiredEditMode()
    {
        return VSEditMode.Model;
    }

    public override string GetTaskName()
    {
        return "Set climate color map";
    }
}
}
