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
        public float MaxAirAcceleration;
        public float MaxGroundAngle;
        public float MaxSnapSpeed = 100f;
        public float ProbeDistance = 1f;
        public float JumpHeight = 2f;
        public float FallMultiplier = 2.5f;
        public Transform PlayerSpace;
        private Rigidbody rb;
        private Vector3 velocity;
        private InputManager inputManager;
        private bool desiredJump;
        private bool onGround;
        private float minGroundDotProduct;
        private Vector3 contactNormal;
        private int stepsSinceLastGrounded;
        private int stepsSinceLastJump;

        private void Awake()
        {
            this.rb = GetComponent<Rigidbody>();
            this.inputManager = InputManager.instance;

            this.OnValidate();
        }

        private void FixedUpdate()
        {
            this.velocity = this.rb.velocity;

            if (InputManager.instance.IsInputEnable)
            {
                this.stepsSinceLastGrounded += 1;
                this.stepsSinceLastJump += 1;

                if (this.onGround || this.SnapToGround())
                {
                    this.contactNormal = Vector3.up;
                    this.stepsSinceLastGrounded = 0;
                }

                this.Move();
                this.Jump();

                if (this.velocity.y < 0 && !this.onGround)
                    this.velocity += Vector3.up * Physics.gravity.y * (this.FallMultiplier - 1) * Time.deltaTime;

                this.rb.velocity = this.velocity;
                this.contactNormal = Vector3.up;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            this.EvaluateCollision(collision);
        }

        private void OnCollisionStay(Collision collision)
        {
            this.EvaluateCollision(collision);
        }

        private void OnValidate()
        {
            this.minGroundDotProduct = Mathf.Cos(this.MaxGroundAngle * Mathf.Deg2Rad);
        }

        private void Move()
        {
            var movementInput = new Vector3(inputManager.InputMovementVector.x, 0, inputManager.InputMovementVector.y);

            var forward = this.PlayerSpace.transform.forward;
            forward.y = 0;
            forward.Normalize();

            var right = this.PlayerSpace.transform.right;
            right.y = 0;
            right.Normalize();

            var xAxis = this.ProjectOnContactPlane(Vector3.right).normalized;
            var zAxis = this.ProjectOnContactPlane(Vector3.forward).normalized;

            var currentX = Vector3.Dot(velocity, xAxis);
            var currentZ = Vector3.Dot(velocity, zAxis);

            var acceleration = onGround ? this.MaxAcceleration : this.MaxAirAcceleration;
            var maxSpeedChange = acceleration * Time.deltaTime;
            var desiredVelocity = (forward * movementInput.z + right * movementInput.x) * MaxSpeed;

            var newX = Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
            var newZ = Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

            this.velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);

            // Align player's model to input direction
            if (movementInput != Vector3.zero)
                this.gameObject.transform.forward = (forward * movementInput.z + right * movementInput.x);
        }

        private void Jump()
        {
            if (!this.onGround)
                return;

            if (!InputManager.instance.JumpButton)
            {
                this.onGround = false;
                return;
            }

            this.stepsSinceLastJump = 0;
            var jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * this.JumpHeight);
            if (velocity.y > 0)
                jumpSpeed = Mathf.Max(jumpSpeed - velocity.y, 0f);

            velocity.y += jumpSpeed;
            this.onGround = false;
        }

        private void EvaluateCollision(Collision collision)
        {
            for (var idx = 0; idx < collision.contactCount; idx++)
            {
                var normal = collision.GetContact(idx).normal;
                if (normal.y >= this.minGroundDotProduct)
                {
                    this.onGround = true;
                    this.contactNormal = normal;
                }
            }
        }

        private Vector3 ProjectOnContactPlane(Vector3 vector)
        {
            return vector - this.contactNormal * Vector3.Dot(vector, this.contactNormal);
        }

        private bool SnapToGround()
        {
            if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 2)
                return false;

            var speed = velocity.magnitude;
            if (speed > MaxSnapSpeed)
                return false;

            if (!Physics.Raycast(this.transform.position, Vector3.down, out RaycastHit hit, this.ProbeDistance))
                return false;

            if (hit.normal.y < minGroundDotProduct)
                return false;

            contactNormal = hit.normal;
            var dot = Vector3.Dot(velocity, hit.normal);
            if (dot > 0f)
                velocity = (velocity - hit.normal * dot).normalized * speed;

            return true;
        }
    }
}
