using UnityEngine;

namespace StickBlade.Combat
{
    public class SwordAttack : MonoBehaviour
    {
        [SerializeField] private Team team = Team.Player;
        [SerializeField] private Transform hitOrigin;
        [SerializeField] private float hitRadius = 1.2f;
        [SerializeField] private float hitForwardOffset = 1f;
        [SerializeField] private int healthDamage = 20;
        [SerializeField] private float postureDamage = 35f;
        [SerializeField] private float cooldown = 0.45f;
        [SerializeField] private LayerMask hitLayers = ~0;

        private float nextAttackTime;

        public bool CanAttack => Time.time >= nextAttackTime;

        private void Reset()
        {
            hitOrigin = transform;
        }

        public bool TryAttack()
        {
            if (!CanAttack)
            {
                return false;
            }

            nextAttackTime = Time.time + cooldown;
            DoHitCheck();
            return true;
        }

        private void DoHitCheck()
        {
            Transform origin = hitOrigin != null ? hitOrigin : transform;
            Vector3 center = origin.position + origin.forward * hitForwardOffset;
            Collider[] hits = Physics.OverlapSphere(center, hitRadius, hitLayers);

            foreach (Collider hit in hits)
            {
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                {
                    continue;
                }

                CombatActor actor = hit.GetComponentInParent<CombatActor>();
                if (actor == null || actor.Team == team)
                {
                    continue;
                }

                actor.ReceiveAttack(this);
                break;
            }
        }

        public int HealthDamage => healthDamage;
        public float PostureDamage => postureDamage;

        private void OnDrawGizmosSelected()
        {
            Transform origin = hitOrigin != null ? hitOrigin : transform;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(origin.position + origin.forward * hitForwardOffset, hitRadius);
        }
    }
}

