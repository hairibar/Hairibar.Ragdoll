using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 649
namespace Hairibar.Ragdoll.Animation
{
    /// <summary>
    /// Defines animation parameters for a Ragdoll. 
    /// </summary>
    [CreateAssetMenu(menuName = "Ragdoll/Animation Profile", fileName = "RAGANIM_New", order = 1)]
    public class RagdollAnimationProfile : RagdollProfile
    {
        #region Serialized Data
        [SerializeField] float globalPositionAlpha = 0.4f;
        [SerializeField] float globalPositionDampingRatio = 0.7f;
        [SerializeField] float globalMaxLinearAcceleration = Mathf.Infinity;

        [SerializeField] BoneProfileOverride[] positionMatchingOverrides;
        [SerializeField] BoneProfileOverride[] rotationMatchingOverrides;

        [SerializeField] float globalRotationAlpha = 0.6f;
        [SerializeField] float globalRotationDampingRatio = 0.7f;
        [SerializeField] float globalMaxAngularAcceleration = Mathf.Infinity;
        [SerializeField] bool matchRootRotation = true;
        #endregion

        #region Private State
        Dictionary<BoneName, BoneProfile> overridenProfiles;
        #endregion

        #region API
        public override bool IsCompatibleWith(RagdollDefinition otherDefinition)
        {
            if (!definition) return true;
            else return base.IsCompatibleWith(otherDefinition);
        }

        /// <summary>
        /// Gets the animator profile for the bone. 
        /// If it has no overrides, the default profile will be returned; even if the bone isn't in the definition.
        /// </summary>
        public BoneProfile GetBoneProfile(BoneName boneName, bool isRoot)
        {
            bool overrideExists = overridenProfiles.TryGetValue(boneName, out BoneProfile returnedProfile);

            if (!overrideExists)
            {
                returnedProfile = DefaultBoneProfile;
            }

            if (isRoot && !matchRootRotation)
            {
                returnedProfile.rotationAlpha = 0;
            }

            return returnedProfile;
        }
        #endregion

        BoneProfile DefaultBoneProfile => new BoneProfile
        {
            positionAlpha = globalPositionAlpha,
            positionDampingRatio = globalPositionDampingRatio,
            maxLinearAcceleration = globalMaxLinearAcceleration,
            rotationAlpha = globalRotationAlpha,
            rotationDampingRatio = globalRotationDampingRatio,
            maxAngularAcceleration = globalMaxAngularAcceleration
        };

        void BuildOverridenBoneProfiles()
        {
            overridenProfiles = new Dictionary<BoneName, BoneProfile>();

            foreach (BoneProfileOverride posOverride in positionMatchingOverrides)
            {
                BoneProfile profile = DefaultBoneProfile;
                posOverride.ApplyToPositionMatching(ref profile);
                overridenProfiles[posOverride.bone] = profile;
            }

            foreach (BoneProfileOverride rotOverride in rotationMatchingOverrides)
            {
                if (!overridenProfiles.TryGetValue(rotOverride.bone, out BoneProfile profile))
                {
                    profile = DefaultBoneProfile;
                }

                rotOverride.ApplyToRotationMatching(ref profile);
                overridenProfiles[rotOverride.bone] = profile;
            }
        }

        #region Unity Messages
        void OnEnable()
        {
            BuildOverridenBoneProfiles();
        }

        //ScriptableObject.Reset() sort-of-but-not-really exists. It doesn't seem to be callable. However, it's documented from 2020.1 onwards. Maybe it will be usable then?
        //Keeping this here for future-proofing.
#pragma warning disable IDE0051 // Remove unused private members
        void Reset()
#pragma warning restore IDE0051 // Remove unused private members
        {
            globalPositionAlpha = 0.4f;
            globalRotationAlpha = 0.6f;
            globalPositionDampingRatio = 0.7f;
            globalRotationDampingRatio = 0.7f;
            globalMaxAngularAcceleration = Mathf.Infinity;

            positionMatchingOverrides = System.Array.Empty<BoneProfileOverride>();
            rotationMatchingOverrides = System.Array.Empty<BoneProfileOverride>();

            if (Application.isPlaying) BuildOverridenBoneProfiles();
        }
        #endregion

        [System.Serializable]
        public struct BoneProfileOverride
        {
            public BoneName bone;
            public float alpha;
            public float dampingRatio;

            internal void ApplyToPositionMatching(ref BoneProfile boneProfile)
            {
                boneProfile.positionAlpha = alpha;
                boneProfile.positionDampingRatio = dampingRatio;
            }

            internal void ApplyToRotationMatching(ref BoneProfile boneProfile)
            {
                boneProfile.rotationAlpha = alpha;
                boneProfile.rotationDampingRatio = dampingRatio;
            }
        }
    }
}
