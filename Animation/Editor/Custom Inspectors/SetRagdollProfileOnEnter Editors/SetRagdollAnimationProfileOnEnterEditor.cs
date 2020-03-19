using UnityEditor;

namespace Hairibar.Ragdoll.Animation.Editor
{
    [CustomEditor(typeof(SetRagdollAnimationProfileOnEnter))]
    internal class SetRagdollAnimationProfileOnEnterEditor : SetRagdollProfileOnEnterEditor
    {
        protected override string TypeDisplayName => "Animation Profile";
    }
}
