using StickBlade.CameraSystem;
using StickBlade.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace StickBlade.EditorTools
{
    public static class PrototypeArenaBuilder
    {
        private const string FloorName = "Prototype_Arena_Floor";
        private const string PlayerPrefabPath = "Assets/_Project/Prefabs/Inkronin_PlayerPlaceholder.prefab";
        private const string PlayerName = "Player_Inkronin";

        [MenuItem("Tools/StickBlade/Build Playable Prototype Scene")]
        public static void BuildPlayablePrototypeScene()
        {
            InkroninModelBuilder.BuildInkronin();
            BuildSimpleTestArena();

            GameObject player = GameObject.Find(PlayerName);
            if (player != null)
            {
                Object.DestroyImmediate(player);
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (prefab == null)
            {
                Debug.LogError($"Could not find player prefab at {PlayerPrefabPath}.");
                return;
            }

            player = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            player.name = PlayerName;

            player.transform.position = new Vector3(0f, 0.02f, 0f);
            player.transform.rotation = Quaternion.identity;
            EnsurePlayerMovement(player);
            Selection.activeGameObject = player;
            SetupCameraFor(player.transform);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("Built playable prototype scene. Press Play, click the Game view, then use WASD.");
        }

        [MenuItem("Tools/StickBlade/Build Simple Test Arena")]
        public static void BuildSimpleTestArena()
        {
            Material floorMaterial = GetOrCreateMaterial(
                "Assets/_Project/Materials/M_Arena_Floor.mat",
                new Color(0.42f, 0.42f, 0.38f));

            GameObject floor = GameObject.Find(FloorName);
            if (floor == null)
            {
                floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                floor.name = FloorName;
            }

            floor.transform.position = new Vector3(0f, -0.05f, 0f);
            floor.transform.localScale = new Vector3(14f, 0.1f, 14f);
            floor.GetComponent<Renderer>().sharedMaterial = floorMaterial;

            EnsureLight();
            EnsureCamera();

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("Built simple test arena floor, light, and camera.");
        }

        private static void EnsureLight()
        {
            Light light = Object.FindFirstObjectByType<Light>();
            if (light == null)
            {
                GameObject lightObject = new GameObject("Directional Light");
                light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
            }

            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            light.intensity = 1.4f;
        }

        private static void EnsureCamera()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                camera = cameraObject.AddComponent<Camera>();
            }

            camera.transform.position = new Vector3(0f, 2.4f, -5.2f);
            camera.transform.rotation = Quaternion.Euler(20f, 0f, 0f);

            if (Selection.activeTransform != null)
            {
                SetupCameraFor(Selection.activeTransform);
            }
        }

        private static void EnsurePlayerMovement(GameObject player)
        {
            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller == null)
            {
                controller = player.AddComponent<CharacterController>();
            }

            controller.center = new Vector3(0f, 1.1f, 0f);
            controller.height = 2.2f;
            controller.radius = 0.34f;
            controller.stepOffset = 0.25f;
            controller.slopeLimit = 45f;

            if (player.GetComponent<PlayerMovement>() == null)
            {
                player.AddComponent<PlayerMovement>();
            }

            if (player.GetComponent<ProceduralRunAnimator>() == null)
            {
                player.AddComponent<ProceduralRunAnimator>();
            }
        }

        private static void SetupCameraFor(Transform target)
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                camera = cameraObject.AddComponent<Camera>();
            }

            ThirdPersonFollowCamera follow = camera.GetComponent<ThirdPersonFollowCamera>();
            if (follow == null)
            {
                follow = camera.gameObject.AddComponent<ThirdPersonFollowCamera>();
            }

            follow.SetTarget(target);
            camera.transform.position = target.position + new Vector3(0f, 2.4f, -5.2f);
            camera.transform.LookAt(target.position + Vector3.up * 1.35f);
        }

        private static Material GetOrCreateMaterial(string path, Color color)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = color;
            material.SetFloat("_Smoothness", 0.1f);
            material.SetFloat("_Metallic", 0f);
            return material;
        }
    }
}
