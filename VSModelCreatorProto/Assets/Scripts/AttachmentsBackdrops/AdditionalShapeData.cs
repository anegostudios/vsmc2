using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using UnityEngine;
namespace VSMC
{
    /// <summary>
    /// Editor-only data stored for an additional shape. 
    /// </summary>
    [System.Serializable]
    public class AdditionalShapeData
    {
        /// <summary>
        /// The filepath to the additional shape, relative to the found assets folder.
        /// </summary>
        public string shapeFilepath;

        /// <summary>
        /// Each root element in a file is a single attachment. This is the root shape element for this attachment.
        /// </summary>
        public string rootShapeElementCode;

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

        /// <summary>
        /// If false, the shape will play its animations purely based on the current animation code of what its attached to.
        /// If true, then the user can select animation to play.
        /// </summary>
        public bool useUniqueAnimations = false;

        /// <summary>
        /// The compressed form of the playback mapping that gets saved into the JSON file.
        /// Saved as an int array to significantly save on space.
        /// </summary>
        [JsonProperty("animationPlaybackMapping")]
        protected int[] animationPlaybackMappingForJson;

        /// <summary>
        /// The runtime form of the animation playback. Maps an anim code from the editing file to an anim code in the loaded backdrop.
        /// </summary>
        [NonSerialized()]
        public Dictionary<string, string> animationPlaybackMapping;

        public AdditionalShapeData()
        {
        }

        public AdditionalShapeData(string path)
        {
            this.shapeFilepath = path;
            this.opacity = 1;
            this.enabled = true;
        }

        public void ResolveBeforeSerialization(Shape loadedShape, Shape backdropOrAttachmentShape)
        {
            List<int> valueMapping = new List<int>();
            foreach (KeyValuePair<string, string> map in animationPlaybackMapping)
            {
                int p1 = loadedShape.Animations.IndexOf(x => x.Code.Equals(map.Key, StringComparison.CurrentCultureIgnoreCase));
                int p2 = backdropOrAttachmentShape.Animations.IndexOf(x => x.Code.Equals(map.Value, StringComparison.CurrentCultureIgnoreCase));
                if (p1 == -1 || p2 == -1) continue; //Can't find the animations - Likely means they have been removed.
                valueMapping.Add(p1);
                valueMapping.Add(p2);
            }
            animationPlaybackMappingForJson = valueMapping.ToArray();
        }
    }
}