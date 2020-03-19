using System.Collections.Generic;
using UnityEngine;

namespace Hairibar.Ragdoll.Animation
{
    internal class TargetToRagdollMapper
    {
        public IReadOnlyCollection<RagdollBoneTargetBonePair> BonePairs => _bonePairs;

        readonly RagdollBoneTargetBonePair[] _bonePairs;


        public void MapTargetToRagdoll()
        {
            foreach (RagdollBoneTargetBonePair pair in _bonePairs)
            {
                MapPair(pair);
            }
        }

        void MapPair(RagdollBoneTargetBonePair pair)
        {
            Vector3 simulatedPosition = pair.RagdollBone.Transform.position;
            Quaternion simulatedRotation = pair.RagdollBone.Transform.rotation;

            pair.TargetBone.position = simulatedPosition;
            pair.TargetBone.rotation = simulatedRotation;
        }

        #region Initialization
        public TargetToRagdollMapper(RagdollDefinitionBindings bindings, Transform targetParent)
        {
            if (!bindings) throw new System.ArgumentNullException("Tried to create a TargetToRagdollMapper with a null RagdollDefinitionBindings.");

            _bonePairs = CreateBonePairs(bindings, targetParent);
        }

        RagdollBoneTargetBonePair[] CreateBonePairs(RagdollDefinitionBindings bindings, Transform targetParent)
        {
            List<RagdollBoneTargetBonePair> pairs = new List<RagdollBoneTargetBonePair>();

            CreateBonePairsRecursively(FindCorrespondingBone(bindings.Root.Transform, targetParent),
                pairs, bindings.transform);

            return pairs.ToArray();


            void CreateBonePairsRecursively(Transform targetBoneTransform, List<RagdollBoneTargetBonePair> ragdollPairs, Transform ragdollParentTransform)
            {
                Transform ragdollBoneTransform = FindCorrespondingBone(targetBoneTransform, ragdollParentTransform);

                RagdollBone ragdollBone = null;
                if (ragdollBoneTransform) ragdollBone = GetRagdollBoneForRagdollBoneTransform(ragdollBoneTransform, bindings);

                if (ragdollBone != null) ragdollPairs.Add(new RagdollBoneTargetBonePair(ragdollBone, targetBoneTransform));

                //Recursively call all of its children
                for (int i = 0; i < targetBoneTransform.childCount; i++)
                {
                    CreateBonePairsRecursively(targetBoneTransform.GetChild(i), ragdollPairs, ragdollParentTransform);
                }
            }
        }

        static Transform FindCorrespondingBone(Transform originalBone, Transform equivalentBoneParent)
        {
            Transform result = null;

            for (int i = 0; i < equivalentBoneParent.childCount; i++)
            {
                Transform child = equivalentBoneParent.GetChild(i);

                if (child.name == originalBone.name)
                {
                    return child;
                }
                else
                {
                    result = FindCorrespondingBone(originalBone, child);
                    if (result != null) return result;
                }
            }

            return result;
        }

        static RagdollBone GetRagdollBoneForRagdollBoneTransform(Transform ragdollBoneTransform, RagdollDefinitionBindings bindings)
        {
            ConfigurableJoint joint = ragdollBoneTransform.GetComponent<ConfigurableJoint>();

            if (bindings.TryGetBoundBoneName(joint, out BoneName boneName))
            {
                bindings.TryGetBone(boneName, out RagdollBone bone);
                return bone;
            }
            else
            {
                return null;
            }
        }
        #endregion
    }
}
