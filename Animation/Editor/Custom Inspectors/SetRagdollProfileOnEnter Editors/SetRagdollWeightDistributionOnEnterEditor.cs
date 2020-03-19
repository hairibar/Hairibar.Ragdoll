using UnityEditor;

namespace Hairibar.Ragdoll.Animation.Editor
{
    [CustomEditor(typeof(SetRagdollWeightDistributionOnEnter))]
    internal class SetRagdollWeightDistributionOnEnterEditor : SetRagdollProfileOnEnterEditor
    {
        protected override string TypeDisplayName => "Weight Distribution";
    }
}