using UnityEngine;

namespace VSMC
{
    /// <summary>
    /// An undoable task to set all four UV values for all selected faces.
    /// </summary>
    public class TaskSetAllUVs : IEditTask
    {

        public int elemUID;
        public bool[] selFaces;
        public float[] oldUvs;
        public float[] newUvs;

        public TaskSetAllUVs(ShapeElement elem, bool[] selFaces, float[] oldUvs, float[] newUvs)
        {
            this.elemUID = elem.elementUID;
            this.selFaces = (bool[])selFaces.Clone();
            this.oldUvs = (float[])oldUvs.Clone();
            this.newUvs = (float[])newUvs.Clone(); ;
        }

        public override void DoTask()
        {
            ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elemUID);
            for (int i = 0; i < 6; i++)
            {
                //Do not act on non-selected faces.
                if (!selFaces[i]) continue;
                elem.FacesResolved[i].Uv = (float[])newUvs.Clone();
            }
            elem.RecreateObjectMesh();
            UVLayoutManager.main.RefreshAllUVSpaces();
        }

        public override void UndoTask()
        {
            ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elemUID);
            for (int i = 0; i < 6; i++)
            {
                //Do not act on non-selected faces.
                if (!selFaces[i]) continue;
                elem.FacesResolved[i].Uv = (float[])oldUvs.Clone();
            }
            elem.RecreateObjectMesh();
            UVLayoutManager.main.RefreshAllUVSpaces();
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

        public override long GetSizeOfTaskInBytes()
        {
            return sizeof(int) * 2 + sizeof(bool) * selFaces.Length + sizeof(float) * 8;
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Texture;
        }

        public override string GetTaskName()
        {
            return "Modify Face UVs";
        }

    }
}