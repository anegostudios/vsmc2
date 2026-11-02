using UnityEngine;
using VSMC;

public class TaskAdditionalShapeHideTextures : IEditTask
{

    ElementCodeInShape rootElementCode;
    bool newHideTextures;
    bool oldHideTextures;

    public TaskAdditionalShapeHideTextures(ShapeElement forRootElement, bool newValue)
    {
        rootElementCode = forRootElement.GetElementCodeInShape();
        newHideTextures = newValue;
        oldHideTextures = MultiShapeManager.main.GetLoadedAdditionalShapeFromRootElementCode(rootElementCode).data.hideTextures;
    }

    public override void DoTask()
    {
        LoadedAdditionalShape las = MultiShapeManager.main.GetLoadedAdditionalShapeFromRootElementCode(rootElementCode);
        las.SetHideTextures(newHideTextures);
        MultiShapeManager.main.UI.ResolveSelectionUI();
    }

    public override void UndoTask()
    {
        LoadedAdditionalShape las = MultiShapeManager.main.GetLoadedAdditionalShapeFromRootElementCode(rootElementCode);
        las.SetHideTextures(oldHideTextures);
        MultiShapeManager.main.UI.ResolveSelectionUI();
    }

    public override VSEditMode GetRequiredEditMode()
    {
        return VSEditMode.Model;
    }

    public override string GetTaskName()
    {
        return "Toggle Hide Textures for Additional Shape";
    }

    public override bool MergeTasksIfPossible(IEditTask nextTask)
    {
        return false;
    }
}
