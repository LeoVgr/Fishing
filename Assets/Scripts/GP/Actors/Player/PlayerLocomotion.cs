using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.GP.Input;
using UnityEngine.InputSystem;

namespace Core.GP.Player
{
    public class PlayerLocomotion : MonoBehaviour
    {
        public float MaxSpeed;
        public float MaxAcceleration;
        public float MaxGroundAngle;
        public Animator Animator;
        private Rigidbody rb;
        private InputManager inputManager;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            inputManager = InputManager.instance;
        }

        private void FixedUpdate()
        {
            if (InputManager.instance.IsInputEnable)
                Move();
        }

        private void Move()
        {
            var movementInput = new Vector3(inputManager.InputMovementVector.x, 0, inputManager.InputMovementVector.y);

            var desiredVelocity = movementInput * MaxSpeed;
            var velocity = rb.velocity;
            var maxSpeedChange = MaxAcceleration * Time.deltaTime;

            velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
            velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);

            rb.velocity = velocity;

            // Manage movement animation
            if (Mathf.Abs(rb.velocity.x) > Mathf.Abs(rb.velocity.z))
            {
                if (rb.velocity.x > 0)
                {
                    RequestAnimation(Animator, "RightRun");
                    return;
                }

                if (rb.velocity.x < 0)
                {
                    RequestAnimation(Animator, "LeftRun");
                    return;
                }
            }
            else
            {
                if (rb.velocity.z > 0)
                {
                    RequestAnimation(Animator, "FrontRun");
                    return;
                }

                if (rb.velocity.z < 0)
                {
                    RequestAnimation(Animator, "BackRun");
                    return;
                }
            }

            RequestAnimation(Animator, "Idle");
        }

        private void RequestAnimation(Animator animator, string animationName)
        {
            if (!Animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
                Animator.CrossFadeInFixedTime(animationName, 0);
        }
    }
}
