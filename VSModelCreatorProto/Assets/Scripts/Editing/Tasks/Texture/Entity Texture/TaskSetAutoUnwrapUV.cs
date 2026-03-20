using UnityEngine;

namespace VSMC
{
    public class TaskSetAutoUnwrapUV : IEditTask
    {

        public int elemUID;
        public bool oldUnwrapUV;
        public bool newUnwrapUV;
        public Vector4[] oldUvs;
        public int[] oldRotationIndices;
        public Vector2 minUVs;
        public Vector2 oldEntityTexUV;

        public TaskSetAutoUnwrapUV(ShapeElement element, bool newUnwrapUV)
        {
            elemUID = element.elementUID;
            this.newUnwrapUV = newUnwrapUV;
            oldUvs = new Vector4[6];
            oldRotationIndices = new int[6];
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            for (int i = 0; i < 6; i++)
            {
                ShapeElementFace f = element.FacesResolved[i];
                oldUvs[i] = new Vector4(f.Uv[0], f.Uv[1], f.Uv[2], f.Uv[3]);
                oldRotationIndices[i] = f.RotationIndex;
                if (f.Enabled)
                {
                    minX = Mathf.Min(minX, f.Uv[0], f.Uv[2]);
                    minY = Mathf.Min(minY, f.Uv[1], f.Uv[3]);
                }
            }
            minUVs = new Vector2(minX, minY);
            if (element.entityTextureUV == null)
            {
                oldEntityTexUV = new Vector2();
            }
            else
            {
                oldEntityTexUV = new Vector2((float)element.entityTextureUV[0], (float)element.entityTextureUV[1]);
            }
        }

        public override void DoTask()
        {
            ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elemUID);
            elem.autoUnwrap = newUnwrapUV;
            elem.entityTextureUV = new double[] { minUVs.x, minUVs.y };
            elem.ResolveUVForFaces();
            UVLayoutManager.main.RecalculateUVPositionsForSingleElement(elem);
        }

        public override void UndoTask()
        {
            ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elemUID);
            elem.autoUnwrap = oldUnwrapUV;
            elem.entityTextureUV = new double[] { oldEntityTexUV.x, oldEntityTexUV.y };
            for (int i = 0; i < 6; i++)
            {
                elem.FacesResolved[i].Uv = new float[] { oldUvs[i].x, oldUvs[i].y, oldUvs[i].z, oldUvs[i].w };
                elem.FacesResolved[i].RotationIndex = oldRotationIndices[i];
            }
            elem.RecreateObjectMesh();
            UVLayoutManager.main.RecalculateUVPositionsForSingleElement(elem);
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Texture;
        }

        public override string GetTaskName()
        {
            return "Set Auto Unwrap";
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

        public override long GetSizeOfTaskInBytes()
        {
            return sizeof(float) * 5 * 6 + sizeof(bool) * 2 + sizeof(int);
        }

    }
}