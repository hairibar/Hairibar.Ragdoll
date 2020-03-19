using UnityEditor;

namespace Hairibar.Ragdoll.Animation.Editor
{
    [CustomEditor(typeof(SetRagdollPowerProfileOnEnter))]
    internal class SetRagdollPowerProfileOnEnterEditor : SetRagdollProfileOnEnterEditor
    {
        protected override string TypeDisplayName => "Power Profile";
    }
}
