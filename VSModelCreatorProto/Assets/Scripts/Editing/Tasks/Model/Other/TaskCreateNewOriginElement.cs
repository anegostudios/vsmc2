using UnityEngine;
using VSMC;

public class TaskCreateNewOriginElement : IEditTask
{

    public int[] rootElemUIDs;
    public IOriginPoint[] rootElemOriginPoints;
    public string[] rootElemStepParentCodes;
    public ShapeElement createdElement;

    public TaskCreateNewOriginElement()
    {
        int c = ShapeHolder.CurrentLoadedShape.Elements.Length;
        rootElemUIDs = new int[c];
        rootElemOriginPoints = new IOriginPoint[c];
        rootElemStepParentCodes = new string[c];
        for (int i = 0; i < ShapeHolder.CurrentLoadedShape.Elements.Length; i++)
        {
            rootElemUIDs[i] = ShapeHolder.CurrentLoadedShape.Elements[i].elementUID;
            rootElemOriginPoints[i] = ShapeHolder.CurrentLoadedShape.Elements[i].OriginPoint;
            rootElemStepParentCodes[i] = ShapeHolder.CurrentLoadedShape.Elements[i].StepParentName;
        }

        createdElement = new ShapeElement(ShapeHolder.CurrentLoadedShape, 0, "origin"); //Create elem with 0 enabled faces.
        createdElement.To = new double[] { 0, 0, 0 };
        createdElement.ResolveFacesAndTextures(TextureManager.main.loadedTextures);
        ShapeElementRegistry.main.UnregisterShapeElement(createdElement);

        //We will tesselate the object and create its game object, but will move it immediately to the deletion state.
        ShapeTesselator.TesselateShapeElements(new ShapeElement[] { createdElement });
        ShapeTesselator.ResolveLocalMatricesForShapeElementAndChildren(createdElement);
        ShapeLoader.main.shapeHolder.CreateShapeElementGameObject(createdElement);

        ShapeLoader.main.shapeHolder.SendElementToDeletionLimbo(createdElement);
    }

    public override void DoTask()
    {
        ShapeHolder.CurrentLoadedShape.AddRootShapeElement(createdElement);

        //Add the element back into the registry.
        ShapeElementRegistry.main.ReregisterShapeElement(createdElement);

        //Restore the gameobject.
        ShapeLoader.main.shapeHolder.RestoreElementFromDeletionLimbo(createdElement);

        //Now add the root element children.
        for (int i = 0; i < rootElemUIDs.Length; i++)
        {
            ShapeElement root = ShapeElementRegistry.main.GetShapeElementByUID(rootElemUIDs[i]);
            root.OriginPoint = new TransformOnlyOriginPoint();
            root.StepParentName = null;
            ShapeHolder.CurrentLoadedShape.RemoveRootShapeElement(root);
            root.SetParent(createdElement);
        }

        //We recreate the entire element list for easy usage.
        ElementHierarchyManager.ElementHierarchy.StartCreatingElementPrefabs(ShapeHolder.CurrentLoadedShape);

        //Refresh the model order.
        MultiShapeManager.main.RecalculateAndApplyTransformsForAllObjects();

        //Finally, select the object to make it noticable.
        ObjectSelector.main.SelectObject(createdElement.gameObject.gameObject, false, false);
    }

    public override void UndoTask()
    {
        ShapeLoader.main.shapeHolder.SendElementToDeletionLimbo(createdElement);
        ShapeElementRegistry.main.UnregisterShapeElement(createdElement);
        ShapeHolder.CurrentLoadedShape.RemoveRootShapeElement(createdElement);

        //Restore the root element children.
        for (int i = 0; i < rootElemUIDs.Length; i++)
        {
            ShapeElement root = ShapeElementRegistry.main.GetShapeElementByUID(rootElemUIDs[i]);
            root.StepParentName = rootElemStepParentCodes[i];
            root.OriginPoint = rootElemOriginPoints[i];
            root.RemoveParent();
            ShapeHolder.CurrentLoadedShape.AddRootShapeElement(root);
        }

        //Refresh the model order.
        MultiShapeManager.main.RecalculateAndApplyTransformsForAllObjects();

        //We recreate the entire element list for easy usage.
        ElementHierarchyManager.ElementHierarchy.StartCreatingElementPrefabs(ShapeHolder.CurrentLoadedShape);
    }

    public override VSEditMode GetRequiredEditMode()
    {
        return VSEditMode.Model;
    }

    public override string GetTaskName()
    {
        return "Create New Origin Element";
    }

    public override bool MergeTasksIfPossible(IEditTask nextTask)
    {
        return false;
    }

    

}
