using UnityEngine;

namespace Hairibar.Ragdoll.Animation
{
    internal class RagdollBoneTargetBonePair
    {
        #region Public API
        public RagdollBone RagdollBone { get; }
        public Transform TargetBone { get; }
        #endregion

        internal RagdollBoneTargetBonePair(RagdollBone ragdollBone, Transform targetBone)
        {
            RagdollBone = ragdollBone;
            TargetBone = targetBone;
        }
    }
}