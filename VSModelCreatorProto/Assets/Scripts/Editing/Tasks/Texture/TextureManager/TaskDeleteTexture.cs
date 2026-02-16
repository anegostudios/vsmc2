using UnityEngine;

namespace VSMC
{
    /// <summary>
    /// An undoable task to delete a texture.
    /// </summary>
    public class TaskDeleteTexture : IEditTask
    {

        public LoadedTexture texToDelete;
        public int indexInTextureArray;

        public TaskDeleteTexture(LoadedTexture texToDelete)
        {
            this.texToDelete = texToDelete;
            indexInTextureArray = TextureManager.main.loadedTextures.IndexOf(texToDelete);
        }

        public override void DoTask()
        {
            TextureManager.main.loadedTextures.Remove(texToDelete);
            TextureManager.main.OnTextureArrayModified();
            TextureManager.main.overlay.RefreshIfOpen();
        }

        public override void UndoTask()
        {
            //To undo, we can just remove the new texture.
            TextureManager.main.loadedTextures.Insert(indexInTextureArray, texToDelete);
            TextureManager.main.OnTextureArrayModified();
            TextureManager.main.overlay.RefreshIfOpen();
        }


        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

        public override long GetSizeOfTaskInBytes()
        {
            //This is uh... the loaded texture can store a fair amount of data that can't easily be calculated.
            return sizeof(int);
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Texture;
        }

        public override string GetTaskName()
        {
            return "Delete Texture";
        }

    }
}