using UnityEngine;
using VSMC;

public class TaskSetOriginPoint : IEditTask
{

    public ElementCodeInShape codeInShape;
    public IOriginPoint oldPoint;
    public IOriginPoint newPoint;

    public TaskSetOriginPoint(ShapeElement rootElem, IOriginPoint oldResolvedOrigin, IOriginPoint newResolvedOrigin)
    {
        codeInShape = new ElementCodeInShape(rootElem.parentShape.shapeLoadedFromPath, rootElem.Name);
        oldPoint = oldResolvedOrigin;
        newPoint = newResolvedOrigin;
    }

    public override void DoTask()
    {
        ShapeElement elem = MultiShapeManager.main.GetShapeElementFromRootElementCodeInShape(codeInShape);
        elem.OriginPoint = newPoint;
        MultiShapeManager.main.RecalculateAndApplyTransformsForAllObjects();
        MultiShapeManager.main.UI.ResolveSelectionUI();
    }

    public override void UndoTask()
    {
        ShapeElement elem = MultiShapeManager.main.GetShapeElementFromRootElementCodeInShape(codeInShape);
        elem.OriginPoint = oldPoint;
        MultiShapeManager.main.RecalculateAndApplyTransformsForAllObjects();
        MultiShapeManager.main.UI.ResolveSelectionUI();
    }

    public override VSEditMode GetRequiredEditMode()
    {
        return VSEditMode.Model;
    }

    public override string GetTaskName()
    {
        return "Set Element Origin Point";
    }

    public override bool MergeTasksIfPossible(IEditTask nextTask)
    {
        return false;
    }

}
