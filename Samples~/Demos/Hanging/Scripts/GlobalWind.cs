using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hairibar.Ragdoll.Demo.Hanging
{
    public class GlobalWind : MonoBehaviour
    {
        public Vector3 direction;
        public float magnitude;
        public float frequency;

        private void FixedUpdate()
        {
            foreach (Rigidbody rigidbody in FindObjectsOfType<Rigidbody>())
            {
                rigidbody.AddForce(direction * magnitude * Mathf.PerlinNoise(Time.time, Time.time * frequency));
            }
        }

        private void OnValidate()
        {
            direction.Normalize();
        }
    }
}
