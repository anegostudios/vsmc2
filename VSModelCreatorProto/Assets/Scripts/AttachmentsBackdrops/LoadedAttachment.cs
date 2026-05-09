using System.IO;
using UnityEditor;
using UnityEngine;

namespace VSMC
{
    /// <summary>
    /// A loaded attachment, loaded by the <see cref="AttachmentManager"/>.
    /// This holds the attachment data, the loaded shape data and the Unity gameobjects for the shape.
    /// </summary>
    public class LoadedAttachment
    {

        public BackdropOrAttachmentData data;
        public Shape loadedAttachmentShape;
        public AttachmentHolder attachmentHolder;
        private string storedFilepath;

        public LoadedAttachment(BackdropOrAttachmentData data, AttachmentHolder createdHolder)
        {
            this.data = data;
            this.attachmentHolder = createdHolder;
            LoadAttachmentFromShapeFile();
        }

        public void SetBackdropFilePath(string filepath)
        {
            LoadAttachmentFromShapeFile();
        }

        public void LoadAttachmentFromShapeFile()
        {
            string path = AssetPathManager.main.GetFullFilePathFromLocal(data.shapeFilepath + ".json", "shapes");
            if (!File.Exists(path))
            {
                return;
            }
            storedFilepath = path;
            loadedAttachmentShape = ShapeAccessor.DeserializeShapeFromFile(path, true);
            //Check for stepparents in the root elements.
            foreach (ShapeElement e in loadedAttachmentShape.Elements)
            {
                e.SearchForStepParentInShape(loadedAttachmentShape);
            }
            attachmentHolder.OnAttachmentShapeLoaded(loadedAttachmentShape, data);
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