using System.Collections.Generic;
using UnityEngine;

namespace Hairibar.Ragdoll.Animation.Debug
{
    /// <summary>
    /// Draws the animated pose that the RagdollAnimator has read this frame. Not suitable for Release.
    /// </summary>
    [AddComponentMenu("Ragdoll/Target Pose Visualizer")]
    [RequireComponent(typeof(RagdollAnimator))]
    public class TargetPoseVisualizer : MonoBehaviour, ITargetPoseModifier
    {
        #region Inspector
        [Header("Visual Style")]
        public Color boneColor = Color.red;
        public Color leafBoneColor = Color.yellow;
        [Range(0, 1)] public float leafBoneLength = 0.2f;
        #endregion

        Dictionary<Transform, Bone> bones = null;


        public void ModifyPose(IEnumerable<RagdollAnimator.AnimatedPair> pairs)
        {
            ReadAnimatedPose(pairs);

            foreach (Bone bone in bones.Values)
            {
                DrawBone(bone);
            }
        }

        void ReadAnimatedPose(IEnumerable<RagdollAnimator.AnimatedPair> pairs)
        {
            foreach (RagdollAnimator.AnimatedPair pair in pairs)
            {
                bones.TryGetValue(pair.RagdollBone.Transform, out Bone bone);

                bone.lastReadPosition = pair.currentPose.worldPosition;
                if (bone.isLeaf) bone.lastReadRotation = pair.currentPose.worldRotation;
            }
        }

        void DrawBone(Bone bone)
        {
            if (bone.parent == null) return;

            bones.TryGetValue(bone.parent, out Bone parent);

            UnityEngine.Debug.DrawLine(parent.lastReadPosition, bone.lastReadPosition, boneColor);

            if (bone.isLeaf) UnityEngine.Debug.DrawLine(bone.lastReadPosition, bone.lastReadPosition + bone.lastReadRotation * Vector3.up * leafBoneLength, leafBoneColor);
        }


        public void Initialize(IEnumerable<RagdollAnimator.AnimatedPair> pairs)
        {
            bones = new Dictionary<Transform, Bone>();

            foreach (RagdollAnimator.AnimatedPair pair in pairs)
            {
                Transform parent;
                Bone bone;

                bone = new Bone
                {
                    transform = pair.RagdollBone.Transform
                };

                parent = bone.transform.parent;

                if (parent.GetComponent<ConfigurableJoint>())
                {
                    bone.parent = parent;
                }

                bool isLeafBone = true;
                for (int j = 0; j < bone.transform.childCount; j++)
                {
                    if (bone.transform.GetChild(j).GetComponent<ConfigurableJoint>())
                    {
                        isLeafBone = false;
                        break;
                    }
                }

                bone.isLeaf = isLeafBone;
                bones.Add(bone.transform, bone);
            }
        }


        class Bone
        {
            public Transform transform;
            public Transform parent;
            public bool isLeaf;

            public Vector3 lastReadPosition;
            public Quaternion lastReadRotation;
        }
    }
}
