using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace VSMC
{

    /// <summary>
    /// Defines how multiple animations should be blended together.
    /// </summary>
    public enum EnumAnimationBlendMode
    {
        /// <summary>
        /// Add the animation without taking other animations into considerations
        /// </summary>
        Add,
        /// <summary>
        /// Add the pose and average it together with all other running animations with blendmode Average or AddAverage
        /// </summary>
        Average,
        /// <summary>
        /// Add the animation without taking other animations into consideration, but add it's weight for averaging 
        /// </summary>
        AddAverage
    }


    /// <summary>
    /// AnimationMetaData stores information about the playback of an animation, as well as how it blends with other animations.
    /// We likely want to actually create and edit these in the editor for use later on, and it gives us a way of showing blended anims.
    /// </summary>
    public class AnimationMetaData
    {
        /// <summary>
        /// Unique identifier to be able to reference this AnimationMetaData instance
        /// </summary>
        [JsonProperty]
        public string Code;

        /// <summary>
        /// The animations code identifier that we want to play
        /// </summary>
        [JsonProperty]
        public string Animation;

        /// <summary>
        /// The weight of this animation. When using multiple animations at a time, this controls the significance of each animation.
        /// The method for determining final animation values depends on this and <see cref="BlendMode"/>.
        /// </summary>
        [JsonProperty]
        public float Weight = 1f;

        /// <summary>
        /// A way of specifying <see cref="Weight"/> for each element.
        /// Also see <see cref="ElementBlendMode"/> to control blend modes per element..
        /// </summary>
        [JsonProperty]
        public Dictionary<string, float> ElementWeight = new Dictionary<string, float>();

        /// <summary>
        /// The speed this animation should play at.
        /// </summary>
        [JsonProperty]
        public float AnimationSpeed = 1f;

        /// <summary>
        /// Should this animation speed be multiplied by the movement speed of the entity?
        /// </summary>
        [JsonProperty]
        public bool MulWithWalkSpeed = false;

        /// <summary>
        /// This property can be used in cases where a animation with high weight is played alongside another animation with low element weight.
        /// In these cases, the easeIn become unaturally fast. Setting a value of 0.8f or similar here addresses this issue.<br/>
        /// - 0f = uncapped weight<br/>
        /// - 0.5f = weight cannot exceed 2<br/>
        /// - 1f = weight cannot exceed 1
        /// </summary>
        [JsonProperty]
        public float WeightCapFactor = 0f;

        /// <summary>
        /// A multiplier applied to the weight value to "ease in" the animation. Choose a high value for looping animations or it will be glitchy
        /// </summary>
        [JsonProperty]
        public float EaseInSpeed = 10f;

        /// <summary>
        /// A multiplier applied to the weight value to "ease out" the animation. Choose a high value for looping animations or it will be glitchy
        /// </summary>
        [JsonProperty]
        public float EaseOutSpeed = 10f;

        /// <summary>
        /// The animation blend mode. Controls how this animation will react with other concurrent animations.
        /// Also see <see cref="ElementBlendMode"/> to control blend mode per element.
        /// </summary>
        [JsonProperty]
        public EnumAnimationBlendMode BlendMode = EnumAnimationBlendMode.Add;

        /// <summary>
        /// A way of specifying <see cref="BlendMode"/> per element.
        /// </summary>
        [JsonProperty]
        public Dictionary<string, EnumAnimationBlendMode> ElementBlendMode = new Dictionary<string, EnumAnimationBlendMode>(StringComparer.OrdinalIgnoreCase);
        
        /// <summary>
        /// <!--<jsonoptional>Optional</jsonoptional><jsondefault>false</jsondefault>-->
        /// Should this animation stop default animations from playing?
        /// </summary>
        [JsonProperty]
        public bool SupressDefaultAnimation = false;

        public float StartFrameOnce;

        public AnimationMetaData(string metaDataCode, string animationCode)
        {
            Code = metaDataCode;
            Animation = animationCode;
        }

        public string ToJSONString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public float GetCurrentAnimationSpeed(float walkspeed)
        {
            return AnimationSpeed * (MulWithWalkSpeed ? walkspeed : 1);
        }

    }
}
