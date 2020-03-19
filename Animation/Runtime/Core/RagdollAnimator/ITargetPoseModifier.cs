using System.Collections.Generic;

namespace Hairibar.Ragdoll.Animation
{
    /// <summary>
    /// Implement this interface in a component alongside a RagdollAnimator to be able to modify the target pose right before animation matching is done.
    /// </summary>
    public interface ITargetPoseModifier
    {
        void ModifyPose(IEnumerable<RagdollAnimator.AnimatedPair> pairs);

        void Initialize(IEnumerable<RagdollAnimator.AnimatedPair> pairs);
    }
}
