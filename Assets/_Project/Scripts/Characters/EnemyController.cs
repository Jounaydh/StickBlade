using StickBlade.Combat;
using UnityEngine;

namespace StickBlade.Characters
{
    [RequireComponent(typeof(SwordAttack))]
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private float moveSpeed = 2.4f;
        [SerializeField] private float stopDistance = 2f;
        [SerializeField] private float attackDistance = 2.2f;
        [SerializeField] private float rotationSpeed = 8f;

        private SwordAttack swordAttack;
        private Health health;
        private Posture posture;

        private void Awake()
        {
            swordAttack = GetComponent<SwordAttack>();
            health = GetComponent<Health>();
            posture = GetComponent<Posture>();
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.Died += OnDied;
            }

            if (posture != null)
            {
                posture.Broken += OnPostureBroken;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.Died -= OnDied;
            }

            if (posture != null)
            {
                posture.Broken -= OnPostureBroken;
            }
        }

        private void Update()
        {
            if (player == null || health.IsDead || posture.IsBroken)
            {
                return;
            }

            Vector3 toPlayer = player.position - transform.position;
            toPlayer.y = 0f;
            float distance = toPlayer.magnitude;

            if (toPlayer.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(toPlayer);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            if (distance > stopDistance)
            {
                transform.position += toPlayer.normalized * moveSpeed * Time.deltaTime;
            }

            if (distance <= attackDistance)
            {
                swordAttack.TryAttack();
            }
        }

        public void SetPlayer(Transform playerTransform)
        {
            player = playerTransform;
        }

        private void OnPostureBroken()
        {
            // The enemy waits in this vulnerable state until the player lands the deathblow.
        }

        private void OnDied()
        {
            gameObject.SetActive(false);
        }
    }
}

