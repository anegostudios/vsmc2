using System.Collections.Generic;
using System;
using UnityEngine;

namespace VSMC
{
    /// <summary>
    /// A game script for the animator interface. Just provides features for <see cref="AnimatorBase"/> and <see cref="ClientAnimator"/>.
    /// </summary>
    public interface IAnimator
    {

        int MaxJointId { get; }

        /// <summary>
        /// The 30 pose transformation matrices that go to the shader
        /// </summary>
        Matrix4x4[] Matrices { get; }

        /// <summary>
        /// Amount of currently active animations
        /// </summary>
        int ActiveAnimationCount { get; }

        /// <summary>
        /// Holds data over all animations. This list always contains all animations of the creature. You have to check yourself which of them are active
        /// </summary>
        RunningAnimation[] Animations { get; }

        RunningAnimation GetAnimationState(string code);

        /// <summary>
        /// Whether or not to calculate the animation matrices, required for GetAttachmentPointPose() to deliver correct values. Default on on the client, server side only on when the creature is dead
        /// </summary>
        bool CalculateMatrices { get; set; }

        /// <summary>
        /// Gets the attachment point pose.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        //AttachmentPointAndPose GetAttachmentPointPose(string code);

        ElementPose GetPosebyName(string name, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase);

        /// <summary>
        /// The event fired on each frame. Same functionality but does not use AnimationMetaData.
        /// </summary>
        /// <param name="activeAnimationsByAnimCode"></param>
        /// <param name="dt"></param>
        void OnFrame(Dictionary<string, AnimationMetaData> activeAnimationsByAnimCode, float dt);

        string DumpCurrentState();
        //void ReloadAttachmentPoints();

    }
}