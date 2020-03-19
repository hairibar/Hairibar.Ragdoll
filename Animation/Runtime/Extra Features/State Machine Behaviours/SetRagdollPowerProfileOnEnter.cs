using UnityEngine;

namespace Hairibar.Ragdoll.Animation
{
    public class SetRagdollPowerProfileOnEnter : SetRagdollProfileOnEnter<RagdollPowerProfile>
    {
        RagdollSettings settings;

        protected override void GatherDependenciesIfNecessary(Animator animator)
        {
            if (!settings)
            {
                settings = animator.GetComponent<RagdollAnimator>().RagdollSettings;
            }
        }

        protected override void SetProfile(RagdollPowerProfile profile)
        {
            settings.PowerProfile = profile;
        }
    }
}
