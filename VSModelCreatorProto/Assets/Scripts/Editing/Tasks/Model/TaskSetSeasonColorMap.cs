using Unity.VisualScripting;
namespace VSMC
{

/// <summary>
/// An undoable task that will set the season color map of a shape element.
/// Used by the UI controls.
/// </summary>
public class TaskSetSeasonColorMap : IEditTask
{
    public int elementUID;
    public string oldSeasonColorMap;
    public string newSeasonColorMap;

    /// <summary>
    /// Creates a new season color map task for a ShapeElement.
    /// </summary>
    public TaskSetSeasonColorMap(ShapeElement elem, string newSeasonColorMap)
    {
        this.elementUID = elem.elementUID;
        this.oldSeasonColorMap = this.newSeasonColorMap;
        this.newSeasonColorMap = newSeasonColorMap;
    }

    public override void DoTask()
    {
        ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elementUID);
        elem.SeasonColorMap = newSeasonColorMap;
    }

    public override void UndoTask()
    {
        ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elementUID);
        elem.SeasonColorMap = oldSeasonColorMap;
    }

    public override bool MergeTasksIfPossible(IEditTask nextTask)
    {
        // Keep the original oldSeasonColorMap, but the next task's newSeasonColorMap.
        if (nextTask is TaskSetSeasonColorMap t2)
        {
            if (elementUID == t2.elementUID)
            {
                newSeasonColorMap = t2.newSeasonColorMap;
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
        return "Set season color map";
    }
}
}
