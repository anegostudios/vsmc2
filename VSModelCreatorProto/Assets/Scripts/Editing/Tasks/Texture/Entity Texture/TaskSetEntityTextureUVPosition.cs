using UnityEngine;
namespace VSMC
{
    public class TaskSetEntityTextureUVPosition : IEditTask
    {

        public int elemUID;
        public Vector2 newUV;
        public Vector2 oldUV;

        public TaskSetEntityTextureUVPosition(ShapeElement elem, Vector2 newUV, Vector2? oldUV = null)
        {
            elemUID = elem.elementUID;
            this.newUV = newUV;
            if (!oldUV.HasValue)
            {
                this.oldUV = new Vector2((float)elem.entityTextureUV[0], (float)elem.entityTextureUV[1]);
            }
            else
            {
                this.oldUV = oldUV.Value;
            }
        }

        public override void DoTask()
        {
            ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elemUID);
            elem.entityTextureUV = new double[] { newUV.x, newUV.y };
            elem.ResolveUVForFaces();
            UVLayoutManager.main.RecalculateUVPositionsForSingleElement(elem);
        }

        public override void UndoTask()
        {
            ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elemUID);
            elem.entityTextureUV = new double[] { oldUV.x, oldUV.y };
            elem.ResolveUVForFaces();
            UVLayoutManager.main.RecalculateUVPositionsForSingleElement(elem);
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Texture;
        }

        public override string GetTaskName()
        {
            return "Set Entity Texture UV";
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

        public override long GetSizeOfTaskInBytes()
        {
            return sizeof(float) * 4;
        }

    }
}