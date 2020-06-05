using NaughtyAttributes;
using UnityEngine;

#pragma warning disable 649
namespace Hairibar.Ragdoll.Animation
{
    /// <summary>
    /// Matches a target rig's animation by applying appropiate forces to a ragdoll.
    /// </summary>
    [AddComponentMenu("Ragdoll/Ragdoll Animator"), DisallowMultipleComponent]
    public partial class RagdollAnimator : MonoBehaviour
    {
        #region Public API
        public RagdollAnimationProfile Profile
        {
            get => currentProfile;
            set
            {
                RagdollProfile.ValidateAsArgument(value, Bindings.Definition, true, "Tried to set a null AnimationProfile at RagdollAnimator.");

                if (Application.isPlaying)
                {
                    TransitionTo(value);
                }
                else
                {
                    currentProfile = value;
                }
            }
        }

        public float ProfileTransitionLength
        {
            get => _profileTransitionLength;
            set => _profileTransitionLength = Mathf.Max(0, value);
        }

        public RagdollSettings RagdollSettings { get; private set; }
        public RagdollDefinitionBindings Bindings => _ragdollBindings;

        public float MasterAlpha
        {
            get => _masterAlpha;
            set => _masterAlpha = Mathf.Clamp01(value);
        }
        public float MasterDampingRatio
        {
            get => _masterDampingRatio;
            set => _masterDampingRatio = Mathf.Clamp01(value);
        }
        #endregion

        #region Serialized Fields
        [SerializeField] RagdollDefinitionBindings _ragdollBindings;

        [SerializeField, UsePropertySetter("Profile")] RagdollAnimationProfile currentProfile;

        [SerializeField] float _masterAlpha = 1;
        [SerializeField] float _masterDampingRatio = 1;
        [SerializeField] float _profileTransitionLength = 1;
        #endregion

        #region Private State
        ValueTransitioner profileTransitioner;
        RagdollAnimationProfile previousProfile;

        TargetToRagdollMapper mapper;
        AnimatedPair[] animatedPairs;

        ITargetPoseModifier[] targetPoseModifiers;
        IBoneProfileModifier[] boneProfileModifiers;
        #endregion

        #region Unity Update Messages
        void FixedUpdate()
        {
            if (!isActiveAndEnabled || animatedPairs is null) return;

            ModifyTargetPose();
            DoAnimationMatching();
        }

        void LateUpdate()
        {
            if (!isActiveAndEnabled || animatedPairs is null) return;

            ReadAnimatedPose();

#if UNITY_EDITOR
            if (!forceAnimatedPose) mapper.MapTargetToRagdoll();
#else
            mapper.MapTargetToRagdoll();
#endif
        }
        #endregion

        #region Lifetime
        void Awake()
        {
            if (!_ragdollBindings)
            {
                throw new UnassignedReferenceException("A RagdollDefinitionBindings must be assigned in RagdollAnimator.");
            }

            RagdollProfile.ValidateAsInspectorField(currentProfile, Bindings.Definition, true, "A RagdollAnimationProfile must be assigned at RagdollAnimator.");

            RagdollSettings = _ragdollBindings.GetComponent<RagdollSettings>();
        }

        void Start()
        {
            CreateTargetToRagdollMapper();
            CreateAnimatedPairs(mapper.BonePairs);

            ForceAnimatorUpdate();
            ReadAnimatedPose();
            InitializePreviousPosesWithCurrentPose();

            GatherBoneProfileModifiers();
            InitializeBoneProfileModifiers(boneProfileModifiers, animatedPairs);

            GatherTargetPoseModifiers();
            InitializeTargetPoseModifiers(targetPoseModifiers, animatedPairs);

            InitializeProfileTransitioning();

            SnapToTargetPose();
        }

        void OnEnable()
        {
            SnapToTargetPose();
        }

        void OnDisable()
        {
            UnpowerAllJoints();
        }
        #endregion
    }
}