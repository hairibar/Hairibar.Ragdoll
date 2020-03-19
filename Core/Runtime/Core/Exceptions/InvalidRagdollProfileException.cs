using UnityEngine;

namespace Hairibar.Ragdoll
{
    public class InvalidRagdollProfileException : UnityException
    {
        public override string Message => $"Tried to use <b>invalid profile</b> <i>{profileName}</i>.";
        readonly string profileName;

        public InvalidRagdollProfileException(Object profile)
        {
            profileName = profile.name;
        }
    }
}

