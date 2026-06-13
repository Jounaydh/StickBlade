using UnityEngine;

namespace StickBlade.Combat
{
    public class Deathblow : MonoBehaviour
    {
        [SerializeField] private LockOnSystem lockOnSystem;
        [SerializeField] private float range = 2.4f;
        [SerializeField] private int deathblowDamage = 999;

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0))
            {
                return;
            }

            if (lockOnSystem == null || !lockOnSystem.HasTarget)
            {
                return;
            }

            CombatActor target = lockOnSystem.CurrentTarget.GetComponent<CombatActor>();
            if (target == null || !target.Posture.IsBroken)
            {
                return;
            }

            float distance = Vector3.Distance(transform.position, target.transform.position);
            if (distance <= range)
            {
                target.Health.TakeDamage(deathblowDamage);
            }
        }
    }
}

