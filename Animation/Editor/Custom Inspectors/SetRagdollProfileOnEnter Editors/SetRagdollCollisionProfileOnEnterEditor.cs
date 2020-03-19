using UnityEditor;

namespace Hairibar.Ragdoll.Animation.Editor
{
    [CustomEditor(typeof(SetRagdollCollisionProfileOnEnter))]
    internal class SetRagdollCollisionProfileOnEnterEditor : SetRagdollProfileOnEnterEditor
    {
        protected override string TypeDisplayName => "Collision Profile";
    }
}
