using System.Collections.Generic;
using UnityEngine;

namespace Hairibar.Ragdoll.Animation
{
    /// <summary>
    /// Sets a bone's target position to follow and object.
    /// </summary>
    [AddComponentMenu("Ragdoll/Set Limb Position"), RequireComponent(typeof(RagdollAnimator))]
    public class SetLimbPosition : MonoBehaviour, ITargetPoseModifier
    {
        public BoneName bone;
        public Transform target;

        public void ModifyPose(IEnumerable<RagdollAnimator.AnimatedPair> pairs)
        {
            foreach (RagdollAnimator.AnimatedPair pair in pairs)
            {
                if (pair.Name == bone)
                {
                    pair.currentPose.worldPosition = target.position;
                }
            }
        }

        public void Initialize(IEnumerable<RagdollAnimator.AnimatedPair> pairs)
        {

        }
    }
}
