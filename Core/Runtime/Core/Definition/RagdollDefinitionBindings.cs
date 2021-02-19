using System;
using System.Collections.Generic;
using System.Linq;
using Hairibar.EngineExtensions.Serialization;
using UnityEngine;

#pragma warning disable 649
namespace Hairibar.Ragdoll
{
    /// <summary>
    /// Links a RagdollDefinition's bone names to the actual GameObjects that represent them.
    /// If using this class at Edit Time, use SubscribeToOnBonesCreated for initialization.
    /// </summary>
    [AddComponentMenu("Ragdoll/Ragdoll Definition Bindings")]
    [DisallowMultipleComponent, ExecuteAlways]
    public class RagdollDefinitionBindings : MonoBehaviour
    {
        #region Public API
        public bool IsInitialized { get; private set; }

        public RagdollDefinition Definition => _definition;

        public RagdollBone Root
        {
            get
            {
                ThrowExceptionIfNotInitialized();

                if (TryGetBone(_definition.Root, out RagdollBone rootBone))
                {
                    return rootBone;
                }
                else
                {
                    throw new InvalidOperationException("There is no root bone.");
                }
            }
        }

        public IEnumerable<RagdollBone> Bones
        {
            get
            {
                ThrowExceptionIfNotInitialized();

                return bones?.Values ?? Array.Empty<RagdollBone>() as IEnumerable<RagdollBone>;
            }
        }

        public bool TryGetBone(BoneName boneName, out RagdollBone bone)
        {
            ThrowExceptionIfNotInitialized();

            return bones.TryGetValue(boneName, out bone);
        }

        public bool TryGetBoundBoneName(ConfigurableJoint joint, out BoneName boneName)
        {
            ThrowExceptionIfNotInitialized();

            foreach (KeyValuePair<BoneName, ConfigurableJoint> pair in bindings)
            {
                if (pair.Value == joint)
                {
                    boneName = pair.Key;
                    return true;
                }
            }

            boneName = "JointDoesNotBelongToARagdollBone";
            return false;
        }
        #endregion

        #region Initialization Event
        event Action OnBonesCreated;

        /// <summary>
        /// If the Definition is initialized, the action will be instantly called. 
        /// If it isn't yet, it will be called when initialized.
        /// <para>
        /// Only useful for [ExecuteAlways] behaviours. At runtime, either the bones are created before Start(), or they are never created due to invalid settings.
        /// If the definition is changed in the inspector, the event will be called again.
        /// </para>
        /// </summary>
        /// <param name="action"></param>
        public void SubscribeToOnBonesCreated(Action action)
        {
            if (IsInitialized)
            {
                action();
            }

            OnBonesCreated += action;
        }

        public void UnsubscribeFromOnBonesCreated(Action action)
        {
            OnBonesCreated -= action;
        }
        #endregion

        #region Serialized Data
        [SerializeField] BoneJointBindingsDictionary bindings;
        [SerializeField] RagdollDefinition _definition;
        #endregion

        #region Private State
        Dictionary<BoneName, RagdollBone> bones;
        #endregion

        #region Validation
        bool BindingsAreValid
        {
            get
            {
                if (!_definition || bindings == null) return false;
                if (bindings.Count < _definition.BoneCount) return false;
                if (bindings.Any(pair => pair.Value == null)) return false;
                if (bindings.Values.Distinct().Count() != bindings.Values.Count) return false;

                return true;
            }
        }
        bool BindingsMatchExistingBones
        {
            get
            {
                if (bones == null) return false;

                foreach (BoneName boneName in bindings.Keys)
                {
                    bool boneExists = TryGetBone(boneName, out RagdollBone bone);

                    if (!boneExists || bone.Joint != GetBindingJoint(boneName))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        ConfigurableJoint GetBindingJoint(BoneName name)
        {
            return bindings[name];
        }

        void ThrowExceptionIfNotInitialized()
        {
            if (!IsInitialized) throw new InvalidOperationException("Attempted to access a non initialized RagdollDefinitionBindings.");
        }

        void OnValidate()
        {
            if (Application.isPlaying) return;

            if (!IsInitialized || !BindingsMatchExistingBones)
            {
                IsInitialized = TryCreateRagdollBones();
                if (IsInitialized) OnBonesCreated?.Invoke();
            }
        }
        #endregion

        #region Initialization
        void OnEnable()
        {
            //Initialize in OnEnable instead of Awake to support fast enter play mode.
            //When not reloading the scene, [ExecuteAlways] scripts won't have Awake called.
            if (Application.IsPlaying(this) && !_definition)
            {
                enabled = false;
                throw new UnassignedReferenceException("No RagdollDefinition was assigned.");
            }

            if (!IsInitialized)
            {
                IsInitialized = TryCreateRagdollBones();
                if (IsInitialized) OnBonesCreated?.Invoke();
            }
            else
            {
                foreach (RagdollBone bone in Bones)
                {
                    bone.ResetJointAxisOnEnable();
                }
            }
        }

        bool TryCreateRagdollBones()
        {
            if (!BindingsAreValid)
            {
                if (Application.isPlaying) UnityEngine.Debug.LogError("Ragdoll Definition Bindings aren't correctly set up.", this);
                return false;
            }

            bones = new Dictionary<BoneName, RagdollBone>();

            //Create the bones
            foreach (BoneName boneName in bindings.Keys)
            {
                ConfigurableJoint joint = bindings[boneName];

                RagdollBone bone = new RagdollBone(boneName, joint.transform, joint.GetComponent<Rigidbody>(), joint, _definition.IsRoot(boneName));
                bones.Add(boneName, bone);
            }

            return true;
        }
        #endregion


        [Serializable]
        class BoneJointBindingsDictionary : SerializableDictionary<BoneName, ConfigurableJoint>
        {

        }
    }
}
