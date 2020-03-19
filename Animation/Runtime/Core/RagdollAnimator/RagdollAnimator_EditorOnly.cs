#if UNITY_EDITOR
namespace Hairibar.Ragdoll.Animation
{
    //Editor-only debug behaviour. No serialized fields are allowed here.
    public partial class RagdollAnimator
    {
        //Changed by the editor via reflection.
        readonly bool forceAnimatedPose = false;
    }
}
#endif