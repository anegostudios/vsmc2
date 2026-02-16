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

        /// <summary>
        /// Will get the keyframe element for a code, or will create it and add to the dict if needed.
        /// The animation needs to be refreshed after doing this.
        /// </summary>
        public AnimationKeyFrameElement GetOrCreateKeyFrameElement(string elemCode)
        {
            if (Elements.ContainsKey(elemCode)) return Elements[elemCode];
            AnimationKeyFrameElement e = new AnimationKeyFrameElement();
            Elements[elemCode] = e;
            return e;
        }
        
        public void RemoveAnyEmptyKeyFrameElements()
        {
            List<string> toRemove = new List<string>();
            foreach (KeyValuePair<string, AnimationKeyFrameElement> kv in Elements)
            {
                if (!kv.Value.AnySet) toRemove.Add(kv.Key);
            }
            foreach (string s in toRemove)
            {
                Elements.Remove(s);
            }
        }

    }
}
