using UnityEngine;

namespace Hairibar.Ragdoll.Animation
{
    internal static class AnimationMatching
    {
        public static Vector3 GetAcclerationFromPositionSpring(Vector3 currentPos, Vector3 targetPos, Vector3 currentLinearVel, Vector3 targetLinearVel, float alpha, float dampingRatio, float mass, float dt)
        {
            float k = GetSpringStiffnessFromAlpha(alpha, mass, dt);
            float d = GetSpringDampingFromDampingRatio(dampingRatio, k, mass);

            Vector3 positionDifference = currentPos - targetPos;
            Vector3 velocityDifference = currentLinearVel - targetLinearVel;

            Vector3 acceleration = (-k / mass * positionDifference) - (d / mass * velocityDifference);
            return acceleration;
        }

        public static JointDrive GetRotationMatchingJointDrive(float alpha, float dampingRatio, float mass, float dt, float maxAcceleration)
        {
            float k = GetSpringStiffnessFromAlpha(alpha, mass, dt);
            float d = GetSpringDampingFromDampingRatio(dampingRatio, k, mass);

            return new JointDrive()
            {
                positionSpring = k,
                positionDamper = d,
                maximumForce = maxAcceleration * mass
            };
        }


        static float GetSpringStiffnessFromAlpha(float alpha, float mass, float dt)
        {
            return (mass * alpha) / (dt * dt);
        }

        static float GetSpringDampingFromDampingRatio(float dampingRatio, float k, float mass)
        {
            return dampingRatio * (2 * Mathf.Sqrt(k * mass));
        }
    }
}
