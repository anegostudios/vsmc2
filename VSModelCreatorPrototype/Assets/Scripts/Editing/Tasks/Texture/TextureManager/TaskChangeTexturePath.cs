using System;
using UnityEngine;

namespace VSMC
{
    /// <summary>
    /// An undoable task to change the texture's path.
    /// </summary>
    public class TaskChangeTexturePath : IEditTask
    {
        public int textureIndex;
        public string oldPath;
        public string newPath;

        public TaskChangeTexturePath(int texIndex, string newPath)
        {
            this.textureIndex = texIndex;
            this.newPath = newPath;
            this.oldPath = TextureManager.main.loadedTextures[textureIndex].path;
        }

        public override void DoTask()
        {
            TextureManager.main.ChangeTexturePath(TextureManager.main.loadedTextures[textureIndex], newPath);
            TextureManager.main.overlay.RefreshIfOpen();

        }

        public override void UndoTask()
        {
            TextureManager.main.ChangeTexturePath(TextureManager.main.loadedTextures[textureIndex], oldPath);
            TextureManager.main.overlay.RefreshIfOpen();
        }


        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

        public override long GetSizeOfTaskInBytes()
        {
            return sizeof(int) + sizeof(char) * (oldPath.Length + newPath.Length);
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Texture;
        }

        public override string GetTaskName()
        {
            return "Change Texture Path";
        }
    }
}