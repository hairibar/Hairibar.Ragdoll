using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 649
namespace Hairibar.Ragdoll
{
    /// <summary>
    /// Defines a collection of BoneNames in a ragdoll. Used by RagdollProfiles to refer to specific bones.
    /// </summary>
    [CreateAssetMenu(menuName = "Ragdoll/Ragdoll Definition", fileName = "ragdef_New", order = 0)]
    public class RagdollDefinition : ScriptableObject
    {
        #region Serialized Data
        [SerializeField] bool _isValid;
        [SerializeField] BoneName _root;
        [SerializeField] BoneName[] bones;
        #endregion

        #region API
        public int BoneCount
        {
            get
            {
                ThrowExceptionIfIsInvalid();
                return bones.Length;
            }
        }

        public IEnumerable<BoneName> Bones
        {
            get
            {
                ThrowExceptionIfIsInvalid();
                return bones;
            }
        }

        public BoneName Root
        {
            get
            {
                ThrowExceptionIfIsInvalid();
                return _root;
            }
        }

        public bool IsRoot(BoneName boneName)
        {
            ThrowExceptionIfIsInvalid();
            return boneName == _root;
        }


        internal bool IsValid => _isValid;
        #endregion

        void ThrowExceptionIfIsInvalid()
        {
            if (!IsValid)
            {
                throw new InvalidRagdollProfileException(this);
            }
        }
    }
}
