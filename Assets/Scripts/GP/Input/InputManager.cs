using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.GP.Input
{
    public class InputManager : Singleton<InputManager>
    {
        public bool IsInputEnable = true;
        private PlayerControls controls;

        /* Inputs */
        public Vector2 InputMovementVector;
        public bool JumpButton;
        public bool ActionButton;

        public override void Awake()
        {
            base.Awake();

            controls = new PlayerControls();
        }

        private void OnEnable()
        {
            //Enables controls
            controls = new PlayerControls();
            controls.Enable();

            controls.Player.Move.performed += (ctx) => this.InputMovementVector = ctx.ReadValue<Vector2>();
            controls.Player.Move.canceled += (ctx) => this.InputMovementVector = ctx.ReadValue<Vector2>();

            controls.Player.Jump.performed += (ctx) => this.JumpButton = true;
            controls.Player.Jump.canceled += (ctx) => this.JumpButton = false;

            controls.Player.Action.performed += (ctx) => this.ActionButton = true;
            controls.Player.Action.canceled += (ctx) => this.ActionButton = false;
        }

        private void OnDisable()
        {
            controls.Disable();
        }
    }
}
