using UnityEngine;

namespace Hairibar.Ragdoll
{
    public class IncompatibleRagdollProfileException : UnityException
    {
        public override string Message => $"RagdollProfile <i>{profile}</i> is <b>incompatible</b> with <i>{definition}</i>.";

        readonly string profile;
        readonly string definition;

        public IncompatibleRagdollProfileException(RagdollProfile profile, RagdollDefinition definition)
        {
            if (profile) this.profile = profile.name;
            else this.profile = "null";

            if (definition) this.definition = definition.name;
            else this.definition = "null";
        }
    }
}
