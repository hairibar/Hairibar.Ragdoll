using System.Collections.Generic;

namespace Hairibar.Ragdoll.Animation
{
    /// <summary>
    /// Implement this interface in a component alongside a RagdollAnimator to modify the Bone Profiles right before animation matching is done.
    /// </summary>
    public interface IBoneProfileModifier
    {
        void Modify(ref BoneProfile boneProfile, RagdollAnimator.AnimatedPair pair, float dt);
        void Initialize(IEnumerable<RagdollAnimator.AnimatedPair> pairs);
    }
}