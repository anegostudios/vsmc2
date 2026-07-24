using Unity.VisualScripting;
namespace VSMC
{

/// <summary>
/// An undoable task that will set the render pass of a shape element.
/// Used by the UI controls.
/// </summary>
public class TaskSetRenderPass : IEditTask
{
    public int elementUID;
    public EnumRenderPass oldRenderPass;
    public EnumRenderPass newRenderPass;

    /// <summary>
    /// Creates a new render pass task for a ShapeElement.
    /// </summary>
    public TaskSetRenderPass(ShapeElement elem, EnumRenderPass newRenderPass)
    {
        this.elementUID = elem.elementUID;
        this.oldRenderPass = this.newRenderPass;
        this.newRenderPass = newRenderPass;
    }

    public override void DoTask()
    {
        ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elementUID);
        elem.RenderPass = (short)(newRenderPass);
    }

    public override void UndoTask()
    {
        ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elementUID);
        elem.RenderPass = (short)(oldRenderPass);
    }

    public override bool MergeTasksIfPossible(IEditTask nextTask)
    {
        // Keep the original oldRenderPass, but the next task's newRenderPass.
        if (nextTask is TaskSetRenderPass t2)
        {
            if (elementUID == t2.elementUID)
            {
                newRenderPass = t2.newRenderPass;
                return true;
            }
        }
        return false;
    }

    public override long GetSizeOfTaskInBytes()
    {
        return sizeof(int) + sizeof(EnumRenderPass) * 2;
    }

    public override VSEditMode GetRequiredEditMode()
    {
        return VSEditMode.Model;
    }

    public override string GetTaskName()
    {
        return "Set Render Pass";
    }
}
}
