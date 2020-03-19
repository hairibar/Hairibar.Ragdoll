using UnityEngine;

namespace Hairibar.Ragdoll
{
    /// <summary>
    /// The possible Power Settings for a ragdoll bone.
    /// <para>Kinematic: The Rigidbody is kinematic, it follows the animation perfectly.</para>
    /// <para>Powered: The Rigidbody is dynamic, systems such as RagdollAnimator will act upon it.</para>
    /// <para>Unpowered: The Rigidbody is dynamic, no forces will be added. Pure, dead weight ragdoll.</para>
    /// </summary>
    public enum PowerSetting
    {
        Kinematic,
        Powered,
        Unpowered
    }

    public static class PowerSettingExtensions
    {
        /// <summary>
        /// For Debug and Editor purposes. 
        /// Returns the associated Color for the PowerSetting.
        /// </summary>
        public static Color GetVisualizationColor(this PowerSetting powerSetting)
        {
            switch (powerSetting)
            {
                case PowerSetting.Kinematic:
                    return new Color(0.2f, 0.56f, 0.92f);
                case PowerSetting.Powered:
                    return new Color(0.2f, 0.92f, 0.2f);
                case PowerSetting.Unpowered:
                    return new Color(0.92f, 0.67f, 0.2f);
                default:
                    return Color.white;
            }
        }
    }
}
