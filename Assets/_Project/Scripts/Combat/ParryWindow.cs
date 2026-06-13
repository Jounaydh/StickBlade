using UnityEngine;

namespace StickBlade.Combat
{
    public class ParryWindow : MonoBehaviour
    {
        [SerializeField] private float parryDuration = 0.25f;
        [SerializeField] private float cooldown = 0.35f;

        public bool IsParrying => Time.time <= parryEndTime;

        private float parryEndTime;
        private float nextParryTime;

        private void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                TryParry();
            }
        }

        public bool TryParry()
        {
            if (Time.time < nextParryTime)
            {
                return false;
            }

            parryEndTime = Time.time + parryDuration;
            nextParryTime = Time.time + cooldown;
            return true;
        }
    }
}

