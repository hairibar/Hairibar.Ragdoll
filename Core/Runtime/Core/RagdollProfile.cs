using UnityEngine;

#pragma warning disable 649
namespace Hairibar.Ragdoll
{
    /// <summary>
    /// Base class for ScriptableObjects that define the behaviour of a ragdoll.
    /// </summary>
    public abstract class RagdollProfile : ScriptableObject
    {
        public bool IsValid => _isValid;

        [SerializeField]
        protected RagdollDefinition definition;
        [SerializeField]
        bool _isValid;


        public static void ValidateAsArgument(RagdollProfile profile, RagdollDefinition definition, bool isRequired, string nullMessage = "")
        {
            if (!profile)
            {
                if (isRequired && Application.isPlaying) throw new System.ArgumentNullException(nullMessage);
            }
            else ValidateCompatibility(profile, definition);
        }

        public static void ValidateAsInspectorField(RagdollProfile profile, RagdollDefinition definition, bool isRequired, string nullMessage = "")
        {
            if (!profile)
            {
                if (isRequired && Application.isPlaying) throw new UnassignedReferenceException(nullMessage);
            }
            else ValidateCompatibility(profile, definition);
        }

        static void ValidateCompatibility(RagdollProfile profile, RagdollDefinition definition)
        {
            if (!profile.IsCompatibleWith(definition))
            {
                throw new IncompatibleRagdollProfileException(profile, definition);
            }
        }


        public virtual bool IsCompatibleWith(RagdollDefinition otherDefinition)
        {
            ThrowExceptionIfNotValid();

            return definition == otherDefinition;
        }

        protected void ThrowExceptionIfNotValid()
        {
            if (!IsValid)
            {
                throw new InvalidRagdollProfileException(this);
            }
        }
    }
}
