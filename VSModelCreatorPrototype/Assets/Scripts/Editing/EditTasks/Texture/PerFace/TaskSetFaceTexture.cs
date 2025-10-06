using System.Linq;
using UnityEngine;
namespace VSMC
{
    public class TaskSetFaceTexture : IEditTask
    {

        public int elemUID;
        public bool[] selFaces;
        public string[] oldTextures;
        public string newTexture;

        public TaskSetFaceTexture(ShapeElement elem, bool[] selFaces, string newTexture)
        {
            this.elemUID = elem.elementUID;
            this.selFaces = new bool[6];
            selFaces.CopyTo(this.selFaces, 0);

            oldTextures = new string[6];
            for (int i = 0; i < 6; i++)
            {
                oldTextures[i] = elem.FacesResolved[i].ResolvedTexture;
            }

            this.newTexture = newTexture;
        }

        public override void DoTask()
        {
            ShapeElement elem = ShapeElementRegistry.main.GetShapeElementByUID(elemUID);
            for (int i = 0; i < 6; i++)
            {
                //Do not act on non-selected faces.
                if (!selFaces[i]) continue;
                elem.FacesResolved[i].Texture = "#" + newTexture;
                elem.FacesResolved[i].ResolveTexture(TextureManager.main.loadedTextures);
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
                elem.FacesResolved[i].Texture = "#" + oldTextures[i];
                elem.FacesResolved[i].ResolveTexture(TextureManager.main.loadedTextures);
            }
            elem.RecreateObjectMesh();
        }

        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

        public override long GetSizeOfTaskInBytes()
        {
            return sizeof(int) + sizeof(bool) * selFaces.Length + sizeof(char) * (newTexture.Length + oldTextures.Sum(x => x.Length));
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Texture;
        }

        public override string GetTaskName()
        {
            return "Set Face Texture";
        }
    }
}