using UnityEngine;

namespace Hairibar.Ragdoll
{
    /// <summary>
    /// Wrapper around a string that holds the name of a bone.
    /// </summary>
    [System.Serializable]
    public struct BoneName
    {
        [SerializeField]
        string name;


        public static implicit operator string(BoneName boneName)
        {
            return boneName.name;
        }

        public static implicit operator BoneName(string str)
        {
            return new BoneName(str);
        }

        public BoneName(string name)
        {
            this.name = name;
        }


        public static bool operator ==(BoneName a, BoneName b)
        {
            return a.name == b.name;
        }

        public static bool operator !=(BoneName a, BoneName b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is BoneName && (BoneName) obj == this;
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        public override string ToString()
        {
            return name;
        }
    }
}