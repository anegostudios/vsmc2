using System;
using UnityEngine;

namespace VSMC
{
    public class TaskChangeTextureCode : IEditTask
    {
        public int textureIndex;
        public string oldCode;
        public string newCode;

        public TaskChangeTextureCode(int texIndex, string newCode)
        {
            this.textureIndex = texIndex;
            this.newCode = newCode;
            this.oldCode = TextureManager.main.loadedTextures[textureIndex].code;
        }

        public override void DoTask()
        {
            TextureManager.main.ChangeTextureCode(TextureManager.main.loadedTextures[textureIndex], newCode);
            TextureManager.main.overlay.RefreshIfOpen();

        }

        public override void UndoTask()
        {
            TextureManager.main.ChangeTextureCode(TextureManager.main.loadedTextures[textureIndex], oldCode);
            TextureManager.main.overlay.RefreshIfOpen();

        }


        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

        public override long GetSizeOfTaskInBytes()
        {
            return sizeof(int) + sizeof(char) * (oldCode.Length + newCode.Length);
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Texture;
        }

        public override string GetTaskName()
        {
            return "Change Texture Code";
        }
    }
}