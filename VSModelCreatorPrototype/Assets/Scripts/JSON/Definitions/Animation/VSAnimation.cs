using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class VSAnimation
{
    [JsonProperty]
    public int QuantityFrames;

    [JsonProperty]
    public string Name;

    [JsonProperty]
    public string Code;

    [JsonProperty]
    public int Version;

    [JsonProperty]
    public bool EaseAnimationSpeed = false;

    [JsonProperty]
    public VSAnimationKeyFrame[] KeyFrames;

    //[JsonProperty]
    //public EnumEntityActivityStoppedHandling OnActivityStopped = EnumEntityActivityStoppedHandling.Rewind;

    //[JsonProperty]
    //public EnumEntityAnimationEndHandling OnAnimationEnd = EnumEntityAnimationEndHandling.Repeat;]

    /// <summary>
    /// Compiles the animation into a bunch of matrices, 31 matrices per frame.
    /// </summary>
    /// <param name="rootElements"></param>
    /// <param name="jointsById"></param>
    /// <param name="recursive">When false, will only do root elements</param>
    public void GenerateAllFrames(ShapeElementJSON[] rootElements, Dictionary<int, VSAnimationJoint> jointsById, bool recursive = true)
    {
        for (int i = 0; i < rootElements.Length; i++)
        {
            rootElements[i].CacheInverseTransformMatrixRecursive();
        }

        VSAnimationFrame[] resolvedKeyFrames = new VSAnimationFrame[KeyFrames.Length];

        for (int i = 0; i < resolvedKeyFrames.Length; i++)
        {
            resolvedKeyFrames[i] = new VSAnimationFrame() { FrameNumber = KeyFrames[i].Frame };
        }

        if (KeyFrames.Length == 0)
        {
            throw new Exception("VSAnimation '" + Code + "' has no keyframes, this will cause other errors every time it is ticked");
        }

        if (jointsById.Count >= GlobalConstants.MaxAnimatedElements)
        {
            if (GlobalConstants.MaxAnimatedElements < 46 && jointsById.Count <= 46) throw new Exception("Max joint cap of " + GlobalConstants.MaxAnimatedElements + " reached, needs to be at least " + jointsById.Count + ". In clientsettings.json, please try increasing the \"maxAnimatedElements\": setting to 46.  This works for most GPUs.  Otherwise you might need to disable the creature.");
            throw new Exception("A mod's entity has " + jointsById.Count + " VSAnimation joints which exceeds the max joint cap of " + GlobalConstants.MaxAnimatedElements + ". Sorry, you'll have to either disable this creature or simplify the model.");
        }

        for (int i = 0; i < resolvedKeyFrames.Length; i++)
        {
            jointsDone.Clear();
            GenerateFrame(i, resolvedKeyFrames, rootElements, jointsById, Mat4f.Create(), resolvedKeyFrames[i].RootElementTransforms, recursive);
        }

        for (int i = 0; i < resolvedKeyFrames.Length; i++)
        {
            resolvedKeyFrames[i].FinalizeMatrices(jointsById);
        }

        PrevNextKeyFrameByFrame = new VSAnimationFrame[QuantityFrames][];
        for (int i = 0; i < QuantityFrames; i++)
        {
            getLeftRightResolvedFrame(i, resolvedKeyFrames, out VSAnimationFrame left, out VSAnimationFrame right);

            PrevNextKeyFrameByFrame[i] = new VSAnimationFrame[] { left, right };
        }


    }



    protected void GenerateFrame(int indexNumber, VSAnimationFrame[] resKeyFrames, ShapeElementJSON[] elements, Dictionary<int, VSAnimationJoint> jointsById, float[] modelMatrix, List<ElementPose> transforms, bool recursive = true)
    {
        int frameNumber = resKeyFrames[indexNumber].FrameNumber;

        if (frameNumber >= QuantityFrames) throw new InvalidOperationException("Invalid VSAnimation '" + Code + "'. Has QuantityFrames set to " + QuantityFrames + " but a key frame at frame " + frameNumber + ". QuantityFrames always must be higher than frame number");

        for (int i = 0; i < elements.Length; i++)
        {
            ShapeElementJSON element = elements[i];

            ElementPose animTransform = new ElementPose();
            animTransform.ForElement = element;

            GenerateFrameForElement(frameNumber, element, ref animTransform);
            transforms.Add(animTransform);

            float[] animModelMatrix = Mat4f.CloneIt(modelMatrix);
            Mat4f.Mul(animModelMatrix, animModelMatrix, element.GetLocalTransformMatrix(Version, null, animTransform));

            if (element.JointId > 0 && !jointsDone.Contains(element.JointId))
            {
                resKeyFrames[indexNumber].SetTransform(element.JointId, animModelMatrix);
                jointsDone.Add(element.JointId);
            }

            if (recursive && element.Children != null)
            {
                GenerateFrame(indexNumber, resKeyFrames, element.Children, jointsById, animModelMatrix, animTransform.ChildElementPoses);
            }
        }
    }



    protected void GenerateFrameForElement(int frameNumber, ShapeElementJSON element, ref ElementPose transform)
    {
        for (int flag = 0; flag < 3; flag++)
        {

            getTwoKeyFramesElementForFlag(frameNumber, element, flag, out VSAnimationKeyFrameElement curKelem, out VSAnimationKeyFrameElement nextKelem);

            if (curKelem == null) continue;


            float t;

            if (nextKelem == null || curKelem == nextKelem)
            {
                nextKelem = curKelem;
                t = 0;
            }
            else
            {
                if (nextKelem.Frame < curKelem.Frame)
                {
                    int quantity = nextKelem.Frame + (QuantityFrames - curKelem.Frame);
                    int framePos = GameMath.Mod(frameNumber - curKelem.Frame, QuantityFrames);

                    t = (float)framePos / quantity;
                }
                else
                {
                    t = (float)(frameNumber - curKelem.Frame) / (nextKelem.Frame - curKelem.Frame);
                }
            }


            lerpKeyFrameElement(curKelem, nextKelem, flag, t, ref transform);

            transform.RotShortestDistanceX = curKelem.RotShortestDistanceX;
            transform.RotShortestDistanceY = curKelem.RotShortestDistanceY;
            transform.RotShortestDistanceZ = curKelem.RotShortestDistanceZ;
        }
    }


    protected void lerpKeyFrameElement(VSAnimationKeyFrameElement prev, VSAnimationKeyFrameElement next, int forFlag, float t, ref ElementPose transform)
    {
        if (prev == null && next == null) return;

        // Applies the transforms in model space
        if (forFlag == 0)
        {
            transform.translateX = GameMath.Lerp((float)prev.OffsetX / 16f, (float)next.OffsetX / 16f, t);
            transform.translateY = GameMath.Lerp((float)prev.OffsetY / 16f, (float)next.OffsetY / 16f, t);
            transform.translateZ = GameMath.Lerp((float)prev.OffsetZ / 16f, (float)next.OffsetZ / 16f, t);
        }
        else if (forFlag == 1)
        {
            transform.degX = GameMath.Lerp((float)prev.RotationX, (float)next.RotationX, t);
            transform.degY = GameMath.Lerp((float)prev.RotationY, (float)next.RotationY, t);
            transform.degZ = GameMath.Lerp((float)prev.RotationZ, (float)next.RotationZ, t);
        }
        else
        {
            transform.scaleX = GameMath.Lerp((float)prev.StretchX, (float)next.StretchX, t);
            transform.scaleY = GameMath.Lerp((float)prev.StretchY, (float)next.StretchY, t);
            transform.scaleZ = GameMath.Lerp((float)prev.StretchZ, (float)next.StretchZ, t);
        }
    }




    protected void getTwoKeyFramesElementForFlag(int frameNumber, ShapeElementJSON forElement, int forFlag, out VSAnimationKeyFrameElement left, out VSAnimationKeyFrameElement right)
    {
        left = null;
        right = null;

        int rightKfIndex = seekRightKeyFrame(frameNumber, forElement, forFlag);
        if (rightKfIndex == -1) return;

        right = KeyFrames[rightKfIndex].GetKeyFrameElement(forElement);

        int leftKfIndex = seekLeftKeyFrame(rightKfIndex, forElement, forFlag);
        if (leftKfIndex == -1)
        {
            left = right;
            return;
        }

        left = KeyFrames[leftKfIndex].GetKeyFrameElement(forElement);
    }


    private int seekRightKeyFrame(int aboveFrameNumber, ShapeElementJSON forElement, int forFlag)
    {
        int firstIndex = -1;

        for (int i = 0; i < KeyFrames.Length; i++)
        {
            VSAnimationKeyFrame keyframe = KeyFrames[i];

            VSAnimationKeyFrameElement kelem = keyframe.GetKeyFrameElement(forElement);
            if (kelem != null && kelem.IsSet(forFlag))
            {
                if (firstIndex == -1) firstIndex = i;
                if (keyframe.Frame <= aboveFrameNumber) continue;

                return i;
            }
        }

        return firstIndex;
    }

    private int seekLeftKeyFrame(int leftOfKeyFrameIndex, ShapeElementJSON forElement, int forFlag)
    {
        for (int i = 0; i < KeyFrames.Length; i++)
        {
            int index = GameMath.Mod(leftOfKeyFrameIndex - i - 1, KeyFrames.Length);
            VSAnimationKeyFrame keyframe = KeyFrames[index];

            VSAnimationKeyFrameElement kelem = keyframe.GetKeyFrameElement(forElement);
            if (kelem != null && kelem.IsSet(forFlag))
            {
                return index;
            }
        }

        return -1;
    }


    protected void getLeftRightResolvedFrame(int frameNumber, VSAnimationFrame[] frames, out VSAnimationFrame left, out VSAnimationFrame right)
    {
        left = null;
        right = null;

        // Go left of frameNumber until we hit the first keyframe
        int keyframeIndex = frames.Length - 1;
        bool loopAround = false;

        while (keyframeIndex >= -1)
        {
            VSAnimationFrame keyframe = frames[GameMath.Mod(keyframeIndex, frames.Length)];
            keyframeIndex--;

            if (keyframe.FrameNumber <= frameNumber || loopAround)
            {
                left = keyframe;
                break;
            }

            if (keyframeIndex == -1) loopAround = true;
        }


        keyframeIndex += 2;
        VSAnimationFrame nextkeyframe = frames[GameMath.Mod(keyframeIndex, frames.Length)];
        right = nextkeyframe;
        return;
    }

    public VSAnimation Clone()
    {
        return new VSAnimation()
        {
            Code = Code,
            CodeCrc32 = CodeCrc32,
            EaseVSAnimationSpeed = EaseVSAnimationSpeed,
            jointsDone = jointsDone,
            KeyFrames = CloneKeyFrames(),
            Name = Name,
            OnActivityStopped = OnActivityStopped,
            OnVSAnimationEnd = OnVSAnimationEnd,
            QuantityFrames = QuantityFrames,
            Version = Version,
        };
    }

    private VSAnimationKeyFrame[] CloneKeyFrames()
    {
        VSAnimationKeyFrame[] elems = new VSAnimationKeyFrame[KeyFrames.Length];

        for (int i = 0; i < KeyFrames.Length; i++)
        {
            elems[i] = KeyFrames[i].Clone();
        }

        return elems;
    }

}
