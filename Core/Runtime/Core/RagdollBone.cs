using System.Collections.Generic;
using UnityEngine;

namespace Hairibar.Ragdoll
{
    public class RagdollBone
    {
        public PowerSetting PowerSetting
        {
            get => _powerSetting;
            set
            {
                PowerSetting oldValue = _powerSetting;
                _powerSetting = value;

                if (oldValue != value)
                {
                    OnPowerSettingChanged?.Invoke(oldValue, value);
                }
            }
        }
        public delegate void OnPowerSettingChangedHandler(PowerSetting previousSetting, PowerSetting newSetting);
        public event OnPowerSettingChangedHandler OnPowerSettingChanged;

        public BoneName Name { get; }
        public bool IsRoot { get; }

        public Transform Transform { get; }
        public Rigidbody Rigidbody { get; }
        public ConfigurableJoint Joint { get; }

        public IEnumerable<Collider> Colliders { get; }
        public Quaternion StartingJointRotation { get; }


        PowerSetting _powerSetting = PowerSetting.Kinematic;

        #region Initialization
        internal RagdollBone(BoneName name, Transform transform, Rigidbody rigidbody, ConfigurableJoint joint, bool isRoot)
        {
            Name = name;
            Transform = transform;
            Rigidbody = rigidbody;
            Joint = joint;
            IsRoot = isRoot;

            Colliders = GatherColliders();
            ConfigureJoint();
            StartingJointRotation = GetStartingJointRotation();
        }

        Collider[] GatherColliders()
        {
            List<Collider> colliders = new List<Collider>();

            GatherCollidersAtTransform(Transform);
            VisitChildren(Transform);

            return colliders.ToArray();


            void GatherCollidersAtTransform(Transform transform)
            {
                colliders.AddRange(transform.GetComponents<Collider>());
            }

            void VisitChildren(Transform parent)
            {
                for (int i = 0; i < parent.childCount; i++)
                {
                    Transform child = parent.GetChild(i);
                    bool isItsOwnBone = child.GetComponent<ConfigurableJoint>();

                    if (!isItsOwnBone)
                    {
                        GatherCollidersAtTransform(child);
                        VisitChildren(child);
                    }
                }
            }
        }

        void ConfigureJoint()
        {
            Joint.configuredInWorldSpace = IsRoot;
            Joint.rotationDriveMode = RotationDriveMode.Slerp;
        }

        Quaternion GetStartingJointRotation()
        {
            return Joint.configuredInWorldSpace ? Transform.rotation : Transform.localRotation;
        }
        #endregion


        /// <summary>
        /// At OnEnable(), joints do some weird re-configuring. This method, if called from OnEnable(), deals with that.
        /// </summary>
        internal void ResetJointAxisOnEnable()
        {
            // https://forum.unity.com/threads/hinge-joint-limits-resets-on-activate-object.483481/#post-5713138
            Quaternion originalRotation = Transform.localRotation;
            Transform.localRotation = StartingJointRotation;
            Joint.axis = Joint.axis;    // Yes, this is intentional. The axis setter triggers some calculations that we need.
            Transform.localRotation = originalRotation;
        }

        public override string ToString()
        {
            return Name.ToString();
        }
    }
}