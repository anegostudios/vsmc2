using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using UnityEngine;

namespace VSMC
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Shape
    {
        /// <summary>
        /// The collection of textures in the shape. The Dictionary keys are the texture short names, used in each ShapeElementFace
        /// </summary>
        [JsonProperty]
        public Dictionary<string, string> Textures;

        /// <summary>
        /// The elements of the shape.
        /// </summary>
        [JsonProperty]
        public ShapeElement[] Elements;

        /// <summary>
        /// The animations for the shape.
        /// </summary>
        [JsonProperty]
        public Animation[] Animations;

        /// <summary>
        /// The width of the texture. (default: 16)
        /// </summary>
        [JsonProperty]
        public int TextureWidth = 16;

        /// <summary>
        /// The height of the texture (default: 16) 
        /// </summary>
        [JsonProperty]
        public int TextureHeight = 16;

        /// <summary>
        /// The size of each individual texture.
        /// </summary>
        [JsonProperty]
        public Dictionary<string, int[]> TextureSizes = new Dictionary<string, int[]>();
        
        //Texture stuff.
        public static int MaxTextureSize = 512;
        public static string TexturesPath;
        public Vector2[] TextureSizeMultipliers;
        public Texture2DArray loadedTextures;

        //Joints stuff.
        public Dictionary<int, AnimationJoint> JointsById = new Dictionary<int, AnimationJoint>();

        /// <summary>
        /// A rather niave method to load in textures.
        /// Needs a very large update.
        /// </summary>
        /// <param name="context"></param>
        [OnDeserialized()]
        public void ResolveFacesAndTextures(StreamingContext context)
        {
            // This code is testing crap that will be rewritten.
            // We resolve all the textures and whatnot for each element.
            TextureSizeMultipliers = new Vector2[Textures.Count];
            foreach (ShapeElement elem in Elements)
            {
                elem.ResolveFacesAndTextures(Textures);
            }

            if (Textures.Count < 1)
            {
                TextureSizeMultipliers = new Vector2[] { Vector2.one };
                return;
            }

            //This actually loads the textures. 
            // Yes, I'm very aware that this is hardcoded to use my own filepaths. Later this will be in a completely seperate class (i.e. TextureManager)
            loadedTextures = new Texture2DArray(MaxTextureSize, MaxTextureSize, Textures.Count, TextureFormat.RGBA32, false);
            loadedTextures.filterMode = FilterMode.Point;
            loadedTextures.wrapMode = TextureWrapMode.Clamp;
            int i = 0;
            foreach (var pair in Textures)
            {

                string path = Path.Combine(TexturesPath, pair.Value + ".png");
                if (File.Exists(path))
                {
                    try
                    {
                        Texture2D load = new Texture2D(0, 0);
                        load.LoadImage(File.ReadAllBytes(path));
                        Debug.Log("Successfully loaded image " + pair.Key);

                        if (TextureSizes.ContainsKey(pair.Key))
                        {
                            TextureSizeMultipliers[i] = new Vector2((float)TextureSizes[pair.Key][0] / load.width, (float)TextureSizes[pair.Key][1] / load.height);
                        }
                        else
                        {
                            TextureSizeMultipliers[i] = new Vector2((float)TextureWidth / load.width, (float)TextureHeight / load.height);
                        }

                        // This may seem to be quite odd code - And again, is going to be rewritten. But...
                        //  The material works by using a Unity texture array - The shader then accesses the textures based on that.
                        //  However, the texture array requires that all its textures are of the same size. BEcause of this, any loaded texture has to be padded to the correct size.
                        //  There is definitely a better way of doing this - Either using a single texture that with custom calculated UVs - Or by using a texture array that recalculates size based on the largest texture.

                        //Also the damn UVs start from bottom-left, whereas texture sizes start from top-left.
                        //Means I can't just set pixels, I have to hack it. It doesn't take too long really.
                        Texture2D created = new Texture2D(MaxTextureSize, MaxTextureSize);
                        for (int x = 0; x < load.width; x++)
                        {
                            for (int y = 0; y < load.height; y++)
                            {
                                created.SetPixel(x, load.height - 1 - y, load.GetPixel(x, y));
                                if (load.GetPixel(x, y).a < 0.5)
                                {
                                    created.SetPixel(x, load.height - 1 - y, new Color(0, 0, 0, 0));
                                }
                            }
                        }
                        created.Apply();

                        //LoadedTextures is the texture array. We set the pixels of texture 'i'.
                        loadedTextures.SetPixels(created.GetPixels(), i);
                    }
                    catch (System.Exception e)
                    {
                        Debug.Log("Failed to load image " + pair.Key);
                        Debug.LogException(e);
                    }
                }
                i++;
            }
            loadedTextures.Apply();
        }

        public Dictionary<string, ShapeElement> CollectAndResolveReferences(string shapeNameForLogging)
        {
            Dictionary<string, ShapeElement> elementsByName = new Dictionary<string, ShapeElement>();
            var Elements = this.Elements;
            CollectElements(Elements, elementsByName);

            var Animations = this.Animations;   // iterating a local reference to an array is quicker, as optimiser then knows the array object is unchanging
            if (Animations != null)
            {
                for (int i = 0; i < Animations.Length; i++)
                {
                    Animation anim = Animations[i];
                    var KeyFrames = anim.KeyFrames;
                    for (int j = 0; j < KeyFrames.Length; j++)
                    {
                        AnimationKeyFrame keyframe = KeyFrames[j];
                        ResolveReferences(shapeNameForLogging, elementsByName, keyframe);

                        foreach (AnimationKeyFrameElement kelem in keyframe.Elements.Values)
                        {
                            kelem.Frame = keyframe.Frame;
                        }
                    }

                    if (anim.Code == null || anim.Code.Length == 0)
                    {
                        anim.Code = anim.Name.ToLowerInvariant().Replace(" ", "");
                    }
                }
            }

            for (int i = 0; i < Elements.Length; i++)
            {
                Elements[i].ResolveReferncesAndUIDs();
            }

            return elementsByName;
        }


        private AnimationKeyFrame getOrCreateKeyFrame(Animation entityAnim, int frame)
        {
            for (int ei = 0; ei < entityAnim.KeyFrames.Length; ei++)
            {
                var entityKeyFrame = entityAnim.KeyFrames[ei];
                if (entityKeyFrame.Frame == frame)
                {
                    return entityKeyFrame;
                }
            }

            for (int ei = 0; ei < entityAnim.KeyFrames.Length; ei++)
            {
                var entityKeyFrame = entityAnim.KeyFrames[ei];
                if (entityKeyFrame.Frame > frame)
                {
                    var kfm = new AnimationKeyFrame() { Frame = frame, Elements = new Dictionary<string, AnimationKeyFrameElement>() };
                    entityAnim.KeyFrames = entityAnim.KeyFrames.InsertAt(kfm, ei);
                    return kfm;
                }
            }

            var kf = new AnimationKeyFrame() { Frame = frame, Elements = new Dictionary<string, AnimationKeyFrameElement>() };
            entityAnim.KeyFrames = entityAnim.KeyFrames.InsertAt(kf, 0);
            return kf;
        }

        /// <summary>
        /// Collects all the elements in the shape recursively.
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="elementsByName"></param>
        public void CollectElements(ShapeElement[] elements, IDictionary<string, ShapeElement> elementsByName)
        {
            if (elements == null) return;

            for (int i = 0; i < elements.Length; i++)
            {
                ShapeElement elem = elements[i];

                elementsByName[elem.Name] = elem;

                CollectElements(elem.Children, elementsByName);
            }
        }

        public void ResolveAndFindJoints(string shapeName, Dictionary<string, ShapeElement> elementsByName, params string[] requireJointsForElements)
        {
            var Animations = this.Animations;
            if (Animations == null) return;

            if (elementsByName == null)
            {
                elementsByName = new Dictionary<string, ShapeElement>(Elements.Length);
                CollectElements(Elements, elementsByName);
            }

            int jointCount = 0;

            HashSet<string> AnimatedElements = new HashSet<string>();

            HashSet<string> animationCodes = new HashSet<string>();

            int version = -1;
            bool errorLogged = false;

            for (int i = 0; i < Animations.Length; i++)
            {
                Animation anim = Animations[i];

                if (!animationCodes.Add(anim.Code))
                {
                    Debug.LogWarningFormat("Shape {0}: Two or more animations use the same code '{1}'. This will lead to undefined behavior.", shapeName, anim.Code);
                }

                if (version == -1) version = anim.Version;
                else if (version != anim.Version)
                {
                    if (!errorLogged) Debug.LogErrorFormat("Shape {0} has mixed animation versions. This will cause incorrect animation blending.", shapeName);
                    errorLogged = true;
                }

                var KeyFrames = anim.KeyFrames;
                for (int j = 0; j < KeyFrames.Length; j++)
                {
                    AnimationKeyFrame kf = KeyFrames[j];
                    foreach (var key in kf.Elements.Keys) AnimatedElements.Add(key);
                    kf.Resolve(elementsByName);
                }
            }

            foreach (ShapeElement elem in elementsByName.Values)
            {
                elem.JointId = 0;
            }

            int maxDepth = 0;

            foreach (string code in AnimatedElements)
            {
                elementsByName.TryGetValue(code, out ShapeElement elem);
                if (elem == null) continue;
                AnimationJoint joint = new AnimationJoint() { JointId = ++jointCount, Element = elem };
                JointsById[jointCount] = joint;

                maxDepth = Math.Max(maxDepth, elem.CountParents());
            }

            // Currently used to require a joint for the head for head control, but not really used because
            // the player head also happens to be using in animations so it has a joint anyway
            foreach (string elemName in requireJointsForElements)
            {
                if (!AnimatedElements.Contains(elemName))
                {
                    ShapeElement elem = GetElementByName(elemName);
                    if (elem == null) continue;

                    AnimationJoint joint = new AnimationJoint() { JointId = ++jointCount, Element = elem };
                    JointsById[joint.JointId] = joint;
                    maxDepth = Math.Max(maxDepth, elem.CountParents());
                }
            }

            // Iteratively and recursively assign the lowest depth to highest depth joints to all elements
            // prevents that we overwrite a child joint id with a parent joint id
            for (int depth = 0; depth <= maxDepth; depth++)
            {
                foreach (AnimationJoint joint in JointsById.Values)
                {
                    if (joint.Element.CountParents() != depth) continue;

                    joint.Element.SetJointIdRecursive(joint.JointId);
                }
            }
        }

        /// <summary>
        /// Recursively searches the element by name from the shape.
        /// </summary>
        /// <param name="name">The name of the element to get.</param>
        /// <returns>The shape element or null if none was found</returns>
        public ShapeElement GetElementByName(string name)
        {
            if (Elements == null) return null;

            return GetElementByName(name, Elements);
        }

        ShapeElement GetElementByName(string name, ShapeElement[] elems)
        {
            foreach (ShapeElement elem in elems)
            {
                if (elem.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) return elem;
                if (elem.Children != null)
                {
                    ShapeElement foundElem = GetElementByName(name, elem.Children);
                    if (foundElem != null) return foundElem;
                }
            }

            return null;
        }

        /// <summary>
        /// Calculates and stores the inverse transforms, for animation purposes.
        /// </summary>
        public void CacheInvTransforms() => CacheInvTransforms(Elements);

        /// <summary>
        /// Calculates and stores the inverse transforms, for animation purposes.
        /// </summary>
        /// <param name="elements"></param>
        public static void CacheInvTransforms(ShapeElement[] elements)
        {
            if (elements == null) return;

            for (int i = 0; i < elements.Length; i++)
            {
                elements[i].CacheInverseTransformMatrix();
                CacheInvTransforms(elements[i].Children);
            }
        }

        /// <summary>
        /// Initializes the shape and elements for use with animation.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="shapeNameForLogging"></param>
        /// <param name="requireJointsForElements"></param>
        public void InitForAnimations(string shapeNameForLogging, params string[] requireJointsForElements)
        {
            Dictionary<string, ShapeElement> elementsByName = CollectAndResolveReferences(shapeNameForLogging);
            CacheInvTransforms();
            ResolveAndFindJoints(shapeNameForLogging, elementsByName, requireJointsForElements);
        }

        private void ResolveReferences(string shapeName, Dictionary<string, ShapeElement> elementsByName, AnimationKeyFrame kf)
        {
            if (kf?.Elements == null) return;

            foreach (var val in kf.Elements)
            {
                if (elementsByName.TryGetValue(val.Key, out ShapeElement elem))
                {
                    val.Value.ForElement = elem;
                }
                else
                {
                    Debug.LogErrorFormat("Shape {0} has a key frame element for which the referencing shape element {1} cannot be found.", shapeName, val.Key);
                    val.Value.ForElement = new ShapeElement();
                }
            }
        }

    }
}