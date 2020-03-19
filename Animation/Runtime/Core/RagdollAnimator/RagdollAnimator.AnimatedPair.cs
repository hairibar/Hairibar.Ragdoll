using UnityEngine;

namespace Hairibar.Ragdoll.Animation
{
    public partial class RagdollAnimator
    {
        public class AnimatedPair
        {
            public BoneName Name => bonePair.RagdollBone.Name;
            public RagdollBone RagdollBone => bonePair.RagdollBone;
            public Transform TargetBone => bonePair.TargetBone;

            public AnimatedPose currentPose;
            internal AnimatedPose previousPose;

            internal Vector3 poseLinearVelocity;
            internal Vector3 poseAngularVelocity;

            readonly RagdollBoneTargetBonePair bonePair;


            internal void UpdateVelocities(float dt)
            {
                if (dt > 0)
                {
                    poseLinearVelocity = CalculateLinearVelocity(previousPose, currentPose, dt);
                    poseAngularVelocity = CalculateAngularVelocity(previousPose, currentPose, dt);
                }
            }

            static Vector3 CalculateLinearVelocity(AnimatedPose previousPose, AnimatedPose newPose, float dt)
            {
                return (newPose.worldPosition - previousPose.worldPosition) / dt;
            }

            static Vector3 CalculateAngularVelocity(AnimatedPose previousPose, AnimatedPose newPose, float dt)
            {
                Quaternion deltaRotation = newPose.localRotation * Quaternion.Inverse(previousPose.localRotation);
                deltaRotation.ToAngleAxis(out float deltaAngle, out Vector3 axis);

                if (deltaAngle > 180)
                {
                    deltaAngle -= 360f;
                }

                return Mathf.Deg2Rad * deltaAngle / dt * axis.normalized;
            }


            internal AnimatedPair(RagdollBoneTargetBonePair bonePair)
            {
                this.bonePair = bonePair;
            }
        }
    }
}