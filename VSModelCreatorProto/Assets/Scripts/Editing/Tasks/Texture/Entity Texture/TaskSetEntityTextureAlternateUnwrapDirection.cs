using UnityEngine;
namespace VSMC
{
    public class TaskSetEntityTextureAlternateUnwrapDirection : IEditTask
    {

        public int elemUID;
        public int newAlternateDirection;
        public int oldAlternateDirection;

        public TaskSetEntityTextureAlternateUnwrapDirection(ShapeElement elem, bool newAlternateDirection)
        {
            elemUID = elem.elementUID;
            this.newAlternateDirection = newAlternateDirection ? 1 : 0;
            this.oldAlternateDirection = elem.entityTextureUnwrapRotationIndex;
        }

        public override void DoTask()
        {
            ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elemUID);
            elem.entityTextureUnwrapRotationIndex = newAlternateDirection;
            elem.ResolveUVForFaces();
            UVLayoutManager.main.RecalculateUVPositionsForSingleElement(elem);
        }

        public override void UndoTask()
        {
            ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elemUID);
            elem.entityTextureUnwrapRotationIndex = oldAlternateDirection;
            elem.ResolveUVForFaces();
            UVLayoutManager.main.RecalculateUVPositionsForSingleElement(elem);
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Texture;
        }

        public override string GetTaskName()
        {
            return "Set Entity Texture Alternate Unwrap Direction";
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

        public override long GetSizeOfTaskInBytes()
        {
            return sizeof(int) * 3;
        }

    }
}