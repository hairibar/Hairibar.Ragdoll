using UnityEngine;

namespace Hairibar.Ragdoll.Animation
{
    /// <summary>
    /// Animation parameters to be used by RagdollAnimator.
    /// </summary>
    [System.Serializable]
    public struct BoneProfile
    {
        public float positionAlpha;
        public float positionDampingRatio;
        public float maxLinearAcceleration;

        public float rotationAlpha;
        public float rotationDampingRatio;
        public float maxAngularAcceleration;


        public static BoneProfile Blend(BoneProfile a, BoneProfile b, float t)
        {
            return new BoneProfile
            {
                positionAlpha = BlendAlpha(a.positionAlpha, b.positionAlpha, t),
                positionDampingRatio = Mathf.Lerp(a.positionDampingRatio, b.positionDampingRatio, t),
                maxLinearAcceleration = BlendMaxAcceleration(a.maxLinearAcceleration, b.maxLinearAcceleration, t),

                rotationAlpha = BlendAlpha(a.rotationAlpha, b.rotationAlpha, t),
                rotationDampingRatio = Mathf.Lerp(a.rotationDampingRatio, b.rotationDampingRatio, t),
                maxAngularAcceleration = BlendMaxAcceleration(a.maxAngularAcceleration, b.maxAngularAcceleration, t)
            };
        }


        /// <summary>
        /// Alpha works best on a squared scale instead of linearly. 
        /// Use this method instead of a lerp for alpha values.
        /// </summary>
        static float BlendAlpha(float a, float b, float t)
        {
            float linearScaleA = Mathf.Sqrt(a);
            float linearScaleB = Mathf.Sqrt(b);

            float blendedLinearScaleValue = Mathf.Lerp(linearScaleA, linearScaleB, t);

            return blendedLinearScaleValue * blendedLinearScaleValue;
        }

        static float BlendMaxAcceleration(float a, float b, float t)
        {
            if (float.IsInfinity(a) || float.IsInfinity(b))
            {
                return Mathf.Infinity;
            }
            else
            {
                return Mathf.Lerp(a, b, t);
            }
        }
    }
}
