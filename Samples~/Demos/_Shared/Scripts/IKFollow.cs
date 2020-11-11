using NaughtyAttributes;
using UnityEngine;

namespace Hairibar.Ragdoll.Demo
{
    [RequireComponent(typeof(Animator))]
    public class IKFollow : MonoBehaviour
    {
        public Transform target;
        public bool doLookAt = true;
        public bool doLimbIK = true;

        [ShowIf("doLimbIK")] public AvatarIKGoal limb;
        [Range(0, 1)] public float weight;

        private Animator animator;

        private void OnAnimatorIK(int layerIndex)
        {
            if (!isActiveAndEnabled) return;

            if (doLookAt)
            {
                animator.SetLookAtPosition(target.position);
                animator.SetLookAtWeight(weight, 0.1f, 1);
            }

            if (doLimbIK)
            {
                animator.SetIKPosition(limb, target.position);
                animator.SetIKPositionWeight(limb, weight);
            }
        }

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        //Just there so that we get the enable checkbox
        private void Start() { }
    }
}
