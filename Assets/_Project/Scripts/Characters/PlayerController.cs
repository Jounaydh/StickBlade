using StickBlade.Combat;
using UnityEngine;

namespace StickBlade.Characters
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(SwordAttack))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 14f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private LockOnSystem lockOnSystem;

        private CharacterController controller;
        private SwordAttack swordAttack;
        private float verticalVelocity;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            swordAttack = GetComponent<SwordAttack>();
        }

        private void Update()
        {
            Move();

            if (Input.GetMouseButtonDown(0))
            {
                swordAttack.TryAttack();
            }
        }

        private void Move()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 input = new Vector3(horizontal, 0f, vertical).normalized;

            Vector3 move = Vector3.zero;
            if (input.sqrMagnitude > 0.01f)
            {
                Vector3 cameraForward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
                Vector3 cameraRight = cameraTransform != null ? cameraTransform.right : Vector3.right;
                cameraForward.y = 0f;
                cameraRight.y = 0f;
                cameraForward.Normalize();
                cameraRight.Normalize();

                move = cameraForward * input.z + cameraRight * input.x;
                move.Normalize();
            }

            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            verticalVelocity += gravity * Time.deltaTime;
            Vector3 velocity = move * moveSpeed + Vector3.up * verticalVelocity;
            controller.Move(velocity * Time.deltaTime);

            Rotate(move);
        }

        private void Rotate(Vector3 move)
        {
            Vector3 lookDirection = move;

            if (lockOnSystem != null && lockOnSystem.HasTarget)
            {
                lookDirection = lockOnSystem.CurrentTarget.transform.position - transform.position;
                lookDirection.y = 0f;
            }

            if (lookDirection.sqrMagnitude < 0.01f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}

