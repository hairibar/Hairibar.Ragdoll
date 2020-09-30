using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace Hairibar.Ragdoll
{
    /// <summary>
    /// Implements the collision ignorance rules defined in a RagdollCollisionIgnorer.
    /// </summary>
    [AddComponentMenu("Ragdoll/Ragdoll Collision Ignorer")]
    [RequireComponent(typeof(RagdollDefinitionBindings)), DisallowMultipleComponent]
    public class RagdollCollisionIgnorer : MonoBehaviour
    {
        public RagdollCollisionProfile CollisionProfile
        {
            get => _profile;
            set
            {
                RagdollDefinition definition;
                if (bindings) definition = bindings.Definition;
                else definition = GetComponent<RagdollDefinitionBindings>().Definition;

                RagdollProfile.ValidateAsArgument(value, definition, false);

                if (Application.isPlaying)
                {
                    UnapplyCurrentProfile();
                    UseNewProfile(value);
                }
                else
                {
                    _profile = value;
                }
            }
        }

        [SerializeField, UsePropertySetter("CollisionProfile")]
        RagdollCollisionProfile _profile;

#if UNITY_EDITOR
        //Used to support tweaking of the assigned profile during play mode.
        //The instance is first unapplied, and then the original is reapplied.
        RagdollCollisionProfile profileInstance;
#endif

        RagdollDefinitionBindings bindings;


        void UnapplyCurrentProfile()
        {
#if UNITY_EDITOR
            if (profileInstance)
            {
                ApplyProfile(profileInstance, false);
                Destroy(profileInstance);
            }
#else
            if (_profile)
            {
                ApplyProfile(_profile, false);
            }
#endif
            if (_profile)
            {
                _profile.OnUpdateValues -= OnProfileUpdated;
            }
        }

        void OnProfileUpdated(RagdollCollisionProfile updatedSettings)
        {
            CollisionProfile = updatedSettings;
        }


        void UseNewProfile(RagdollCollisionProfile newProfile)
        {
            if (newProfile)
            {
                _profile = newProfile;
                _profile.OnUpdateValues += OnProfileUpdated;
#if UNITY_EDITOR
                profileInstance = Instantiate(_profile);
                ApplyProfile(profileInstance, true);
#else
                ApplyProfile(newProfile, true);
#endif
            }
        }

        void ApplyProfile(RagdollCollisionProfile profile, bool setToIgnored)
        {
            ApplyIgnoredPairs(profile.IgnoredPairs, setToIgnored);
            ApplyDisabledColliderBones(profile.DisabledColliders, setToIgnored);
        }

        void ApplyIgnoredPairs(IEnumerable<RagdollCollisionProfile.BonePair> ignoredPairs, bool setToIgnored)
        {
            foreach (RagdollCollisionProfile.BonePair pair in ignoredPairs)
            {
                if (!bindings.TryGetBone(pair.boneA, out RagdollBone boneA)) continue;
                if (!bindings.TryGetBone(pair.boneB, out RagdollBone boneB)) continue;

                IEnumerable<Collider> aColliders = boneA.Colliders;
                IEnumerable<Collider> bColliders = boneB.Colliders;

                foreach (Collider colliderA in aColliders)
                {
                    foreach (Collider colliderB in bColliders)
                    {
                        Physics.IgnoreCollision(colliderA, colliderB, setToIgnored);
                    }
                }
            }
        }

        void ApplyDisabledColliderBones(IEnumerable<BoneName> disabledColliderBones, bool setToIgnored)
        {
            foreach (BoneName boneName in disabledColliderBones)
            {
                if (!bindings.TryGetBone(boneName, out RagdollBone bone)) continue;

                bone.Rigidbody.detectCollisions = !setToIgnored;
            }
        }

        #region Initialization
        void Awake()
        {
            bindings = GetComponent<RagdollDefinitionBindings>();

            RagdollProfile.ValidateAsInspectorField(_profile, bindings.Definition, false);
        }

        void Start()
        {
            UseNewProfile(_profile);
        }
        #endregion

        void OnDestroy()
        {
            UnapplyCurrentProfile();
        }
    }
}