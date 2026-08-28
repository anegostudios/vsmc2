using UnityEngine;
using VSMC;

public class TaskSetElementRenderPass : IEditTask
{

    public int elemUID;
    public int oldRenderPass;
    public int newRenderPass;

    public TaskSetElementRenderPass(ShapeElement elem, int newRenderPass)
    {
        elemUID = elem.elementUID;
        oldRenderPass = elem.RenderPass;
        this.newRenderPass = newRenderPass;
    }

    public override void DoTask()
    {
        ShapeElement e = ShapeElementRegistry.main.GetShapeElementByUID(elemUID);
        e.RenderPass = (short)newRenderPass;
        if (e.gameObject != null) e.gameObject.RefreshMaterialChoice();
    }

    public override void UndoTask()
    {
        ShapeElement e = ShapeElementRegistry.main.GetShapeElementByUID(elemUID);
        e.RenderPass = (short)oldRenderPass; 
        if (e.gameObject != null) e.gameObject.RefreshMaterialChoice();

    }

    public override VSEditMode GetRequiredEditMode()
    {
        return VSEditMode.Model;
    }

    public override string GetTaskName()
    {
        return "Change Render Pass";
    }

    public override bool MergeTasksIfPossible(IEditTask nextTask)
    {
        return false;
    }

}
