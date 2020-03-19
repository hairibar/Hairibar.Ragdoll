using UnityEngine;

namespace Hairibar.Ragdoll
{
    internal class ValueTransitioner
    {
        public float Value => Mathf.Lerp(startValue, endValue, interpolator(t));
        public float Length { get; private set; }


        readonly float startValue;
        readonly float endValue;

        readonly Interpolator interpolator;
        float t;
        bool isTransitioning;


        public delegate float Interpolator(float linearT);


        public void StartTransition(float length)
        {
            ValidateLength(length);

            Length = length;
            t = 0;
            isTransitioning = true;
        }

        void ValidateLength(float length)
        {
            if (length < 0)
            {
                throw new System.ArgumentOutOfRangeException("length", length, "Tried to perform a transition with negative length.");
            }
        }


        public void EndTransition()
        {
            t = 1;
            isTransitioning = false;
        }

        public void Update(float dt)
        {
            float delta = dt / Length;

            if (isTransitioning)
            {
                t += delta;

                if (t > 1) EndTransition();
            }
        }


        //Use a linear interpolator by default
        public ValueTransitioner(float startValue, float endValue) : 
            this(startValue, endValue, (t) => t) { }
        

        public ValueTransitioner(float startValue, float endValue, Interpolator interpolator)
        {
            this.startValue = startValue;
            this.endValue = endValue;
            this.interpolator = interpolator;
        }
    }
}