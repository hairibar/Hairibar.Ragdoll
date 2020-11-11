using UnityEngine;

namespace Hairibar.Ragdoll.Demo
{
    public class LocomotionInput : MonoBehaviour
    {
        private void Update()
        {
            float vertical = 0;
            if (Input.GetKey(KeyCode.W)) vertical += 1;
            if (Input.GetKey(KeyCode.S)) vertical -= 1;

            float horizontal = 0;
            if (Input.GetKey(KeyCode.D)) horizontal += 1;
            if (Input.GetKey(KeyCode.A)) horizontal -= 1;

            GetComponent<SimpleCharacterController>().verticalInput = vertical;
            GetComponent<SimpleCharacterController>().horizontalInput = horizontal;
        }
    }
}
