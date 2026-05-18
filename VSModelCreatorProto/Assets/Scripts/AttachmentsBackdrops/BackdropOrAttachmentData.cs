using System.ComponentModel;
using Newtonsoft.Json;
using UnityEngine;
namespace VSMC
{
    /// <summary>
    /// Editor-only data stored for a backdrop. 
    /// </summary>
    [System.Serializable]
    public class BackdropOrAttachmentData
    {
        /// <summary>
        /// The filepath to the backdrop's shape, relative to the found assets folder.
        /// This is used as the backdrop's ID, too.
        /// </summary>
        public string shapeFilepath;

        /// <summary>
        /// Between 0 and 1, the percentage opacity of the rendered backdrop object.
        /// Make sure to use the opaque material when opacity is one to avoid layering issues.
        /// </summary>
        [DefaultValue(1)]
        public float opacity;

        /// <summary>
        /// Whether to use this backdrop. A disabled backdrop has no impact on the currently loaded shape.
        /// </summary>
        public bool enabled;

        /// <summary>
        /// Should we show the textures for the backdrop?
        /// </summary>
        public bool hideTextures;

        /// <summary>
        /// If showtextures is false, then we use this index to select the render color.
        /// </summary>
        public int flatColorIndex;

        public BackdropOrAttachmentData()
        {
        }

        public BackdropOrAttachmentData(string path)
        {
            this.shapeFilepath = path;
            this.opacity = 1;
            this.enabled = true;
        }
    }
}