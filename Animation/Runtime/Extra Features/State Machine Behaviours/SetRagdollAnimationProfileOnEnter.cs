using UnityEngine;

namespace Hairibar.Ragdoll.Animation
{
    public class SetRagdollAnimationProfileOnEnter : SetRagdollProfileOnEnter<RagdollAnimationProfile>
    {
        RagdollAnimator ragdollAnimator;

        protected override void GatherDependenciesIfNecessary(Animator animator)
        {
            if (!ragdollAnimator)
            {
                ragdollAnimator = animator.GetComponent<RagdollAnimator>();
            }
        }

        protected override void SetProfile(RagdollAnimationProfile profile)
        {
            ragdollAnimator.Profile = profile;
        }
    }
}
