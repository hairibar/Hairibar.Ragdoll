using System;
using System.Collections.Generic;
using UnityEngine;
using Hairibar.Ragdoll.Animation;
using System.Linq;
using UnityEngine.UIElements;
using Hairibar.EngineExtensions;

namespace Hairibar.Ragdoll.Demo.Climbing
{
    public class ClimbingExtremityPlacer : MonoBehaviour, ITargetPoseModifier
    {
        private const int MAX_CONSIDERED_GRABBABLES = 4;

        [Header("Raycast Parameters")]
        public float searchRadius = 0.5f;
        public LayerMask climbingLayerMask;

        [Header("Movement Parameters")]
        public float lerpDuration;
        public float maxLookAtVelocity;
        [Range(0, 1)] public float lookAtWeight;


        readonly GrabInfo leftHandGrab = new GrabInfo();
        readonly GrabInfo rightHandGrab = new GrabInfo();
        readonly GrabInfo leftFootGrab = new GrabInfo();
        readonly GrabInfo rightFootGrab = new GrabInfo();

        Vector3 lastLookAt;

        private void OnAnimatorIK(int layerIndex)
        {
            Animator animator = GetComponent<Animator>();

            Vector3 idealLookAt = GetNonGrabbingHand().position;

            Vector3 localLookAt = Vector3.MoveTowards(transform.InverseTransformPoint(lastLookAt), transform.InverseTransformPoint(idealLookAt), maxLookAtVelocity * Time.deltaTime);
            Vector3 worldLookAt = transform.TransformPoint(localLookAt);

            animator.SetLookAtPosition(worldLookAt);
            animator.SetLookAtWeight(lookAtWeight, 0);

            lastLookAt = worldLookAt;
        }

        public void ModifyPose(IEnumerable<RagdollAnimator.AnimatedPair> pairs)
        {
            foreach (var pair in pairs)
            {
                if (pair.Name == "R_Hand" ||
                    pair.Name == "L_Hand") //||
                    //pair.Name == "R_Foot" ||
                    //pair.Name == "L_Foot")
                {
                    if (ChooseExtremityPlacement(pair, out Vector3 targetPos))
                    {
                        pair.currentPose.worldPosition = targetPos;
                    }
                }
            }
        }

        Transform GetNonGrabbingHand()
        {
            if (!leftHandGrab.isGrabbing)
            {
                transform.parent.GetComponentInChildren<RagdollDefinitionBindings>().TryGetBone("L_Hand", out RagdollBone leftHand);
                return leftHand.Transform;
            }
            else
            {
                transform.parent.GetComponentInChildren<RagdollDefinitionBindings>().TryGetBone("R_Hand", out RagdollBone rightHand);
                return rightHand.Transform;
            }
        }


        private bool ChooseExtremityPlacement(RagdollAnimator.AnimatedPair pair, out Vector3 targetPos)
        {
            GrabInfo thisLimbGrabInfo = GetGrabInfoForBone(pair.Name);

            ClimbingGrabbable chosenGrabbable;
            if (thisLimbGrabInfo.isGrabbing)
            {
                chosenGrabbable = thisLimbGrabInfo.Grabbable;
            }
            else
            {
                chosenGrabbable = FindClosestGrabbable(pair.currentPose.worldPosition);
            }

            //Return the chosen position
            if (chosenGrabbable)
            {
                thisLimbGrabInfo.Grabbable = chosenGrabbable;
                targetPos = thisLimbGrabInfo.GetGrabPosition(pair.RagdollBone.Rigidbody.position, lerpDuration, Time.deltaTime);

                UnityEngine.Debug.DrawLine(pair.RagdollBone.Rigidbody.position, chosenGrabbable.Transform.position, Color.blue);
                UnityEngine.Debug.DrawLine(pair.RagdollBone.Rigidbody.position, targetPos, Color.yellow);

                return true;
            }
            else
            {
                thisLimbGrabInfo.Grabbable = null;
                targetPos = Vector3.zero;

                return false;
            }
        }

        /// <summary>
        /// Fils grabbableCache with the available Grabbables. The lower the index, the closer to the center it is.
        /// </summary>
        ClimbingGrabbable FindClosestGrabbable(Vector3 center)
        {
            Collider[] colliders = Physics.OverlapSphere(center, searchRadius, climbingLayerMask, QueryTriggerInteraction.Collide);

            List<ClimbingGrabbable> grabbables = new List<ClimbingGrabbable>();
            foreach (Collider collider in colliders)
            {
                if (collider.TryGetComponent(out ClimbingGrabbable grabbable))
                {
                    grabbables.Add(grabbable);
                }
            }

            if (grabbables.Count == 0) return null;
            else return grabbables.OrderBy(g => Vector3.Distance(center, g.transform.position)).First();
        }


        public void GrabLeft()
        {
            leftHandGrab.isGrabbing = true;
            rightHandGrab.isGrabbing = false;
        }

        public void GrabRight()
        {
            rightHandGrab.isGrabbing = true;
            leftHandGrab.isGrabbing = false;
        }


        private GrabInfo GetGrabInfoForBone(BoneName bone)
        {
            switch (bone)
            {
                case "L_Foot":
                    return leftFootGrab;
                case "R_Foot":
                    return rightFootGrab;
                case "L_Hand":
                    return leftHandGrab;
                case "R_Hand":
                    return rightHandGrab;
                default:
                    UnityEngine.Debug.LogError($"Asked for the GrabInfo for a bone that isn't an extremity ({bone}). This is wrong.", this);
                    return null;
            }
        }

        public void Initialize(IEnumerable<RagdollAnimator.AnimatedPair> pairs)
        {

        }

        private class GrabInfo
        {
            public ClimbingGrabbable Grabbable
            {
                get
                {
                    return _grabbable;
                }
                set
                {
                    if (value != _grabbable)
                    {
                        lerpT = 0;
                        _grabbable = value;
                    }
                }
            }
            private ClimbingGrabbable _grabbable;

            public bool isGrabbing;
            private float lerpT = 0;

            public Vector3 GetGrabPosition(Vector3 currentExtremityPosition, float lerpDuration, float deltaTime)
            {
                lerpT += (1 / lerpDuration) * deltaTime;
                return Vector3.Lerp(currentExtremityPosition, _grabbable.Transform.position, lerpT);
            }

            public static bool operator!(GrabInfo grabInfo)
            {
                return grabInfo.Grabbable;
            }
        }
    }
}
