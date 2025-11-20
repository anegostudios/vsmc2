namespace VSMC
{
    public class TaskSetFaceUV : IEditTask
    {
        public int elemUID;
        public bool[] selFaces;
        public int uvIndex;
        public float oldUv;
        public float newUv;

        public TaskSetFaceUV(ShapeElement elem, bool[] selFaces, int uvIndex, float newUV)
        {
            this.elemUID = elem.elementUID;
            this.uvIndex = uvIndex;
            this.selFaces = new bool[6];
            selFaces.CopyTo(this.selFaces, 0);

            for (int i = 0; i < 6; i++)
            {
                if (selFaces[i]) oldUv = elem.FacesResolved[i].Uv[uvIndex];
            }

            this.newUv = newUV;
        }

        public override void DoTask()
        {
            ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elemUID);
            for (int i = 0; i < 6; i++)
            {
                //Do not act on non-selected faces.
                if (!selFaces[i]) continue;
                elem.FacesResolved[i].Uv[uvIndex] = newUv;
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
                elem.FacesResolved[i].Uv[uvIndex] = oldUv;
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
            return sizeof(int) * 2 + sizeof(bool) * selFaces.Length + sizeof(float) * 2;
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Texture;
        }

        public override string GetTaskName()
        {
            return "Set Face UV";
        }
    }
}