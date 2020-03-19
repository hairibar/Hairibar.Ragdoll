using System.Collections.Generic;
using Hairibar.EngineExtensions;
using UnityEngine;

namespace Hairibar.Ragdoll
{
    /// <summary>
    /// Invokes Collision events for the Ragdoll's bones.
    /// </summary>
    [AddComponentMenu("Radgoll/Ragdoll Collision Event Dispatcher")]
    [RequireComponent(typeof(RagdollDefinitionBindings))]
    public class RagdollCollisionEventDispatcher : MonoBehaviour
    {
        public delegate void CollisionEventListener(Collision collision, RagdollBone bone);

        public event CollisionEventListener OnCollisionEnter;
        public event CollisionEventListener OnCollisionStay;
        public event CollisionEventListener OnCollisionExit;


        Dictionary<Rigidbody, RagdollBone> bones;
        CollisionEventDispatcher[] dispatchers;

        #region Initialization
        void Start()
        {
            RagdollDefinitionBindings bindings = GetComponent<RagdollDefinitionBindings>();
            bindings.SubscribeToOnBonesCreated(Initialize);
        }

        void Initialize()
        {
            InitializeDictionary();
            SetUpCollisionEventDispatchers(bones.Keys);
        }

        void InitializeDictionary()
        {
            RagdollDefinitionBindings bindings = GetComponent<RagdollDefinitionBindings>();
            bones = new Dictionary<Rigidbody, RagdollBone>();

            foreach (RagdollBone bone in bindings.Bones)
            {
                bones.Add(bone.Rigidbody, bone);
            }
        }

        void SetUpCollisionEventDispatchers(IReadOnlyCollection<Rigidbody> rigidbodies)
        {
            dispatchers = new CollisionEventDispatcher[rigidbodies.Count];

            int i = 0;
            foreach (Rigidbody rigidbody in rigidbodies)
            {
                CollisionEventDispatcher dispatcher = SetUpCollisionEventDispatcher(rigidbody);

                dispatchers[i] = dispatcher;
                i++;
            }
        }

        CollisionEventDispatcher SetUpCollisionEventDispatcher(Rigidbody rigidbody)
        {
            CollisionEventDispatcher dispatcher = rigidbody.gameObject.AddComponent<CollisionEventDispatcher>();

            dispatcher.OnCollisionEntered += DispatchOnCollisionEnter;
            dispatcher.OnCollisionStayed += DispatchOnCollisionStay;
            dispatcher.OnCollisionExited += DispatchOnCollisionExit;

            return dispatcher;
        }
        #endregion

        #region Dispatchers
        void DispatchOnCollisionEnter(Collision collision)
        {
            OnCollisionEnter?.Invoke(collision, GetCollidingBone(collision));
        }

        void DispatchOnCollisionStay(Collision collision)
        {
            OnCollisionStay?.Invoke(collision, GetCollidingBone(collision));
        }

        void DispatchOnCollisionExit(Collision collision)
        {
            OnCollisionExit?.Invoke(collision, GetCollidingBone(collision));
        }
        #endregion

        RagdollBone GetCollidingBone(Collision collision)
        {
            Rigidbody rigidbody = collision.GetContact(0).thisCollider.attachedRigidbody;
            RagdollBone bone = bones[rigidbody];
            return bone;
        }


        void OnDestroy()
        {
            if (dispatchers is null) return;

            foreach (CollisionEventDispatcher dispatcher in dispatchers)
            {
                if (dispatcher) Destroy(dispatcher);
            }
        }
    }
}
