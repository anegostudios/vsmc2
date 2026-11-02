using UnityEngine;
using VSMC;

public class TaskAdditionalShapeSetColorIndex : IEditTask
{

    ElementCodeInShape rootElementCode;
    int newColorIndex;
    int oldColorIndex;

    public TaskAdditionalShapeSetColorIndex(ShapeElement forRootElement, int newValue)
    {
        rootElementCode = forRootElement.GetElementCodeInShape();
        newColorIndex = newValue;
        oldColorIndex = MultiShapeManager.main.GetLoadedAdditionalShapeFromRootElementCode(rootElementCode).data.flatColorIndex;
    }

    public override void DoTask()
    {
        LoadedAdditionalShape las = MultiShapeManager.main.GetLoadedAdditionalShapeFromRootElementCode(rootElementCode);
        las.SetColorIndex(newColorIndex);
        MultiShapeManager.main.UI.ResolveSelectionUI();
    }

    public override void UndoTask()
    {
        LoadedAdditionalShape las = MultiShapeManager.main.GetLoadedAdditionalShapeFromRootElementCode(rootElementCode);
        las.SetColorIndex(oldColorIndex);
        MultiShapeManager.main.UI.ResolveSelectionUI();
    }

    public override VSEditMode GetRequiredEditMode()
    {
        return VSEditMode.Model;
    }

    public override string GetTaskName()
    {
        return "Set Color Index for Additional Shape";
    }

    public override bool MergeTasksIfPossible(IEditTask nextTask)
    {
        return false;
    }
}
