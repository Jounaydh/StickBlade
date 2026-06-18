using StickBlade.CameraSystem;
using UnityEditor;
using UnityEngine;

namespace StickBlade.EditorTools
{
    public static class PrototypeSceneSetup
    {
        [MenuItem("Tools/StickBlade/Setup Selected Player Camera")]
        public static void SetupSelectedPlayerCamera()
        {
            if (Selection.activeTransform == null)
            {
                Debug.LogWarning("Select the player object in the Hierarchy first.");
                return;
            }

            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                mainCamera = cameraObject.AddComponent<Camera>();
                cameraObject.tag = "MainCamera";
            }

            ThirdPersonFollowCamera followCamera = mainCamera.GetComponent<ThirdPersonFollowCamera>();
            if (followCamera == null)
            {
                followCamera = mainCamera.gameObject.AddComponent<ThirdPersonFollowCamera>();
            }

            followCamera.SetTarget(Selection.activeTransform);
            mainCamera.transform.position = Selection.activeTransform.position + new Vector3(0f, 2.2f, -4.2f);
            mainCamera.transform.LookAt(Selection.activeTransform.position + Vector3.up * 1.35f);

            Debug.Log($"Camera now follows {Selection.activeTransform.name}.");
        }
    }
}
