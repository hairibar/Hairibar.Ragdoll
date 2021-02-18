using Hairibar.EngineExtensions.Serialization;
using NaughtyAttributes;
using UnityEngine;

#pragma warning disable 649
namespace Hairibar.Ragdoll
{
    /// <summary>
    /// Defines how the total weight of the ragdoll is distributed between its bones.
    /// </summary>
    [CreateAssetMenu(menuName = "Ragdoll/Weight Distribution", fileName = "RAGWGT_New", order = 3)]
    public class RagdollWeightDistribution : RagdollProfile
    {
        [SerializeField] WeightDistributionDictionary factors;


        internal event System.Action OnUpdateValues;

        internal float GetBoneMass(BoneName bone, float totalMass)
        {
            ThrowExceptionIfNotValid();

            if (factors.TryGetValue(bone, out float factor))
            {
                float actualFactor = factor / GetTotalFactorSum();
                return totalMass * actualFactor;
            }
            else
            {
                UnityEngine.Debug.LogWarning($"Bone \"{bone}\" is not present in {definition.name}.", this);
                return 1;
            }
        }


        float GetTotalFactorSum()
        {
            float total = 0;

            foreach (float factor in factors.Values)
            {
                total += factor;
            }

            return total;
        }

        void OnValidate()
        {
            if (IsValid) OnUpdateValues?.Invoke();
        }


        [System.Serializable]
        class WeightDistributionDictionary : SerializableDictionary<BoneName, float> { }
    }
}
