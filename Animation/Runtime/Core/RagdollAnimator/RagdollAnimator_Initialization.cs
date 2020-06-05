using System.Collections.Generic;

namespace Hairibar.Ragdoll.Animation
{
    //Initialization
    public partial class RagdollAnimator
    {
        void CreateTargetToRagdollMapper()
        {
            mapper = new TargetToRagdollMapper(_ragdollBindings, transform);
        }

        void CreateAnimatedPairs(IReadOnlyCollection<RagdollBoneTargetBonePair> bonePairs)
        {
            animatedPairs = new AnimatedPair[bonePairs.Count];

            int i = 0;
            foreach (RagdollBoneTargetBonePair bonePair in bonePairs)
            {
                animatedPairs[i] = new AnimatedPair(bonePair);
                i++;
            }
        }

        void GatherBoneProfileModifiers()
        {
            boneProfileModifiers = GetComponents<IBoneProfileModifier>();
        }

        void InitializeBoneProfileModifiers(IBoneProfileModifier[] boneProfileModifiers, AnimatedPair[] pairs)
        {
            foreach (IBoneProfileModifier modifier in boneProfileModifiers)
            {
                modifier.Initialize(pairs);
            }
        }


        void GatherTargetPoseModifiers()
        {
            targetPoseModifiers = GetComponents<ITargetPoseModifier>();
        }

        void InitializeTargetPoseModifiers(ITargetPoseModifier[] targetPoseModifiers, AnimatedPair[] pairs)
        {
            foreach (ITargetPoseModifier modifier in targetPoseModifiers)
            {
                modifier.Initialize(pairs);
            }
        }


        void InitializeProfileTransitioning()
        {
            profileTransitioner = new ValueTransitioner(0, 1);
            profileTransitioner.EndTransition();

            previousProfile = currentProfile;
        }


        void InitializePreviousPosesWithCurrentPose()
        {
            foreach (AnimatedPair pair in animatedPairs)
            {
                pair.previousPose = pair.currentPose;
            }
        }
    }
}
