using System.Collections.Generic;
using Hairibar.EngineExtensions;
using NaughtyAttributes;
using UnityEngine;

namespace Hairibar.Ragdoll.Animation
{
    /// <summary>
    /// Lowers a ragdoll bone's alpha when it collides with something.
    /// </summary>
    [AddComponentMenu("Ragdoll/Ragdoll Collision Reaction"), RequireComponent(typeof(RagdollAnimator))]
    public class RagdollCollisionReaction : MonoBehaviour, IBoneProfileModifier
    {
        public LayerMask collisionMask = -1;
        public bool softenPositionMatching = true;
        public bool softenRotationMatching = false;

        public float SofteningAmount
        {
            get => softeningAmount;
            set => softeningAmount = Mathf.Clamp01(value);
        }
        [SerializeField, Range(0, 1)] float softeningAmount = 1;

        public float RecoveryTime
        {
            get => _recoveryTime;
            set => _recoveryTime = Mathf.Max(0, value);
        }
        [SerializeField, UsePropertySetter] float _recoveryTime = 0.5f;


        Dictionary<BoneName, ValueTransitioner> transitioners;
        RagdollCollisionEventDispatcher collisionEventDispatcher;


        #region Initialization
        public void Initialize(IEnumerable<RagdollAnimator.AnimatedPair> pairs)
        {
            SetUpCollisionEventDispatcher();
            InitializeTransitionerDictionary(pairs);
        }

        void SetUpCollisionEventDispatcher()
        {
            RagdollDefinitionBindings bindings = GetComponent<RagdollAnimator>().Bindings;
            collisionEventDispatcher = bindings.gameObject.AddComponent<RagdollCollisionEventDispatcher>();

            collisionEventDispatcher.OnCollisionEnter += CollisionListener;
            collisionEventDispatcher.OnCollisionStay += CollisionListener;
        }

        void InitializeTransitionerDictionary(IEnumerable<RagdollAnimator.AnimatedPair> pairs)
        {
            transitioners = new Dictionary<BoneName, ValueTransitioner>();
            foreach (RagdollAnimator.AnimatedPair pair in pairs)
            {
                ValueTransitioner transitioner = CreateTranstitioner();
                InitializeTransitioner(pair, transitioner);
            }
        }

        static ValueTransitioner CreateTranstitioner()
        {
            return new ValueTransitioner(0, 1, (t) => t * t);
        }

        void InitializeTransitioner(RagdollAnimator.AnimatedPair pair, ValueTransitioner transitioner)
        {
            transitioner.EndTransition();

            transitioners.Add(pair.Name, transitioner);
        }

        #endregion

        public void Modify(ref BoneProfile boneProfile, RagdollAnimator.AnimatedPair animatedPair, float dt)
        {
            if (!enabled) return;

            ValueTransitioner transitioner = transitioners[animatedPair.Name];

            transitioner.Update(dt);

            float multiplier = 1 - Mathf.Lerp(SofteningAmount, 0, transitioner.Value);

            if (softenPositionMatching) boneProfile.positionAlpha *= multiplier;
            if (softenRotationMatching) boneProfile.rotationAlpha *= multiplier;
        }


        void CollisionListener(Collision collision, RagdollBone bone)
        {
            if (collisionMask.LayerIsEnabled(collision.gameObject.layer))
            {
                transitioners[bone.Name].StartTransition(RecoveryTime);
            }
        }


        void OnDestroy()
        {
            if (collisionEventDispatcher)
            {
                Destroy(collisionEventDispatcher);
            }
        }
    }
}
