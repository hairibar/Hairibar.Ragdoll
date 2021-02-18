using Hairibar.EngineExtensions.Serialization;
using UnityEngine;

#pragma warning disable 649
namespace Hairibar.Ragdoll
{
    /// <summary>
    /// Defines the PowerSetting of each bone in the ragdoll. Used by RagdollSettings.
    /// </summary>
    [CreateAssetMenu(menuName = "Ragdoll/Power Profile", fileName = "RAGPOW_New", order = 0)]
    public class RagdollPowerProfile : RagdollProfile
    {
        [SerializeField] PowerSettingsDictionary settings;

        #region Public API
        internal event System.Action OnUpdateValues;

        internal PowerSetting GetBoneSetting(BoneName bone)
        {
            ThrowExceptionIfNotValid();

            if (settings.TryGetValue(bone, out PowerSetting setting))
            {
                return setting;
            }
            else
            {
                UnityEngine.Debug.LogWarning($"Requested power settings for {bone}, but no setting for it was found in {name}.");
                return PowerSetting.Unpowered;
            }
        }
        #endregion


        void OnValidate()
        {
            if (IsValid) OnUpdateValues?.Invoke();
        }


        [System.Serializable]
        class PowerSettingsDictionary : SerializableDictionary<BoneName, PowerSetting> { }
    }
}
