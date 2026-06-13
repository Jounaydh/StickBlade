using UnityEngine;

namespace StickBlade.Combat
{
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(Posture))]
    public class CombatActor : MonoBehaviour
    {
        [SerializeField] private Team team;

        public Team Team => team;
        public Health Health { get; private set; }
        public Posture Posture { get; private set; }
        public ParryWindow ParryWindow { get; private set; }

        private void Awake()
        {
            Health = GetComponent<Health>();
            Posture = GetComponent<Posture>();
            ParryWindow = GetComponent<ParryWindow>();
        }

        public void ReceiveAttack(SwordAttack attack)
        {
            if (Health.IsDead)
            {
                return;
            }

            bool parried = ParryWindow != null && ParryWindow.IsParrying;
            if (parried)
            {
                attack.GetComponentInParent<Posture>()?.AddPostureDamage(attack.PostureDamage * 1.25f);
                return;
            }

            Health.TakeDamage(attack.HealthDamage);
            Posture.AddPostureDamage(attack.PostureDamage);
        }
    }
}

