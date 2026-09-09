using System.IO;
using UnityEditor;
using UnityEngine;

namespace VSMC
{
    /// <summary>
    /// A loaded attachment, loaded by the <see cref="AttachmentManager"/>.
    /// This holds the attachment data, the loaded shape data and the Unity gameobjects for the shape.
    /// </summary>
    public class LoadedAdditionalShape
    {

        public AdditionalShapeData data;
        public Shape loadedAdditionalShape;
        public AdditionalObjectHolder attachmentHolder;
        private string storedFilepath;

        public LoadedAdditionalShape(AdditionalShapeData data, AdditionalObjectHolder createdHolder)
        {
            this.data = data;
            this.attachmentHolder = createdHolder;
            LoadAdditionalShapeFromShapeFile();
        }

        public void SetBackdropFilePath(string filepath)
        {
            LoadAdditionalShapeFromShapeFile();
        }

        public void LoadAdditionalShapeFromShapeFile()
        {
            string path = AssetPathManager.main.GetFullFilePathFromLocal(data.shapeFilepath + ".json", "shapes");
            if (!File.Exists(path))
            {
                return;
            }
            storedFilepath = path;
            loadedAdditionalShape = ShapeAccessor.DeserializeShapeFromFile(path, JSONStreamingContexts.backdropOrAttachment);

            //This is where we want to split the shape into its respective files.
            loadedAdditionalShape.FilterShapeByRootElement(this.data.rootShapeElementCode);
            if (loadedAdditionalShape.Elements.Length != 1) //The attachment couldn't be loaded.
                return;

            attachmentHolder.OnAttachmentShapeLoaded(loadedAdditionalShape, data);
        }

        public void SetAttachmentEnabled(bool enabled)
        {
            if (enabled) OnAttachmentEnabled();
            if (!enabled) OnAttachmentDisabled();
        }

        public void OnAttachmentEnabled()
        {
            //Iterate root shape elements to find stepparents, and then retesellate current shape.
            attachmentHolder.gameObject.SetActive(true);
            attachmentHolder.SearchForStepparents();
        }

        public void OnAttachmentDisabled()
        {
            //Iterate root shape elements to ensure there are no stepparents, and then retesellate the current shape.
            attachmentHolder.gameObject.SetActive(false);
            attachmentHolder.RemoveStepparents();
        }

        public void SetOpacity(float opacity)
        {
            data.opacity = opacity;
            attachmentHolder.AssignMaterials(data);
        }

        public void SetHideTextures(bool hideTextures)
        {
            data.hideTextures = hideTextures;
            attachmentHolder.AssignMaterials(data);
        }

        public void SetColorIndex(int colorIndex)
        {
            data.flatColorIndex = colorIndex;
            attachmentHolder.AssignMaterials(data);
        }

        public void LoadInContext(LoadIntoAttachmentContext context)
        {
            if (storedFilepath != null && File.Exists(storedFilepath))
            {
                ShapeLoader.main.LoadShape(storedFilepath, context);
            }
        }

    }
}