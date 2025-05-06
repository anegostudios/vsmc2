
using System;
using System.Collections.Generic;
using UnityEngine;

namespace VSMC
{
    public abstract class AnimatorBase : IAnimator
    {
        protected int activeAnimCount = 0;
        public ShapeElement[] RootElements;
        public List<ElementPose> RootPoses;
        public RunningAnimation[] anims;

        public Matrix4x4[] TransformationMatrices = new Matrix4x4[GlobalConstants.MaxAnimatedElements];
        public Matrix4x4[] TransformationMatricesDefaultPose = new Matrix4x4[GlobalConstants.MaxAnimatedElements];

        //Although we're only going to play one animation at a time...
        public RunningAnimation[] CurAnims = new RunningAnimation[20];

        public bool CalculateMatrices { get; set; } = true;

        public Matrix4x4[] Matrices
        {
            get
            {
                return activeAnimCount > 0 ? TransformationMatrices : TransformationMatricesDefaultPose;
            }
        }

        public int ActiveAnimationCount
        {
            get { return activeAnimCount; }
        }

        public RunningAnimation[] Animations => anims;

        public abstract int MaxJointId { get; }

        public RunningAnimation GetAnimationState(string code)
        {
            for (int i = 0; i < anims.Length; i++)
            {
                RunningAnimation anim = anims[i];

                if (anim.Animation.Code.Equals(code, StringComparison.OrdinalIgnoreCase))
                {
                    return anim;
                }
            }
            return null;
        }

        public AnimatorBase(Animation[] Animations)
        {
            anims = new RunningAnimation[Animations == null ? 0 : Animations.Length];

            for (int i = 0; i < anims.Length; i++)
            {
                Animations[i].Code = Animations[i].Code.ToLower();

                anims[i] = new RunningAnimation()
                {
                    Active = false,
                    Running = false,
                    Animation = Animations[i],
                    CurrentFrame = 0
                };
            }

            for (int i = 0; i < TransformationMatricesDefaultPose.Length; i++)
            {
                TransformationMatricesDefaultPose[i] = Matrix4x4.identity;
            }
        }

        public virtual void OnFrame(Dictionary<string, AnimationMetaData> activeAnimationsByAnimCode, float dt)
        {
            activeAnimCount = 0;

            for (int i = 0; i < anims.Length; i++)
            {
                RunningAnimation anim = anims[i];

                activeAnimationsByAnimCode.TryGetValue(anim.Animation.Code, out AnimationMetaData animData);

                bool wasActive = anim.Active;
                anim.Active = animData != null;

                // Animation got started
                if (!wasActive && anim.Active)
                {
                    AnimNowActive(anim, animData);
                }

                // Animation got stopped
                if (wasActive && !anim.Active)
                {
                    if (anim.Animation.OnActivityStopped == EnumEntityActivityStoppedHandling.Rewind)
                    {
                        anim.ShouldRewind = true;
                    }

                    if (anim.Animation.OnActivityStopped == EnumEntityActivityStoppedHandling.Stop)
                    {
                        anim.Stop();
                        activeAnimationsByAnimCode.Remove(anim.Animation.Code);
                        onAnimationStoppedListener?.Invoke(anim.Animation.Code);
                    }

                    if (anim.Animation.OnActivityStopped == EnumEntityActivityStoppedHandling.PlayTillEnd)
                    {
                        anim.ShouldPlayTillEnd = true;
                    }
                }


                if (!anim.Running)
                {
                    continue;
                }

                bool shouldStop =
                    (anim.Iterations > 0 && anim.Animation.OnAnimationEnd == EnumEntityAnimationEndHandling.Stop) ||
                    (anim.Iterations > 0 && !anim.Active && (anim.Animation.OnActivityStopped == EnumEntityActivityStoppedHandling.PlayTillEnd || anim.Animation.OnActivityStopped == EnumEntityActivityStoppedHandling.EaseOut) && anim.EasingFactor < 0.002f) ||
                    (anim.Iterations > 0 && (anim.Animation.OnAnimationEnd == EnumEntityAnimationEndHandling.EaseOut) && anim.EasingFactor < 0.002f) ||
                    (anim.Iterations < 0 && !anim.Active && anim.Animation.OnActivityStopped == EnumEntityActivityStoppedHandling.Rewind && anim.EasingFactor < 0.002f)
                ;

                if (shouldStop)
                {
                    anim.Stop();
                    if (anim.Animation.OnAnimationEnd == EnumEntityAnimationEndHandling.Stop || anim.Animation.OnAnimationEnd == EnumEntityAnimationEndHandling.EaseOut)
                    {
                        activeAnimationsByAnimCode.Remove(anim.Animation.Code);
                        onAnimationStoppedListener?.Invoke(anim.Animation.Code);
                    }
                    continue;
                }

                CurAnims[activeAnimCount] = anim;

                if ((anim.Animation.OnAnimationEnd == EnumEntityAnimationEndHandling.Hold && anim.Iterations != 0 && !anim.Active) || (anim.Animation.OnAnimationEnd == EnumEntityAnimationEndHandling.EaseOut && anim.Iterations != 0))
                {
                    anim.EaseOut(dt);
                }

                anim.Progress(dt, (float)walkSpeed);

                activeAnimCount++;
            }


            calculateMatrices(dt);
        }

    }
}