using StickBlade.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace StickBlade.UI
{
    public class BarUI : MonoBehaviour
    {
        [SerializeField] private Image fill;

        public void SetValue(float current, float max)
        {
            if (fill == null)
            {
                return;
            }

            fill.fillAmount = max <= 0f ? 0f : Mathf.Clamp01(current / max);
        }

        public void BindHealth(Health health)
        {
            health.Changed += SetValue;
            SetValue(health.CurrentHealth, health.MaxHealth);
        }

        public void BindPosture(Posture posture)
        {
            posture.Changed += SetValue;
            SetValue(posture.CurrentPosture, posture.MaxPosture);
        }
    }
}

