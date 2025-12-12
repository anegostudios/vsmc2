using System.Linq;
using UnityEngine;
namespace VSMC
{
    /// <summary>
    /// An undoable task to enable or disable the selected faces.
    /// </summary>
    public class TaskSetFaceEnabled : IEditTask
    {

        public int elemUID;
        public bool[] selFaces;
        public bool[] oldEnabled;
        public bool newEnabled;

        public TaskSetFaceEnabled(ShapeElement elem, bool[] selFaces, bool newEnabled)
        {
            this.elemUID = elem.elementUID;
            this.selFaces = (bool[])selFaces.Clone();

            oldEnabled = new bool[6];
            for (int i = 0; i < 6; i++)
            {
                oldEnabled[i] = elem.FacesResolved[i].Enabled;
            }

            this.newEnabled = newEnabled;
        }

        public override void DoTask()
        {
            ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elemUID);
            for (int i = 0; i < 6; i++)
            {
                //Do not act on non-selected faces.
                if (!selFaces[i]) continue;
                elem.FacesResolved[i].Enabled = newEnabled;
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
                elem.FacesResolved[i].Enabled = oldEnabled[i];
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
            return sizeof(int) + sizeof(bool) * (selFaces.Length + 7);
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Texture;
        }

        public override string GetTaskName()
        {
            return "Set Face Enabled";
        }
    }
}