using System;
using UnityEngine;

namespace VSMC
{
    /// <summary>
    /// An undoable task to enable or disable auto resolution for the selected faces.
    /// </summary>
    public class TaskSetFaceSnapUV : IEditTask
    {

        public int elemUID;
        public bool[] selFaces;
        public bool[] oldSnapUV;
        public Vector4[] oldUvs;
        public bool newSnapUV;

        public TaskSetFaceSnapUV(ShapeElement elem, bool[] selFaces, bool newSnapUV)
        {
            this.elemUID = elem.elementUID;
            this.selFaces = (bool[])selFaces.Clone();

            oldSnapUV = new bool[6];
            oldUvs = new Vector4[6];
            for (int i = 0; i < 6; i++)
            {
                ShapeElementFace f = elem.FacesResolved[i];
                oldSnapUV[i] = f.snapUV;
                oldUvs[i] = new Vector4(f.Uv[0], f.Uv[1], f.Uv[2], f.Uv[3]);
            }

            this.newSnapUV = newSnapUV;
        }

        public override void DoTask()
        {
            ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elemUID);
            for (int i = 0; i < 6; i++)
            {
                //Do not act on non-selected faces.
                if (!selFaces[i]) continue;
                elem.FacesResolved[i].snapUV = newSnapUV;
            }
            elem.ResolveUVForFaces();
            elem.RecreateObjectMesh();
            UVLayoutManager.main.RecalculateUVPositionsForSingleElement(elem);
        }

        public override void UndoTask()
        {
            ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elemUID);
            for (int i = 0; i < 6; i++)
            {
                //Do not act on non-selected faces.
                if (!selFaces[i]) continue;
                elem.FacesResolved[i].snapUV = oldSnapUV[i];
                elem.FacesResolved[i].Uv = new float[] { oldUvs[i].x, oldUvs[i].y, oldUvs[i].z, oldUvs[i].w };

            }
            elem.RecreateObjectMesh();
            UVLayoutManager.main.RecalculateUVPositionsForSingleElement(elem);
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

        public override long GetSizeOfTaskInBytes()
        {
            return sizeof(int) + sizeof(bool) * (selFaces.Length + 7) + sizeof(bool) * (6 * 8);
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Texture;
        }

        public override string GetTaskName()
        {
            return "Set Face Auto Resolution";
        }
    }
}