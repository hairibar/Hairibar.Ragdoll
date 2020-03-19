using UnityEngine;

#pragma warning disable 649
namespace Hairibar.Ragdoll.Animation
{
    /// <summary>
    /// Base class for StateMachineBehaviours that set a ragdoll profile when entering a state.
    /// </summary>
    public abstract class SetRagdollProfileOnEnter<T> : StateMachineBehaviour where T : RagdollProfile
    {
        [SerializeField] T profile;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            GatherDependenciesIfNecessary(animator);
            SetProfile(profile);
        }

        protected virtual void GatherDependenciesIfNecessary(Animator animator) { }
        protected abstract void SetProfile(T profile);

    }
}
