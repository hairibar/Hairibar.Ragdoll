using System.Collections.Generic;
using Hairibar.EngineExtensions;
using UnityEngine;

namespace Hairibar.Ragdoll
{
    /// <summary>
    /// Invokes Collision events for the Ragdoll's bones.
    /// </summary>
    [AddComponentMenu("Ragdoll/Ragdoll Collision Event Dispatcher")]
    [RequireComponent(typeof(RagdollDefinitionBindings))]
    public class RagdollCollisionEventDispatcher : MonoBehaviour
    {
        public delegate void CollisionEventListener(Collision collision, RagdollBone bone);

        public event CollisionEventListener OnCollisionEnter;
        public event CollisionEventListener OnCollisionStay;
        public event CollisionEventListener OnCollisionExit;


        Dictionary<CollisionEventDispatcher, RagdollBone> bones;


        #region Initialization
        void Start()
        {
            RagdollDefinitionBindings bindings = GetComponent<RagdollDefinitionBindings>();
            bindings.SubscribeToOnBonesCreated(Initialize);
        }

        void Initialize()
        {
            SetUpCollisionEventDispatchers();
        }


        void SetUpCollisionEventDispatchers()
        {
            RagdollDefinitionBindings bindings = GetComponent<RagdollDefinitionBindings>();
            bones = new Dictionary<CollisionEventDispatcher, RagdollBone>();

            foreach (RagdollBone bone in bindings.Bones)
            {
                bones.Add(SetUpCollisionEventDispatcher(bone), bone);
            }
        }

        CollisionEventDispatcher SetUpCollisionEventDispatcher(RagdollBone bone)
        {
            CollisionEventDispatcher dispatcher = bone.Rigidbody.gameObject.AddComponent<CollisionEventDispatcher>();

            dispatcher.OnCollisionEntered += DispatchOnCollisionEnter;
            dispatcher.OnCollisionStayed += DispatchOnCollisionStay;
            dispatcher.OnCollisionExited += DispatchOnCollisionExit;

            return dispatcher;
        }
        #endregion

        #region Dispatchers
        void DispatchOnCollisionEnter(Collision collision, CollisionEventDispatcher dispatcher)
        {
            OnCollisionEnter?.Invoke(collision, bones[dispatcher]);
        }

        void DispatchOnCollisionStay(Collision collision, CollisionEventDispatcher dispatcher)
        {
            OnCollisionStay?.Invoke(collision, bones[dispatcher]);
        }

        void DispatchOnCollisionExit(Collision collision, CollisionEventDispatcher dispatcher)
        {
            OnCollisionExit?.Invoke(collision, bones[dispatcher]);
        }
        #endregion


        void OnDestroy()
        {
            if (bones == null) return;

            foreach (CollisionEventDispatcher dispatcher in bones.Keys)
            {
                if (dispatcher) Destroy(dispatcher);
            }
        }
    }
}
