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

            controls.Player.Move.performed += (ctx) => InputMovementVector = ctx.ReadValue<Vector2>();
            controls.Player.Move.canceled += (ctx) => InputMovementVector = ctx.ReadValue<Vector2>();

            controls.Player.Action.performed += (ctx) => ActionButton = true;
            controls.Player.Action.canceled += (ctx) => ActionButton = false;
        }

        private void OnDisable()
        {
            controls.Disable();
        }
    }
}
