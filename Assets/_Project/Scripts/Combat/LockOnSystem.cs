using UnityEngine;

namespace StickBlade.Combat
{
    public class LockOnSystem : MonoBehaviour
    {
        [SerializeField] private float searchRadius = 14f;
        [SerializeField] private LayerMask targetLayers = ~0;
        [SerializeField] private Transform marker;

        public LockOnTarget CurrentTarget { get; private set; }
        public bool HasTarget => CurrentTarget != null;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (CurrentTarget == null)
                {
                    CurrentTarget = FindNearestTarget();
                }
                else
                {
                    CurrentTarget = null;
                }
            }

            UpdateMarker();
        }

        private LockOnTarget FindNearestTarget()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius, targetLayers);
            LockOnTarget bestTarget = null;
            float bestDistance = float.MaxValue;

            foreach (Collider hit in hits)
            {
                LockOnTarget target = hit.GetComponentInParent<LockOnTarget>();
                if (target == null)
                {
                    continue;
                }

                Health health = target.GetComponent<Health>();
                if (health != null && health.IsDead)
                {
                    continue;
                }

                float distance = Vector3.Distance(transform.position, target.transform.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestTarget = target;
                }
            }

            return bestTarget;
        }

        private void UpdateMarker()
        {
            if (marker == null)
            {
                return;
            }

            bool show = CurrentTarget != null;
            marker.gameObject.SetActive(show);

            if (show)
            {
                marker.position = CurrentTarget.AimPoint.position + Vector3.up * 0.55f;
            }
        }
    }
}

