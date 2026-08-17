using System.Collections.Generic;
using UnityEngine;

namespace VSMC
{
    public class TaskImportShape : IEditTask
    {

        public class ImportData
        {
            public List<ShapeElement> importedElements;
            public List<LoadedTexture> importedTextures;
            public List<Animation> importedAnimations;
        }

        public ImportData importedStuff;

        public TaskImportShape(string filePath)
        {
            Shape importShape = ShapeAccessor.DeserializeShapeFromFile(filePath, JSONStreamingContexts.import);
            importedStuff = ShapeHolder.CurrentLoadedShape.MergeWithOtherShape(importShape);

            
            foreach (ShapeElement e in ShapeHolder.CurrentLoadedShape.Elements)
            {
                e.SearchForStepParentInShape(ShapeHolder.CurrentLoadedShape);
            }
            ShapeTesselator.TesselateShape(ShapeHolder.CurrentLoadedShape);
            foreach (ShapeElement e in importShape.Elements)
            {
                ShapeLoader.main.shapeHolder.CreateShapeElementGameObject(e, true);
            }

            foreach (ShapeElement elem in importedStuff.importedElements)
            {
                ShapeElementRegistry.main.UnregisterShapeElement(elem, true);
                ShapeLoader.main.shapeHolder.SendElementToDeletionLimbo(elem, true);
                ShapeHolder.CurrentLoadedShape.RemoveRootShapeElement(elem);
            }

            foreach (LoadedTexture t in importedStuff.importedTextures)
            {
                TextureManager.main.loadedTextures.Remove(t);
            }

            foreach (Animation a in importedStuff.importedAnimations)
            {
                ShapeHolder.CurrentLoadedShape.Animations = ShapeHolder.CurrentLoadedShape.Animations.Remove(a);
            }
        }

        public override void DoTask()
        {
            foreach (ShapeElement elem in importedStuff.importedElements)
            {
                ShapeElementRegistry.main.ReregisterShapeElement(elem, true);
                ShapeHolder.CurrentLoadedShape.AddRootShapeElement(elem);
                ShapeLoader.main.shapeHolder.RestoreElementFromDeletionLimbo(elem, true);
            }
            foreach (LoadedTexture t in importedStuff.importedTextures)
            {
                TextureManager.main.loadedTextures.Add(t);
            }

            foreach (Animation a in importedStuff.importedAnimations)
            {
                ShapeHolder.CurrentLoadedShape.Animations = ShapeHolder.CurrentLoadedShape.Animations.Append(a);
            }
            ShapeLoader.main.hierarchy.StartCreatingElementPrefabs(ShapeHolder.CurrentLoadedShape);
            EditModeManager.main.RefreshMode();
        }

        public override void UndoTask()
        {
            foreach (ShapeElement elem in importedStuff.importedElements)
            {
                ShapeElementRegistry.main.UnregisterShapeElement(elem, true);
                ShapeLoader.main.shapeHolder.SendElementToDeletionLimbo(elem, true);
                ShapeHolder.CurrentLoadedShape.RemoveRootShapeElement(elem);
            }

            foreach (LoadedTexture t in importedStuff.importedTextures)
            {
                TextureManager.main.loadedTextures.Remove(t);
            }

            foreach (Animation a in importedStuff.importedAnimations)
            {
                ShapeHolder.CurrentLoadedShape.Animations = ShapeHolder.CurrentLoadedShape.Animations.Remove(a);
            }
            ShapeLoader.main.hierarchy.StartCreatingElementPrefabs(ShapeHolder.CurrentLoadedShape);
            EditModeManager.main.RefreshMode();
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.None;
        }

        public override long GetSizeOfTaskInBytes()
        {
            return 0;
        }

        public override string GetTaskName()
        {
            return "Import Shape";
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }
    }
}