using UnityEngine;

namespace StickBlade.Combat
{
    public class LockOnTarget : MonoBehaviour
    {
        [SerializeField] private Transform aimPoint;

        public Transform AimPoint => aimPoint != null ? aimPoint : transform;

        public void SetAimPoint(Transform point)
        {
            aimPoint = point;
        }
    }
}

