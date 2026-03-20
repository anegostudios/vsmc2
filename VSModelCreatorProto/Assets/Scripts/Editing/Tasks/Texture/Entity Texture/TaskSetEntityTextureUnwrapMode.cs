using UnityEngine;
namespace VSMC
{
    public class TaskSetEntityTextureUnwrapMode : IEditTask
    {

        public int elemUID;
        public int newUnwrapMode;
        public int oldUnwrapmode;

        public TaskSetEntityTextureUnwrapMode(ShapeElement elem, int newUnwrapMode)
        {
            elemUID = elem.elementUID;
            this.newUnwrapMode = newUnwrapMode;
            this.oldUnwrapmode = elem.entityTextureUnwrapMode;
        }

        public override void DoTask()
        {
            ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elemUID);
            elem.entityTextureUnwrapMode = newUnwrapMode;
            elem.ResolveUVForFaces();
            UVLayoutManager.main.RecalculateUVPositionsForSingleElement(elem);
        }

        public override void UndoTask()
        {
            ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elemUID);
            elem.entityTextureUnwrapMode = oldUnwrapmode;
            elem.ResolveUVForFaces();
            UVLayoutManager.main.RecalculateUVPositionsForSingleElement(elem);
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Texture;
        }

        public override string GetTaskName()
        {
            return "Set Entity Texture Unwrap Mode";
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