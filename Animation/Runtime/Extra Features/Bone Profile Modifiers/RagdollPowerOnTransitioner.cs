using System.Collections.Generic;
using Hairibar.NaughtyExtensions;
using UnityEngine;

namespace Hairibar.Ragdoll.Animation
{
    /// <summary>
    /// Interpolates alpha from 0 to the desired value over a time period when a bone is made powered.
    /// This prevents the bone from snapping into place too quickly.
    /// </summary>
    [AddComponentMenu("Ragdoll/Ragdoll Power On Transitioner"), RequireComponent(typeof(RagdollAnimator))]
    public class RagdollPowerOnTransitioner : MonoBehaviour, IBoneProfileModifier
    {
        public float TransitionLength
        {
            get => _transitionLength;
            set => _transitionLength = Mathf.Max(0, value);
        }


        [SerializeField, UsePropertySetter] float _transitionLength = 0.5f;
        [SerializeField] bool doStartingTransition = true;


        Dictionary<BoneName, ValueTransitioner> transitioners;


        public void Initialize(IEnumerable<RagdollAnimator.AnimatedPair> pairs)
        {
            transitioners = new Dictionary<BoneName, ValueTransitioner>();

            foreach (RagdollAnimator.AnimatedPair pair in pairs)
            {
                InitializePair(pair);
                InitializeTransitioner(transitioners[pair.Name]);
            }
        }

        void InitializePair(RagdollAnimator.AnimatedPair pair)
        {
            ValueTransitioner transitioner = CreateTransitioner();
            transitioners.Add(pair.Name, transitioner);

            pair.RagdollBone.OnPowerSettingChanged += (previousSetting, newSetting) => OnPowerSettingChanged(pair, previousSetting, newSetting);
        }

        static ValueTransitioner CreateTransitioner()
        {
            return new ValueTransitioner(0, 1, (t) => t * t);
        }

        void InitializeTransitioner(ValueTransitioner transitioner)
        {
            if (doStartingTransition)
            {
                transitioner.StartTransition(_transitionLength);
            }
            else
            {
                transitioner.EndTransition();
            }
        }


        public void Modify(ref BoneProfile boneProfile, RagdollAnimator.AnimatedPair pair, float dt)
        {
            ValueTransitioner transitioner = transitioners[pair.Name];
            transitioner.Update(dt);

            boneProfile.positionAlpha *= transitioner.Value;
            boneProfile.rotationAlpha *= transitioner.Value;
        }

        void OnPowerSettingChanged(RagdollAnimator.AnimatedPair pair, PowerSetting previousSetting, PowerSetting newSetting)
        {
            ValueTransitioner transitioner = transitioners[pair.Name];

            if (previousSetting == PowerSetting.Unpowered && newSetting == PowerSetting.Powered)
            {
                transitioner.StartTransition(_transitionLength);
            }
            else
            {
                transitioner.EndTransition();
            }
        }
    }
}
