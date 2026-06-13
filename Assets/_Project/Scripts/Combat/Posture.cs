using System;
using UnityEngine;

namespace StickBlade.Combat
{
    public class Posture : MonoBehaviour
    {
        [SerializeField] private float maxPosture = 100f;
        [SerializeField] private float recoveryPerSecond = 12f;
        [SerializeField] private float recoveryDelay = 1.5f;

        public event Action<float, float> Changed;
        public event Action Broken;

        public float CurrentPosture { get; private set; }
        public float MaxPosture => maxPosture;
        public bool IsBroken { get; private set; }

        private float lastDamageTime;

        private void Update()
        {
            if (IsBroken || CurrentPosture <= 0f)
            {
                return;
            }

            if (Time.time < lastDamageTime + recoveryDelay)
            {
                return;
            }

            CurrentPosture = Mathf.Max(0f, CurrentPosture - recoveryPerSecond * Time.deltaTime);
            Changed?.Invoke(CurrentPosture, maxPosture);
        }

        public void AddPostureDamage(float amount)
        {
            if (IsBroken || amount <= 0f)
            {
                return;
            }

            lastDamageTime = Time.time;
            CurrentPosture = Mathf.Min(maxPosture, CurrentPosture + amount);
            Changed?.Invoke(CurrentPosture, maxPosture);

            if (CurrentPosture >= maxPosture)
            {
                IsBroken = true;
                Broken?.Invoke();
            }
        }

        public void ResetPosture()
        {
            IsBroken = false;
            CurrentPosture = 0f;
            Changed?.Invoke(CurrentPosture, maxPosture);
        }
    }
}

