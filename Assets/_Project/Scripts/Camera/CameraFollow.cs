using StickBlade.Combat;
using UnityEngine;

namespace StickBlade.CameraSystem
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private LockOnSystem lockOnSystem;
        [SerializeField] private Vector3 offset = new Vector3(0f, 4f, -6f);
        [SerializeField] private float followSpeed = 10f;
        [SerializeField] private float lookHeight = 1.3f;

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Quaternion cameraRotation = Quaternion.Euler(18f, target.eulerAngles.y, 0f);
            Vector3 desiredPosition = target.position + cameraRotation * offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

            Vector3 lookPoint = target.position + Vector3.up * lookHeight;
            if (lockOnSystem != null && lockOnSystem.HasTarget)
            {
                lookPoint = Vector3.Lerp(lookPoint, lockOnSystem.CurrentTarget.AimPoint.position, 0.45f);
            }

            transform.LookAt(lookPoint);
        }

        public void SetTarget(Transform followTarget)
        {
            target = followTarget;
        }

        public void SetLockOnSystem(LockOnSystem system)
        {
            lockOnSystem = system;
        }
    }
}

