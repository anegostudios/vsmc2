using UnityEngine;
using VSMC;

public class TaskAddNewAttachmentPoint : IEditTask
{

    public string apCode;
    public int apParentUID;

    public TaskAddNewAttachmentPoint(string code, ShapeElement parent)
    {
        apCode = code;
        apParentUID = parent.elementUID;
    }

    public override void DoTask()
    {
        LoadedAP ap = new LoadedAP(ShapeElementRegistry.main.GetShapeElementByUID(apParentUID)) { code = apCode };
        AttachmentPointsManager.main.AddNewAP(ap);
    }

    public override void UndoTask()
    {
        AttachmentPointsManager.main.RemoveAP(AttachmentPointsManager.main.GetLoadedAPFromCode(apCode));
    }

    public override VSEditMode GetRequiredEditMode()
    {
        return VSEditMode.Model;
    }

    public override string GetTaskName()
    {
        return "Add New AP";
    }

    public override bool MergeTasksIfPossible(IEditTask nextTask)
    {
        return false;
    }
}
