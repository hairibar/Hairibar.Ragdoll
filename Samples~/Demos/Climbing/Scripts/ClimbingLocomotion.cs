using UnityEngine;

namespace Hairibar.Ragdoll.Demo.Climbing
{
    public class ClimbingLocomotion : MonoBehaviour
    {
        public float transitionLength;
        public float blendTreeDamping = 0.3f;

        Animator animator;
        Vector2 lastInput;

        void Update()
        {
            Vector2 input = new Vector2(0, 0);

            if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)) input.x = -1;
            else if (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A)) input.x = 1;

            if (Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.W)) input.y = -1;
            else if (Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S)) input.y = 1;

            if (input.magnitude > 1)
            {
                input.Normalize();
            }

            string animationState;
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                animationState = Mathf.Sign(input.x) == 1 ? "Right" : "Left";
            }
            else if (input != Vector2.zero)
            {
                animationState = Mathf.Sign(input.y) == 1 ? "Up" : "Down";
            }
            else
            {
                animationState = "";
            }

            //float velocity = input.magnitude;
            //if (input.y == 0)
            //{
            //    velocity = input.x;
            //}
            //else if (input.y < 0)
            //{
            //    velocity *= -1;
            //}

            if (animationState != "")
            {
                if (!animator.GetCurrentAnimatorStateInfo(0).IsName(animationState))
                {
                    animator.CrossFadeInFixedTime(animationState, transitionLength);
                }

            }

            animator.SetFloat("Velocity", input.magnitude, blendTreeDamping, Time.deltaTime);

            lastInput = input.normalized;
        }



        void OnAnimatorMove()
        {
            //Vector3 movement = new Vector3
            //{
            //    x = animator.velocity.x + Mathf.Abs(animator.velocity.y) * lastInput.x,
            //    y = animator.velocity.y * Mathf.Abs(lastInput.y)
            //};

            Vector3 movement = new Vector3()
            {
                x = animator.velocity.x,
                y = animator.velocity.y,
                z = 0
            };

            transform.Translate(movement * Time.deltaTime);
        }


        void Awake()
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

}
