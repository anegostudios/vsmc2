using UnityEngine;

namespace VSMC
{
    public class RunningAnimation
    {

        public float CurrentFrame;
        public Animation Animation;
        public bool Active;
        public bool Running;
        public int Iterations;

        public bool ShouldRewind = false;
        public bool ShouldPlayTillEnd = false;

        public float AnimProgress => CurrentFrame / (Animation.QuantityFrames - 1);

        public void Progress(float dt, float walkspeed = 1)
        {
            dt *= walkspeed;
            float newFrame = CurrentFrame + 30 * (ShouldRewind ? -dt : dt);

            if (!Active && Animation.OnActivityStopped == EnumEntityActivityStoppedHandling.PlayTillEnd && (Iterations >= 1 || newFrame >= Animation.QuantityFrames - 1))
            {
                CurrentFrame = Animation.QuantityFrames - 1;
                Stop();
                return;
            }

            if ((Animation.OnAnimationEnd == EnumEntityAnimationEndHandling.Hold || Animation.OnAnimationEnd == EnumEntityAnimationEndHandling.EaseOut) && newFrame >= Animation.QuantityFrames - 1 && dt >= 0)
            {
                Iterations = 1;
                CurrentFrame = Animation.QuantityFrames - 1;
                return;
            }

            if ((Animation.OnAnimationEnd == EnumEntityAnimationEndHandling.EaseOut) && newFrame < 0 && dt < 0)
            {
                Iterations = 1;
                CurrentFrame = Animation.QuantityFrames - 1;
                return;
            }


            if (dt >= 0 && newFrame <= 0)
            {
                Iterations--;
                CurrentFrame = 0;
                return;
            }

            CurrentFrame = newFrame;

            if (dt >= 0 && CurrentFrame >= Animation.QuantityFrames) // here and in the modulo used to be a -1 but that skips the last frame (tyron 10dec2020)
            {
                Iterations++;
                CurrentFrame = GameMath.Mod(newFrame, Animation.QuantityFrames);
            }
            if (dt < 0 && CurrentFrame < 0)
            {
                Iterations++;
                CurrentFrame = GameMath.Mod(newFrame, Animation.QuantityFrames);
            }


            if (Animation.OnAnimationEnd == EnumEntityAnimationEndHandling.Stop && Iterations > 0)
            {
                CurrentFrame = Animation.QuantityFrames - 1;
            }

        }

        public void Stop()
        {
            Active = false;
            Running = false;
            CurrentFrame = 0;
            Iterations = 0;
        }



    }
}
