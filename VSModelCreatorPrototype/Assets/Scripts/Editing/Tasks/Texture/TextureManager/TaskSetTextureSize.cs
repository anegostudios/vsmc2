using System;
using UnityEngine;

namespace VSMC
{
    public class TaskSetTextureSize : IEditTask
    {
        public int textureIndex;
        public int oldWidth;
        public int oldHeight;
        public int newWidth;
        public int newHeight;

        public TaskSetTextureSize(int texIndex, int newWidth, int newHeight)
        {
            this.textureIndex = texIndex;
            this.newWidth = newWidth;
            this.newHeight = newHeight;
            this.oldWidth = TextureManager.main.loadedTextures[textureIndex].storedWidth;
            this.oldHeight = TextureManager.main.loadedTextures[textureIndex].storedHeight;
        }

        public override void DoTask()
        {
            TextureManager.main.SetTextureSize(TextureManager.main.loadedTextures[textureIndex], newWidth, newHeight);
            TextureManager.main.overlay.RefreshIfOpen();

        }

        public override void UndoTask()
        {
            TextureManager.main.SetTextureSize(TextureManager.main.loadedTextures[textureIndex], oldWidth, oldHeight);
            TextureManager.main.overlay.RefreshIfOpen();

        }


        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

        public override long GetSizeOfTaskInBytes()
        {
            return sizeof(int) * 5;
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Texture;
        }

        public override string GetTaskName()
        {
            return "Set Texture Size";
        }
    }
}