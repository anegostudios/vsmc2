using System.IO;
using UnityEditor;
using UnityEngine;

namespace VSMC
{
    /// <summary>
    /// A loaded backdrop, loaded by the <see cref="BackdropManager"/>.
    /// This holds the backdrop data, the loaded shape data and the Unity gameobjects for the shape.
    /// </summary>
    public class LoadedBackdrop
    {

        //Backdrop step by step:
        //Load model and then resolve stepparenting of current object.
        //  Check all root elements to see any stepparents.

        public BackdropOrAttachmentData data;
        public Shape loadedBackdropShape;
        public BackdropHolder backdropHolder;
        private string storedFilepath;

        public LoadedBackdrop(BackdropOrAttachmentData data, BackdropHolder createdHolder)
        {
            this.data = data;
            this.backdropHolder = createdHolder;
            LoadBackdropFromShapeFile();
        }

        public void SetBackdropFilePath(string filepath)
        {
            LoadBackdropFromShapeFile();
        }

        public void LoadBackdropFromShapeFile()
        {
            //backdropHolder.OnShapeLoaded();
            string path = AssetPathManager.main.GetFullFilePathFromLocal(data.shapeFilepath+".json", "shapes");
            if (!File.Exists(path))
            {
                Debug.LogError("Cannot find file path at " + path);
                return;
            }
            storedFilepath = path;
            loadedBackdropShape = ShapeAccessor.DeserializeShapeFromFile(path, true);
            //Check for stepparents in the root elements.
            foreach (ShapeElement e in loadedBackdropShape.Elements)
            {
                e.SearchForStepParentInShape(loadedBackdropShape);
            }
            backdropHolder.OnBackdropShapeLoaded(loadedBackdropShape, data);
        }

        public void SetBackdropEnabled(bool enabled)
        {
            if (enabled) OnBackdropEnabled();
            if (!enabled) OnBackdropDisabled();
        }

        public void OnBackdropEnabled()
        {
            //Iterate root shape elements to find stepparents, and then retesellate current shape.
            backdropHolder.gameObject.SetActive(true);
            backdropHolder.SearchForStepparents();
        }

        public void OnBackdropDisabled()
        {
            //Iterate root shape elements to ensure there are no stepparents, and then retesellate the current shape.
            backdropHolder.gameObject.SetActive(false);
            backdropHolder.RemoveStepparents();
        }

        public void SetOpacity(float opacity)
        {
            data.opacity = opacity;
            backdropHolder.AssignMaterials(data);
        }

        public void SetHideTextures(bool hideTextures)
        {
            data.hideTextures = hideTextures;
            backdropHolder.AssignMaterials(data);
        }

        public void SetColorIndex(int colorIndex)
        {
            data.flatColorIndex = colorIndex;
            backdropHolder.AssignMaterials(data);
        }

        public void LoadInContext(LoadIntoBackdropContext context)
        {
            if (storedFilepath != null && File.Exists(storedFilepath))
            {
                ShapeLoader.main.LoadShape(storedFilepath, context);
            }
        }

    }
}