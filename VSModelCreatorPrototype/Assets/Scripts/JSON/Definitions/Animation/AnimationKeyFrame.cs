using Newtonsoft.Json;
using System.Collections.Generic;

namespace VSMC
{
    [System.Serializable]
    public class AnimationKeyFrame
    {

        // <summary>
        /// The ID of the keyframe.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public int Frame;

        /// <summary>
        /// The elements of the keyframe.
        /// </summary>
        [JsonProperty]
        public Dictionary<string, AnimationKeyFrameElement> Elements;

        IDictionary<ShapeElement, AnimationKeyFrameElement> ElementsByShapeElement;
        
        
        /// <summary>
        /// Resolves the keyframe animation for which elements are important.
        /// </summary>
        /// <param name="allElements"></param>
        public void Resolve(Dictionary<string, ShapeElement> allElements)
        {
            if (Elements == null) return;

            ElementsByShapeElement = new Dictionary<ShapeElement, AnimationKeyFrameElement>(Elements.Count);
            foreach (var val in Elements)
            {
                AnimationKeyFrameElement kelem = val.Value;
                kelem.Frame = Frame;
                allElements.TryGetValue(val.Key, out ShapeElement elem);
                if (elem != null) ElementsByShapeElement[elem] = kelem;
            }
        }

        internal AnimationKeyFrameElement GetKeyFrameElement(ShapeElement forElem)
        {
            if (forElem == null) return null;
            ElementsByShapeElement.TryGetValue(forElem, out AnimationKeyFrameElement kelem);
            return kelem;
        }

    }
}
