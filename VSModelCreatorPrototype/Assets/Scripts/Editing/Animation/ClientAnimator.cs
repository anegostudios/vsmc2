using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace VSMC
{
    public class ClientAnimator : AnimatorBase
    {

        protected HashSet<int> jointsDone = new HashSet<int>();
        public Dictionary<int, AnimationJoint> jointsById;

        public static int MaxConcurrentAnimations = 16;
        int maxDepth;
        List<ElementPose>[][] frameByDepthByAnimation;
        List<ElementPose>[][] nextFrameTransformsByAnimation;
        ShapeElementWeights[][][] weightsByAnimationAndElement;

        Matrix4x4 localTransformMatrix = Matrix4x4.identity;
        Matrix4x4 tmpMatrix = Matrix4x4.identity;

        int[] prevFrame = new int[MaxConcurrentAnimations];
        int[] nextFrame = new int[MaxConcurrentAnimations];
        public override int MaxJointId => jointsById.Count + 1;

        public static ClientAnimator Create(List<ElementPose> rootPoses, Animation[] animations, ShapeElement[] rootElements, Dictionary<int, AnimationJoint> jointsById)
        {
            return new ClientAnimator(
                rootPoses,
                animations,
                rootElements,
                jointsById
            );
        }
         
        public static ClientAnimator Create(Animation[] animations, ShapeElement[] rootElements, Dictionary<int, AnimationJoint> jointsById)
        {
            return new ClientAnimator(
                animations,
                rootElements,
                jointsById
            );
        }

        public ClientAnimator(
            List<ElementPose> rootPoses,
            Animation[] animations,
            ShapeElement[] rootElements,
            Dictionary<int, AnimationJoint> jointsById
        ) : base(animations)
        {
            this.RootElements = rootElements;
            this.jointsById = jointsById;
            this.RootPoses = rootPoses;
            initFields();
        }

        public ClientAnimator(
            Animation[] animations,
            ShapeElement[] rootElements,
            Dictionary<int, AnimationJoint> jointsById
        ) : base(animations)
        {
            this.RootElements = rootElements;
            this.jointsById = jointsById;
            RootPoses = new List<ElementPose>();
            LoadPoses(rootElements, RootPoses);
            initFields();
        }

        protected virtual void initFields()
        {
            maxDepth = 2 + (RootPoses == null ? 0 : getMaxDepth(RootPoses, 1));

            frameByDepthByAnimation = new List<ElementPose>[maxDepth][];
            nextFrameTransformsByAnimation = new List<ElementPose>[maxDepth][];
            weightsByAnimationAndElement = new ShapeElementWeights[maxDepth][][];

            for (int i = 0; i < maxDepth; i++)
            {
                frameByDepthByAnimation[i] = new List<ElementPose>[MaxConcurrentAnimations];
                nextFrameTransformsByAnimation[i] = new List<ElementPose>[MaxConcurrentAnimations];
                weightsByAnimationAndElement[i] = new ShapeElementWeights[MaxConcurrentAnimations][];
            }
        }

        protected virtual void LoadPoses(ShapeElement[] elements, List<ElementPose> intoPoses)
        {
            ElementPose pose;
            for (int i = 0; i < elements.Length; i++)
            {
                ShapeElement elem = elements[i];

                intoPoses.Add(pose = new ElementPose());
                pose.AnimModelMatrix = Matrix4x4.identity;
                pose.ForElement = elem;

                /*
                if (elem.AttachmentPoints != null)
                {
                    for (int j = 0; j < elem.AttachmentPoints.Length; j++)
                    {
                        AttachmentPoint apoint = elem.AttachmentPoints[j];
                        AttachmentPointByCode[apoint.Code] = new AttachmentPointAndPose()
                        {
                            AttachPoint = apoint,
                            CachedPose = pose
                        };
                    }
                }
                */

                if (elem.Children != null)
                {
                    pose.ChildElementPoses = new List<ElementPose>(elem.Children.Length);
                    LoadPoses(elem.Children, pose.ChildElementPoses);
                }
            }
        }

        private int getMaxDepth(List<ElementPose> poses, int depth)
        {
            for (int i = 0; i < poses.Count; i++)
            {
                var pose = poses[i];

                if (pose.ChildElementPoses != null)
                {
                    depth = getMaxDepth(pose.ChildElementPoses, depth);
                }
            }

            return depth + 1;
        }

        public override ElementPose GetPosebyName(string name, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase)
        {
            return getPosebyName(RootPoses, name);
        }

        private ElementPose getPosebyName(List<ElementPose> poses, string name, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase)
        {
            for (int i = 0; i < poses.Count; i++)
            {
                var pose = poses[i];
                if (pose.ForElement.Name.Equals(name, stringComparison)) return pose;

                if (pose.ChildElementPoses != null)
                {
                    var foundPose = getPosebyName(pose.ChildElementPoses, name);
                    if (foundPose != null) return foundPose;
                }
            }

            return null;
        }

        protected override void AnimNowActive(RunningAnimation anim, AnimationMetaData animData)
        {
            base.AnimNowActive(anim, animData);

            if (anim.Animation.PrevNextKeyFrameByFrame == null)
            {
                anim.Animation.GenerateAllFrames(RootElements, jointsById);
            }

            anim.LoadWeights(RootElements);
        }

        public override void OnFrame(Dictionary<string, AnimationMetaData> activeAnimationsByAnimCode, float dt)
        {
            for (int j = 0; j < activeAnimCount; j++)
            {
                RunningAnimation anim = CurAnims[j];
                if (anim.Animation.PrevNextKeyFrameByFrame == null && anim.Animation.KeyFrames.Length > 0)
                {
                    anim.Animation.GenerateAllFrames(RootElements, jointsById);
                }
            }

            base.OnFrame(activeAnimationsByAnimCode, dt);
        }

        /// <summary>
        /// This is the first call to calculate matrices.
        /// This will start the recursive function.
        /// </summary>
        /// <param name="dt"></param>
        protected override void calculateMatrices(float dt)
        {

            if (!CalculateMatrices) return;

            jointsDone.Clear();

            int animVersion = 0;

            for (int j = 0; j < activeAnimCount; j++)
            {
                RunningAnimation anim = CurAnims[j];
                weightsByAnimationAndElement[0][j] = anim.ElementWeights;

                animVersion = Math.Max(animVersion, anim.Animation.Version);

                AnimationFrame[] prevNextFrame = anim.Animation.PrevNextKeyFrameByFrame[(int)anim.CurrentFrame % anim.Animation.QuantityFrames];
                frameByDepthByAnimation[0][j] = prevNextFrame[0].RootElementTransforms;
                prevFrame[j] = prevNextFrame[0].FrameNumber;

                if (anim.Animation.OnAnimationEnd == EnumEntityAnimationEndHandling.Hold && (int)anim.CurrentFrame + 1 == anim.Animation.QuantityFrames)
                {
                    nextFrameTransformsByAnimation[0][j] = prevNextFrame[0].RootElementTransforms;
                    nextFrame[j] = prevNextFrame[0].FrameNumber;
                }
                else
                {
                    nextFrameTransformsByAnimation[0][j] = prevNextFrame[1].RootElementTransforms;
                    nextFrame[j] = prevNextFrame[1].FrameNumber;
                }
            }

            calculateMatrices(
                animVersion,
                dt,
                RootPoses,
                weightsByAnimationAndElement[0],
                Matrix4x4.identity,
                frameByDepthByAnimation[0],
                nextFrameTransformsByAnimation[0],
                0
            );


            for (int jointid = 0; jointid < GlobalConstants.MaxAnimatedElements; jointid++)
            {
                if (jointsById.ContainsKey(jointid)) continue;
                TransformationMatrices[jointid] = Matrix4x4.identity;
            }

            /*
            foreach (var val in AttachmentPointByCode)
            {
                for (int i = 0; i < 16; i++)
                {
                    val.Value.AnimModelMatrix[i] = val.Value.CachedPose.AnimModelMatrix[i];
                }
            }
            */
        }


        private void calculateMatrices(
            int animVersion,
            float dt,
            List<ElementPose> outFrame,
            ShapeElementWeights[][] weightsByAnimationAndElement,
            Matrix4x4 modelMatrix,
            List<ElementPose>[] nowKeyFrameByAnimation,
            List<ElementPose>[] nextInKeyFrameByAnimation,
            int depth
        )
        {
            depth++;
            List<ElementPose>[] nowChildKeyFrameByAnimation = this.frameByDepthByAnimation[depth];
            List<ElementPose>[] nextChildKeyFrameByAnimation = this.nextFrameTransformsByAnimation[depth];
            ShapeElementWeights[][] childWeightsByAnimationAndElement = this.weightsByAnimationAndElement[depth];


            for (int childPoseIndex = 0; childPoseIndex < outFrame.Count; childPoseIndex++)
            {
                ElementPose outFramePose = outFrame[childPoseIndex];
                ShapeElement elem = outFramePose.ForElement;

                outFramePose.SetMat(modelMatrix);
                localTransformMatrix = Matrix4x4.identity;

                outFramePose.Clear();

                float weightSum = 0f; 
                for (int animIndex = 0; animIndex < activeAnimCount; animIndex++)
                {
                    RunningAnimation anim = CurAnims[animIndex];
                    ShapeElementWeights sew = weightsByAnimationAndElement[animIndex][childPoseIndex];

                    if (sew.BlendMode != EnumAnimationBlendMode.Add)
                    {
                        weightSum += sew.Weight * anim.EasingFactor;
                    }
                }

                for (int animIndex = 0; animIndex < activeAnimCount; animIndex++)
                {
                    RunningAnimation anim = CurAnims[animIndex];
                    ShapeElementWeights sew = weightsByAnimationAndElement[animIndex][childPoseIndex];
                    anim.CalcBlendedWeight(weightSum / sew.Weight, sew.BlendMode);

                    ElementPose nowFramePose = nowKeyFrameByAnimation[animIndex][childPoseIndex];
                    ElementPose nextFramePose = nextInKeyFrameByAnimation[animIndex][childPoseIndex];

                    int prevFrame = this.prevFrame[animIndex];
                    int nextFrame = this.nextFrame[animIndex];

                    // May loop around, so nextFrame can be smaller than prevFrame
                    float keyFrameDist = nextFrame > prevFrame ? (nextFrame - prevFrame) : (anim.Animation.QuantityFrames - prevFrame + nextFrame);
                    float curFrameDist = anim.CurrentFrame >= prevFrame ? (anim.CurrentFrame - prevFrame) : (anim.Animation.QuantityFrames - prevFrame + anim.CurrentFrame);

                    float lerp = curFrameDist / keyFrameDist;

                    outFramePose.Add(nowFramePose, nextFramePose, lerp, anim.BlendedWeight);

                    nowChildKeyFrameByAnimation[animIndex] = nowFramePose.ChildElementPoses;
                    childWeightsByAnimationAndElement[animIndex] = sew.ChildElements;

                    nextChildKeyFrameByAnimation[animIndex] = nextFramePose.ChildElementPoses;
                }

                localTransformMatrix = elem.GetLocalTransformMatrix(animVersion, localTransformMatrix, outFramePose);
                outFramePose.AnimModelMatrix *= localTransformMatrix;

                if (elem.JointId > 0 && !jointsDone.Contains(elem.JointId))
                {
                    tmpMatrix = outFramePose.AnimModelMatrix * elem.inverseModelTransform;

                    TransformationMatrices[elem.JointId] = tmpMatrix;
                    jointsDone.Add(elem.JointId);
                }

                if (outFramePose.ChildElementPoses != null)
                {
                    calculateMatrices(
                        animVersion,
                        dt,
                        outFramePose.ChildElementPoses,
                        childWeightsByAnimationAndElement,
                        outFramePose.AnimModelMatrix,
                        nowChildKeyFrameByAnimation,
                        nextChildKeyFrameByAnimation,
                        depth
                    );
                }
            }
        }
    }
}