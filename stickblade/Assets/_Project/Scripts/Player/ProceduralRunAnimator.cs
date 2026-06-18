using UnityEngine;

namespace StickBlade.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class ProceduralRunAnimator : MonoBehaviour
    {
        [SerializeField] private float strideSpeed = 9f;
        [SerializeField] private float armSwing = 42f;
        [SerializeField] private float legSwing = 34f;
        [SerializeField] private float bodyBob = 0.08f;
        [SerializeField] private float bodyLean = 7f;
        [SerializeField] private float animationBlend = 12f;

        private CharacterController controller;
        private Transform body;
        private Transform leftArm;
        private Transform rightArm;
        private Transform leftLeg;
        private Transform rightLeg;
        private Vector3 bodyStartLocalPosition;
        private float runAmount;
        private float strideTime;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            body = FindChild("Body_Robe");
            leftArm = FindChild("LeftArm_Sleeve");
            rightArm = FindChild("RightArm_Sleeve");
            leftLeg = FindChild("LeftLeg_Pants");
            rightLeg = FindChild("RightLeg_Pants");

            if (body != null)
            {
                bodyStartLocalPosition = body.localPosition;
            }
        }

        private void Update()
        {
            Vector3 horizontalVelocity = controller.velocity;
            horizontalVelocity.y = 0f;

            float targetRunAmount = Mathf.InverseLerp(0.05f, 4.5f, horizontalVelocity.magnitude);
            runAmount = Mathf.MoveTowards(runAmount, targetRunAmount, animationBlend * Time.deltaTime);
            strideTime += Time.deltaTime * strideSpeed * Mathf.Lerp(0.25f, 1f, runAmount);

            float stride = Mathf.Sin(strideTime) * runAmount;
            float bob = Mathf.Abs(Mathf.Sin(strideTime)) * bodyBob * runAmount;

            if (body != null)
            {
                body.localPosition = bodyStartLocalPosition + Vector3.up * bob;
                body.localEulerAngles = new Vector3(bodyLean * runAmount, 0f, 0f);
            }

            Swing(leftArm, stride * armSwing, 0.02f * stride);
            Swing(rightArm, -stride * armSwing, -0.02f * stride);
            Swing(leftLeg, -stride * legSwing, -0.03f * stride);
            Swing(rightLeg, stride * legSwing, 0.03f * stride);
        }

        private Transform FindChild(string childName)
        {
            Transform[] children = GetComponentsInChildren<Transform>();
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i].name == childName)
                {
                    return children[i];
                }
            }

            return null;
        }

        private static void Swing(Transform part, float angle, float zOffset)
        {
            if (part == null)
            {
                return;
            }

            Vector3 euler = part.localEulerAngles;
            euler.x = angle;
            part.localEulerAngles = euler;

            Vector3 position = part.localPosition;
            position.z = zOffset;
            part.localPosition = position;
        }
    }
}
