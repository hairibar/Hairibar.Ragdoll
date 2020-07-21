using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 649
namespace Hairibar.Ragdoll
{
    /// <summary>
    /// Allows specific bones to ignore collisions between them, or disabling a bone's collisions altogether.
    /// </summary>
    [CreateAssetMenu(menuName = "Ragdoll/Collision Profile", fileName = "RAGCOL_New", order = 2)]
    public class RagdollCollisionProfile : RagdollProfile
    {
        #region Serialized Fields
        [SerializeField] List<BonePair> bonePairs = new List<BonePair>();
        [SerializeField] List<BoneName> disabled = new List<BoneName>();
        #endregion

        #region API
        internal event System.Action<RagdollCollisionProfile> OnUpdateValues;

        internal IEnumerable<BonePair> IgnoredPairs
        {
            get
            {
                ThrowExceptionIfNotValid();

                return bonePairs as IEnumerable<BonePair>;
            }
        }

        internal IEnumerable<BoneName> DisabledColliders
        {
            get
            {
                ThrowExceptionIfNotValid();

                return disabled as IEnumerable<BoneName>;
            }
        }
        #endregion


        void OnValidate()
        {
            if (IsValid) OnUpdateValues?.Invoke(this);
        }


        [System.Serializable]
        public struct BonePair
        {
            public BoneName boneA;
            public BoneName boneB;


            public BonePair(BoneName boneA, BoneName boneB)
            {
                this.boneA = boneA;
                this.boneB = boneB;
            }

            public override string ToString()
            {
                return "{" + $"{boneA.ToString()}, {boneB.ToString()}" + "}";
            }

            public static bool operator ==(BonePair a, BonePair b)
            {
                if (a.boneA == b.boneA && a.boneB == b.boneB) return true;
                if (a.boneA == b.boneB && a.boneB == b.boneA) return true;

                return false;
            }

            public static bool operator !=(BonePair a, BonePair b)
            {
                return !(a == b);
            }

            //Overriden stuff just to avoid warnings
            public override bool Equals(object obj)
            {
                if (obj is BonePair) return this == (BonePair) obj;
                else return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
    }
}