using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hairibar.Ragdoll.Demo.Climbing
{
    public class ClimbingGrabbable : MonoBehaviour
    {
        public Transform Transform
        {
            get
            {
                if (!_transform)
                {
                    _transform = GetComponent<Transform>();
                }
                return _transform;
            }
        }
        private Transform _transform;
    }

}
