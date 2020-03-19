using UnityEngine;

namespace Hairibar.Ragdoll.Animation
{
    public partial class RagdollAnimator
    {
        public struct AnimatedPose
        {
            public Vector3 worldPosition;

            public Quaternion worldRotation;
            public Quaternion localRotation;


            public static AnimatedPose Read(Transform transform)
            {
                return new AnimatedPose
                {
                    worldPosition = transform.position,
                    worldRotation = transform.rotation,
                    localRotation = transform.localRotation
                };
            }
        }
    }
}