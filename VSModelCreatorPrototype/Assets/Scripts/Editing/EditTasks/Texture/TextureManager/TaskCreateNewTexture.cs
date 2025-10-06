namespace VSMC
{
    public class TaskCreateNewTexture : IEditTask
    {
        string newTextureCode;

        public TaskCreateNewTexture()
        {
            string uniqueTexCode = "texture";
            int tryIndex = 0;
            bool validName = false;
            while (!validName)
            {
                tryIndex++;
                validName = true;
                foreach (LoadedTexture tex in TextureManager.main.loadedTextures)
                {
                    if (tex.code == uniqueTexCode + tryIndex) validName = false;
                }
            }
            newTextureCode = uniqueTexCode + tryIndex;
        }

        public override void DoTask()
        {
            LoadedTexture newTex = new LoadedTexture(newTextureCode, "");
            newTex.LoadTextureFromCodeAndPath();
            newTex.ResolveTextureSize(ShapeLoader.main.shapeHolder.cLoadedShape);
            TextureManager.main.loadedTextures.Add(newTex);
            TextureManager.main.RegenerateTextureArray();
            TextureManager.main.overlay.RefreshIfOpen();
            TextureManager.main.overlay.OnTextureSelected(TextureManager.main.overlay.textureImages[TextureManager.main.overlay.textureImages.Length - 1]); //lol this line
        }

        public override void UndoTask()
        {
            //To undo, we can just remove the new texture.
            TextureManager.main.loadedTextures.RemoveAll(x => x.code == newTextureCode);
            TextureManager.main.RegenerateTextureArray();
            TextureManager.main.overlay.RefreshIfOpen();
        }


        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
        }

        public override long GetSizeOfTaskInBytes()
        {
            //Roughly...
            return sizeof(char) * (newTextureCode.Length);
        }

        public override VSEditMode GetRequiredEditMode()
        {
            return VSEditMode.Texture;
        }

        public override string GetTaskName()
        {
            return "Create New Texture";
        }
    }
}