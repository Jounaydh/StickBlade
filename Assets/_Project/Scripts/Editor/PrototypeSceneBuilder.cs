using System.IO;
using StickBlade.CameraSystem;
using StickBlade.Characters;
using StickBlade.Combat;
using StickBlade.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace StickBlade.EditorTools
{
    public static class PrototypeSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/PrototypeArena.unity";
        private const string MaterialPath = "Assets/_Project/Art/Materials/";

        [MenuItem("Tools/StickBlade/Build Prototype Arena")]
        public static void BuildPrototypeArena()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "PrototypeArena";

            Material playerMaterial = CreateMaterial("M_Player", new Color(0.24f, 0.55f, 1f));
            Material enemyMaterial = CreateMaterial("M_Enemy", new Color(1f, 0.22f, 0.18f));
            Material groundMaterial = CreateMaterial("M_Ground", new Color(0.42f, 0.56f, 0.42f));
            Material swordMaterial = CreateMaterial("M_Sword", new Color(0.14f, 0.14f, 0.16f));
            Material markerMaterial = CreateMaterial("M_LockOnMarker", new Color(1f, 0.86f, 0.2f));

            GameObject light = new GameObject("Directional Light");
            Light lightComponent = light.AddComponent<Light>();
            lightComponent.type = LightType.Directional;
            lightComponent.intensity = 1.2f;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.transform.position = new Vector3(0f, -0.05f, 0f);
            ground.transform.localScale = new Vector3(18f, 0.1f, 18f);
            ApplyMaterial(ground, groundMaterial);

            GameObject player = CreatePlayer(playerMaterial, swordMaterial, markerMaterial);
            GameObject enemy = CreateEnemy(enemyMaterial, swordMaterial);
            enemy.transform.position = new Vector3(0f, 1f, 5f);
            enemy.GetComponent<EnemyController>().SetPlayer(player.transform);

            LockOnSystem lockOnSystem = player.GetComponent<LockOnSystem>();
            Camera mainCamera = CreateCamera(player.transform, lockOnSystem);
            ConfigurePlayerReferences(player, mainCamera.transform, lockOnSystem);

            CreateUi(player, enemy);
            CreateArenaWalls(groundMaterial);

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);

            Selection.activeGameObject = player;
            Debug.Log("StickBlade prototype arena created. Press Play to test movement, lock-on, attacks, parry, posture, and deathblow.");
        }

        private static GameObject CreatePlayer(Material bodyMaterial, Material swordMaterial, Material markerMaterial)
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.position = new Vector3(0f, 1f, -3f);
            ApplyMaterial(player, bodyMaterial);

            CharacterController controller = player.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.45f;
            controller.center = Vector3.zero;

            player.AddComponent<Health>();
            player.AddComponent<Posture>();
            player.AddComponent<ParryWindow>();
            player.AddComponent<PlayerController>();
            player.AddComponent<Deathblow>();
            player.AddComponent<LockOnSystem>();

            CombatActor actor = player.AddComponent<CombatActor>();
            SetEnum(actor, "team", Team.Player);

            SwordAttack attack = player.AddComponent<SwordAttack>();
            SetEnum(attack, "team", Team.Player);
            SetFloat(attack, "hitForwardOffset", 1.25f);
            SetFloat(attack, "hitRadius", 1.15f);

            GameObject sword = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sword.name = "Placeholder Sword";
            sword.transform.SetParent(player.transform);
            sword.transform.localPosition = new Vector3(0.42f, 0.1f, 0.65f);
            sword.transform.localRotation = Quaternion.Euler(18f, 0f, 0f);
            sword.transform.localScale = new Vector3(0.12f, 0.12f, 1.15f);
            ApplyMaterial(sword, swordMaterial);
            Object.DestroyImmediate(sword.GetComponent<Collider>());

            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "Lock On Marker";
            marker.transform.localScale = Vector3.one * 0.28f;
            ApplyMaterial(marker, markerMaterial);
            Object.DestroyImmediate(marker.GetComponent<Collider>());
            marker.SetActive(false);
            SetObject(player.GetComponent<LockOnSystem>(), "marker", marker.transform);

            return player;
        }

        private static GameObject CreateEnemy(Material bodyMaterial, Material swordMaterial)
        {
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = "Enemy_01";
            ApplyMaterial(enemy, bodyMaterial);

            enemy.AddComponent<Health>();
            enemy.AddComponent<Posture>();
            enemy.AddComponent<LockOnTarget>();
            enemy.AddComponent<EnemyController>();

            CombatActor actor = enemy.AddComponent<CombatActor>();
            SetEnum(actor, "team", Team.Enemy);

            SwordAttack attack = enemy.AddComponent<SwordAttack>();
            SetEnum(attack, "team", Team.Enemy);
            SetInt(attack, "healthDamage", 12);
            SetFloat(attack, "postureDamage", 22f);
            SetFloat(attack, "cooldown", 1.1f);
            SetFloat(attack, "hitForwardOffset", 1.25f);

            GameObject sword = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sword.name = "Placeholder Sword";
            sword.transform.SetParent(enemy.transform);
            sword.transform.localPosition = new Vector3(-0.42f, 0.1f, 0.65f);
            sword.transform.localRotation = Quaternion.Euler(18f, 0f, 0f);
            sword.transform.localScale = new Vector3(0.12f, 0.12f, 1.15f);
            ApplyMaterial(sword, swordMaterial);
            Object.DestroyImmediate(sword.GetComponent<Collider>());

            GameObject aimPoint = new GameObject("Aim Point");
            aimPoint.transform.SetParent(enemy.transform);
            aimPoint.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            enemy.GetComponent<LockOnTarget>().SetAimPoint(aimPoint.transform);

            return enemy;
        }

        private static Camera CreateCamera(Transform target, LockOnSystem lockOnSystem)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 4f, -8f);
            cameraObject.transform.LookAt(target.position + Vector3.up);

            AudioListener listener = cameraObject.AddComponent<AudioListener>();
            listener.enabled = true;

            CameraFollow follow = cameraObject.AddComponent<CameraFollow>();
            follow.SetTarget(target);
            follow.SetLockOnSystem(lockOnSystem);

            return camera;
        }

        private static void ConfigurePlayerReferences(GameObject player, Transform cameraTransform, LockOnSystem lockOnSystem)
        {
            SetObject(player.GetComponent<PlayerController>(), "cameraTransform", cameraTransform);
            SetObject(player.GetComponent<PlayerController>(), "lockOnSystem", lockOnSystem);
            SetObject(player.GetComponent<Deathblow>(), "lockOnSystem", lockOnSystem);
        }

        private static void CreateUi(GameObject player, GameObject enemy)
        {
            GameObject canvasObject = new GameObject("Canvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.AddComponent<GraphicRaycaster>();

            CreateBar(canvasObject.transform, "Player Health", new Vector2(24f, -24f), Color.red)
                .BindHealth(player.GetComponent<Health>());
            CreateBar(canvasObject.transform, "Player Posture", new Vector2(24f, -52f), new Color(1f, 0.78f, 0.18f))
                .BindPosture(player.GetComponent<Posture>());
            CreateBar(canvasObject.transform, "Enemy Health", new Vector2(-244f, -24f), Color.red, TextAnchor.UpperRight)
                .BindHealth(enemy.GetComponent<Health>());
            CreateBar(canvasObject.transform, "Enemy Posture", new Vector2(-244f, -52f), new Color(1f, 0.78f, 0.18f), TextAnchor.UpperRight)
                .BindPosture(enemy.GetComponent<Posture>());
        }

        private static BarUI CreateBar(Transform parent, string name, Vector2 anchoredPosition, Color fillColor, TextAnchor anchor = TextAnchor.UpperLeft)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent);
            RectTransform rootRect = root.AddComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(220f, 16f);
            rootRect.anchorMin = anchor == TextAnchor.UpperLeft ? new Vector2(0f, 1f) : new Vector2(1f, 1f);
            rootRect.anchorMax = rootRect.anchorMin;
            rootRect.pivot = anchor == TextAnchor.UpperLeft ? new Vector2(0f, 1f) : new Vector2(1f, 1f);
            rootRect.anchoredPosition = anchoredPosition;

            Image background = root.AddComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.55f);

            GameObject fillObject = new GameObject("Fill");
            fillObject.transform.SetParent(root.transform);
            RectTransform fillRect = fillObject.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(2f, 2f);
            fillRect.offsetMax = new Vector2(-2f, -2f);

            Image fill = fillObject.AddComponent<Image>();
            fill.color = fillColor;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;

            BarUI bar = root.AddComponent<BarUI>();
            SetObject(bar, "fill", fill);
            return bar;
        }

        private static void CreateArenaWalls(Material material)
        {
            CreateWall("Back Wall", new Vector3(0f, 0.5f, 9f), new Vector3(18f, 1f, 0.4f), material);
            CreateWall("Front Wall", new Vector3(0f, 0.5f, -9f), new Vector3(18f, 1f, 0.4f), material);
            CreateWall("Left Wall", new Vector3(-9f, 0.5f, 0f), new Vector3(0.4f, 1f, 18f), material);
            CreateWall("Right Wall", new Vector3(9f, 0.5f, 0f), new Vector3(0.4f, 1f, 18f), material);
        }

        private static void CreateWall(string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.position = position;
            wall.transform.localScale = scale;
            ApplyMaterial(wall, material);
        }

        private static Material CreateMaterial(string name, Color color)
        {
            Directory.CreateDirectory(MaterialPath);
            string path = $"{MaterialPath}{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                {
                    shader = Shader.Find("Standard");
                }

                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = color;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void ApplyMaterial(GameObject gameObject, Material material)
        {
            Renderer renderer = gameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }
        }

        private static void SetObject(Object target, string propertyName, Object value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = FindRequiredProperty(serializedObject, propertyName);
            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetEnum(Object target, string propertyName, Team value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = FindRequiredProperty(serializedObject, propertyName);
            property.enumValueIndex = (int)value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetFloat(Object target, string propertyName, float value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = FindRequiredProperty(serializedObject, propertyName);
            property.floatValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetInt(Object target, string propertyName, int value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = FindRequiredProperty(serializedObject, propertyName);
            property.intValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static SerializedProperty FindRequiredProperty(SerializedObject serializedObject, string propertyName)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogError($"Could not find serialized property '{propertyName}' on {serializedObject.targetObject.name}.");
            }

            return property;
        }
    }
}
