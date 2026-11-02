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
        public AdditionalShapeHolder attachmentHolder;
        private string storedFilepath;

        public LoadedAdditionalShape(AdditionalShapeData data, AdditionalShapeHolder emptyHolder)
        {
            this.data = data;
            this.attachmentHolder = emptyHolder;
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
            loadedAdditionalShape.shapeLoadedFromPath = data.shapeFilepath;

            //This is where we want to split the shape into its respective files.
            loadedAdditionalShape.FilterShapeByRootElement(data.rootShapeElementCode);
            if (loadedAdditionalShape.Elements.Length != 1) //The attachment couldn't be loaded.
                return;

            if (data.origin != null)
            {
                loadedAdditionalShape.Elements[0].OriginPoint = data.origin.ConvertToOriginPoint();
            }

            loadedAdditionalShape.Elements[0].renderInEditor = data.enabled;
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
        }

        public void OnAttachmentDisabled()
        {
            //Iterate root shape elements to ensure there are no stepparents, and then retesellate the current shape.
            attachmentHolder.gameObject.SetActive(false);
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