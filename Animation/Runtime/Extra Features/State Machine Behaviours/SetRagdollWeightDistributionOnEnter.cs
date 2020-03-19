using UnityEngine;

namespace Hairibar.Ragdoll.Animation
{
    public class SetRagdollWeightDistributionOnEnter : SetRagdollProfileOnEnter<RagdollWeightDistribution>
    {
        RagdollSettings settings;

        protected override void GatherDependenciesIfNecessary(Animator animator)
        {
            if (!settings)
            {
                settings = animator.GetComponent<RagdollAnimator>().RagdollSettings;
            }
        }

        protected override void SetProfile(RagdollWeightDistribution profile)
        {
            settings.WeightDistribution = profile;
        }
    }
}
