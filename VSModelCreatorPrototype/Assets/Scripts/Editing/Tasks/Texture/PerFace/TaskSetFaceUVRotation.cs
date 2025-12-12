using System.Linq;
using UnityEngine;
namespace VSMC
{
    /// <summary>
    /// An undoable task to set the UV rotation for all selected faces. UV rotation should be 0, 90, 180 or 270.
    /// </summary>
    public class TaskSetFaceUVRotation : IEditTask
    {

        public int elemUID;
        public bool[] selFaces;
        public float[] oldRot;
        public float newRot;

        public TaskSetFaceUVRotation(ShapeElement elem, bool[] selFaces, float newRot)
        {
            this.elemUID = elem.elementUID;
            this.selFaces = (bool[])selFaces.Clone();

            oldRot = new float[6];
            for (int i = 0; i < 6; i++)
            {
                oldRot[i] = elem.FacesResolved[i].Rotation;
            }

            this.newRot = newRot;
        }

        public override void DoTask()
        {
            ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elemUID);
            for (int i = 0; i < 6; i++)
            {
                //Do not act on non-selected faces.
                if (!selFaces[i]) continue;
                elem.FacesResolved[i].Rotation = newRot;
            }
            elem.RecreateObjectMesh();
        }

        public override void UndoTask()
        {
            ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elemUID);
            for (int i = 0; i < 6; i++)
            {
                //Do not act on non-selected faces.
                if (!selFaces[i]) continue;
                elem.FacesResolved[i].Rotation = oldRot[i];
            }
            elem.RecreateObjectMesh();
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

        public override long GetSizeOfTaskInBytes()
        {
            return sizeof(int) + sizeof(bool) * (selFaces.Length) + sizeof(float) * 7;
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Texture;
        }

        public override string GetTaskName()
        {
            return "Set Face UV Rotation";
        }
    }
}