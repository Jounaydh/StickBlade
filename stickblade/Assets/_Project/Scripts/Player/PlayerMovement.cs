using UnityEngine;
using UnityEngine.InputSystem;

namespace StickBlade.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 4.8f;
        [SerializeField] private float sprintSpeed = 7.2f;
        [SerializeField] private float turnSpeed = 14f;
        [SerializeField] private float gravity = -24f;
        [SerializeField] private float dashDistance = 3.2f;
        [SerializeField] private float dashDuration = 0.16f;
        [SerializeField] private float dashCooldown = 0.45f;

        [Header("References")]
        [SerializeField] private Transform cameraTransform;

        private CharacterController controller;
        private float verticalVelocity;
        private Vector2 lastInput;
        private Vector3 dashVelocity;
        private float dashTimer;
        private float dashCooldownTimer;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();

            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        private void Update()
        {
            Vector2 input = ReadMoveInput();
            lastInput = input;
            Vector3 move = GetCameraRelativeMove(input);
            HandleDashInput(move);

            if (move.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(move, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }

            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            verticalVelocity += gravity * Time.deltaTime;

            float targetSpeed = IsSprintHeld() ? sprintSpeed : moveSpeed;
            Vector3 velocity = move * targetSpeed;
            if (dashTimer > 0f)
            {
                dashTimer -= Time.deltaTime;
                velocity += dashVelocity;
            }

            if (dashCooldownTimer > 0f)
            {
                dashCooldownTimer -= Time.deltaTime;
            }

            velocity.y = verticalVelocity;
            controller.Move(velocity * Time.deltaTime);
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(16f, 16f, 360f, 28f), $"PlayerMovement active | WASD input: {lastInput}");
        }

        private void HandleDashInput(Vector3 move)
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.spaceKey.wasPressedThisFrame || dashCooldownTimer > 0f)
            {
                return;
            }

            Vector3 dashDirection = move.sqrMagnitude > 0.001f ? move : transform.forward;
            dashVelocity = dashDirection.normalized * (dashDistance / dashDuration);
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;
        }

        private static Vector2 ReadMoveInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return Vector2.zero;
            }

            Vector2 input = Vector2.zero;

            if (keyboard.wKey.isPressed) input.y += 1f;
            if (keyboard.sKey.isPressed) input.y -= 1f;
            if (keyboard.dKey.isPressed) input.x += 1f;
            if (keyboard.aKey.isPressed) input.x -= 1f;

            return Vector2.ClampMagnitude(input, 1f);
        }

        private static bool IsSprintHeld()
        {
            Keyboard keyboard = Keyboard.current;
            return keyboard != null && keyboard.leftShiftKey.isPressed;
        }

        private Vector3 GetCameraRelativeMove(Vector2 input)
        {
            if (input.sqrMagnitude <= 0.001f)
            {
                return Vector3.zero;
            }

            Transform reference = cameraTransform != null ? cameraTransform : transform;
            Vector3 forward = reference.forward;
            Vector3 right = reference.right;

            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            return (forward * input.y + right * input.x).normalized;
        }
    }
}
