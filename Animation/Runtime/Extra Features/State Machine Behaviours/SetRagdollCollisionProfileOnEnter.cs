using UnityEngine;

namespace Hairibar.Ragdoll.Animation
{
    public class SetRagdollCollisionProfileOnEnter : SetRagdollProfileOnEnter<RagdollCollisionProfile>
    {
        RagdollCollisionIgnorer collisionIgnorer;

        protected override void GatherDependenciesIfNecessary(Animator animator)
        {
            if (!collisionIgnorer)
            {
                collisionIgnorer = animator.GetComponent<RagdollAnimator>().Bindings.GetComponent<RagdollCollisionIgnorer>();
            }
        }

        protected override void SetProfile(RagdollCollisionProfile profile)
        {
            collisionIgnorer.CollisionProfile = profile;
        }
    }
}
