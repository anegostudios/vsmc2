namespace VSMC
{
    public class TaskChangeTextureBasePath : IEditTask
    {
        string newPath;
        string oldPath;

        public TaskChangeTextureBasePath(string newTextureBasePath)
        {
            newPath = newTextureBasePath;
            oldPath = TextureManager.main.textureBasePath;
        }

        public override void DoTask()
        {
            TextureManager.main.ChangeTextureBasePath(newPath);
            TextureManager.main.overlay.RefreshIfOpen();
        }

        public override void UndoTask()
        {
            TextureManager.main.ChangeTextureBasePath(oldPath);
            TextureManager.main.overlay.RefreshIfOpen();
        }


        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

        public override long GetSizeOfTaskInBytes()
        {
            //Roughly...
            return sizeof(char) * (newPath.Length + oldPath.Length);
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Texture;
        }

        public override string GetTaskName()
        {
            return "Change Texture Base Path";
        }
    }
}