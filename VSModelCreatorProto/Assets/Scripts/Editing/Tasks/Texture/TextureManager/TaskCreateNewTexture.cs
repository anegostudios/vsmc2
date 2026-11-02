using System.IO;
using System.Linq;
using Unity.Collections;

namespace VSMC
{
    /// <summary>
    /// An undoable task to create a new texture.
    /// </summary>
    public class TaskCreateNewTexture : IEditTask
    {
        string newTextureCode;
        string path;

        public TaskCreateNewTexture(string path = "")
        {
            this.path = path;
            string uniqueTexCode = "texture";
            if (path != "")
            {
                uniqueTexCode = Path.GetFileNameWithoutExtension(path);
            }
            while (TextureManager.main.loadedTextures.Any(x => x.code == uniqueTexCode))
            {
                uniqueTexCode = ShapeElement.IncrementName(uniqueTexCode);
            }
            newTextureCode = uniqueTexCode;
        }

        public override void DoTask()
        {
            LoadedTexture newTex = new LoadedTexture(newTextureCode, path);
            newTex.LoadTextureFromCodeAndPath(ShapeHolder.CurrentLoadedShape);
            newTex.ResolveTextureSize(ShapeHolder.CurrentLoadedShape);
            TextureManager.main.loadedTextures.Add(newTex);
            TextureManager.main.RegenerateTextureArray();
            TextureManager.main.overlay.RefreshIfOpen();
            TextureManager.main.overlay.OnTextureSelected(TextureManager.main.overlay.textureImages[TextureManager.main.overlay.textureImages.Length - 1]); //lol this line
            UVLayoutManager.main.RefreshAllUVSpaces(true);
        }

        public override void UndoTask()
        {
            //To undo, we can just remove the new texture.
            TextureManager.main.loadedTextures.RemoveAll(x => x.code == newTextureCode);
            TextureManager.main.RegenerateTextureArray();
            TextureManager.main.overlay.RefreshIfOpen();
            UVLayoutManager.main.RefreshAllUVSpaces(true);
        }


        public override bool MergeTasksIfPossible(IEditTask nextTask)
        {
            return false;
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