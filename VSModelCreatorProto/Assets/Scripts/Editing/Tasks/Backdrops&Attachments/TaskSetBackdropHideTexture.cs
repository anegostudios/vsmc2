using UnityEngine;
using VSMC;

public class TaskSetBackdropHideTexture : IEditTask
{

    public string backdropPath;
    public bool newHideTexture;
    public bool oldHideTexture;

    public TaskSetBackdropHideTexture(LoadedBackdrop backdrop, bool newHideTexture)
    {
        this.backdropPath = backdrop.data.shapeFilepath;
        this.newHideTexture = newHideTexture;
        this.oldHideTexture = !backdrop.data.hideTextures;
    }

    public override void DoTask()
    {
        BackdropManager.main.GetBackdropFromPath(backdropPath).SetHideTextures(newHideTexture);
        BackdropAndAttachmentMenuManager.main.RefreshUIElements();
    }

    public override void UndoTask()
    {
        BackdropManager.main.GetBackdropFromPath(backdropPath).SetHideTextures(oldHideTexture);
        BackdropAndAttachmentMenuManager.main.RefreshUIElements();
    }

    public override VSEditMode GetRequiredEditMode()
    {
        return VSEditMode.Model;
    }

    public override long GetSizeOfTaskInBytes()
    {
        return 2;
    }

    public override string GetTaskName()
    {
        return "Set Backdrop Hide Textures";
    }

    public override bool MergeTasksIfPossible(IEditTask nextTask)
    {
        return false;
    }

}
