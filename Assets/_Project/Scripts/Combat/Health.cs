using System;
using UnityEngine;

namespace StickBlade.Combat
{
    public class Health : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 100;

        public event Action<int, int> Changed;
        public event Action Died;

        public int CurrentHealth { get; private set; }
        public int MaxHealth => maxHealth;
        public bool IsDead => CurrentHealth <= 0;

        private void Awake()
        {
            CurrentHealth = maxHealth;
            Changed?.Invoke(CurrentHealth, maxHealth);
        }

        public void TakeDamage(int amount)
        {
            if (IsDead || amount <= 0)
            {
                return;
            }

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            Changed?.Invoke(CurrentHealth, maxHealth);

            if (CurrentHealth == 0)
            {
                Died?.Invoke();
            }
        }

        public void RestoreFull()
        {
            CurrentHealth = maxHealth;
            Changed?.Invoke(CurrentHealth, maxHealth);
        }
    }
}

