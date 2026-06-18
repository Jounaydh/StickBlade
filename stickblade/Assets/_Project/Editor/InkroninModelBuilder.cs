using System.IO;
using StickBlade.Player;
using StickBlade.Visuals;
using UnityEditor;
using UnityEngine;

namespace StickBlade.EditorTools
{
    public static class InkroninModelBuilder
    {
        private const string Root = "Assets/_Project";
        private const string PrefabPath = Root + "/Prefabs/Inkronin_PlayerPlaceholder.prefab";
        private const string ScenePrefabPath = Root + "/Prefabs/Inkronin_ArenaPreview.prefab";

        [MenuItem("Tools/StickBlade/Build Inkronin Placeholder Model")]
        public static void BuildInkronin()
        {
            EnsureFolders();

            Material black = CreateMaterial("M_Inkronin_Black", new Color(0.015f, 0.015f, 0.018f), 0.18f);
            Material cloth = CreateMaterial("M_Inkronin_ClothDark", new Color(0.035f, 0.036f, 0.04f), 0.22f);
            Material accent = CreateMaterial("M_Inkronin_CyanAccent", new Color(0.0f, 0.68f, 0.9f), 0.35f);
            Material blade = CreateMaterial("M_Inkronin_Blade", new Color(0.08f, 0.09f, 0.095f), 0.55f);
            Material skin = CreateMaterial("M_Inkronin_FaceShadow", new Color(0.01f, 0.01f, 0.012f), 0.12f);

            GameObject character = new GameObject("Inkronin_PlayerPlaceholder");
            CharacterVisualProfile profile = character.AddComponent<CharacterVisualProfile>();
            profile.approximateMeshParts = 38;
            ConfigureMovement(character);

            Transform visualRoot = new GameObject("VisualRoot").transform;
            visualRoot.SetParent(character.transform, false);
            visualRoot.localRotation = Quaternion.Euler(0f, 180f, 0f);

            BuildBody(visualRoot, black, cloth, accent, blade, skin);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(character, PrefabPath);
            Object.DestroyImmediate(character);

            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);

            BuildArenaPreview(prefab);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Built low-poly Inkronin placeholder prefab at {PrefabPath}");
        }

        private static void ConfigureMovement(GameObject character)
        {
            CharacterController controller = character.AddComponent<CharacterController>();
            controller.center = new Vector3(0f, 1.1f, 0f);
            controller.height = 2.2f;
            controller.radius = 0.34f;
            controller.stepOffset = 0.25f;
            controller.slopeLimit = 45f;

            character.AddComponent<PlayerMovement>();
            character.AddComponent<ProceduralRunAnimator>();
        }

        private static void BuildBody(Transform root, Material black, Material cloth, Material accent, Material blade, Material skin)
        {
            AddPart(root, "Body_Robe", PrimitiveType.Capsule, new Vector3(0f, 1.32f, 0f), new Vector3(0.64f, 0.82f, 0.4f), cloth);
            AddPart(root, "Upper_Robe_Block", PrimitiveType.Cube, new Vector3(0f, 1.45f, -0.01f), new Vector3(0.82f, 0.52f, 0.48f), cloth);
            AddPart(root, "Shoulder_Cowl", PrimitiveType.Cube, new Vector3(0f, 1.72f, -0.02f), new Vector3(0.92f, 0.16f, 0.58f), black);
            AddPart(root, "Chest_Wrap", PrimitiveType.Cube, new Vector3(0f, 1.49f, -0.255f), new Vector3(0.76f, 0.055f, 0.035f), accent, new Vector3(0f, 0f, -22f));
            AddPart(root, "Chest_Diagonal_Strap", PrimitiveType.Cube, new Vector3(-0.16f, 1.39f, -0.28f), new Vector3(0.05f, 0.62f, 0.035f), accent, new Vector3(0f, 0f, -28f));
            AddPart(root, "Waist_Sash", PrimitiveType.Cube, new Vector3(0f, 0.98f, 0f), new Vector3(0.86f, 0.13f, 0.52f), accent);
            AddPart(root, "Waist_Black_Belt", PrimitiveType.Cube, new Vector3(0f, 1.06f, -0.01f), new Vector3(0.92f, 0.07f, 0.54f), black);
            AddPart(root, "Front_Sash_Left", PrimitiveType.Cube, new Vector3(-0.12f, 0.72f, -0.18f), new Vector3(0.08f, 0.48f, 0.04f), accent, new Vector3(0f, 0f, -12f));
            AddPart(root, "Front_Sash_Right", PrimitiveType.Cube, new Vector3(0.1f, 0.72f, -0.2f), new Vector3(0.07f, 0.42f, 0.04f), accent, new Vector3(0f, 0f, 14f));
            AddTornRobePanels(root, black);

            AddPart(root, "Head", PrimitiveType.Sphere, new Vector3(0f, 2.08f, 0f), new Vector3(0.48f, 0.48f, 0.48f), skin);
            AddPart(root, "Eye_Left", PrimitiveType.Capsule, new Vector3(-0.12f, 2.1f, -0.245f), new Vector3(0.05f, 0.13f, 0.02f), accent);
            AddPart(root, "Eye_Right", PrimitiveType.Capsule, new Vector3(0.12f, 2.1f, -0.245f), new Vector3(0.05f, 0.13f, 0.02f), accent);

            AddPart(root, "Scarf", PrimitiveType.Cube, new Vector3(0f, 1.78f, -0.04f), new Vector3(0.72f, 0.22f, 0.54f), black);
            AddPart(root, "Scarf_Front_Point", PrimitiveType.Cube, new Vector3(0f, 1.62f, -0.29f), new Vector3(0.28f, 0.24f, 0.035f), black, new Vector3(0f, 0f, 45f));
            AddMoonMark(root, accent);

            AddLimb(root, "LeftArm", -0.47f, black, accent);
            AddLimb(root, "RightArm", 0.47f, black, accent);
            AddLeg(root, "LeftLeg", -0.22f, black, accent);
            AddLeg(root, "RightLeg", 0.22f, black, accent);
            AddHair(root, black);
            AddHeadband(root, accent);
            AddSword(root, blade, accent);
        }

        private static void AddTornRobePanels(Transform root, Material black)
        {
            for (int i = 0; i < 9; i++)
            {
                float x = -0.36f + i * 0.09f;
                float length = i % 2 == 0 ? 0.5f : 0.37f;
                AddPart(root, "Torn_Robe_Panel_" + i, PrimitiveType.Cube, new Vector3(x, 0.72f - length * 0.12f, -0.08f), new Vector3(0.055f, length, 0.035f), black, new Vector3(0f, 0f, -10f + i * 3f));
            }
        }

        private static void AddLimb(Transform root, string name, float x, Material black, Material accent)
        {
            AddPart(root, name + "_Sleeve", PrimitiveType.Capsule, new Vector3(x, 1.33f, -0.01f), new Vector3(0.18f, 0.58f, 0.18f), black, new Vector3(0f, 0f, x < 0f ? -14f : 14f));
            AddPart(root, name + "_ForearmWrap", PrimitiveType.Cube, new Vector3(x * 1.05f, 1.05f, -0.02f), new Vector3(0.19f, 0.08f, 0.2f), accent);
            AddPart(root, name + "_WristBand", PrimitiveType.Cube, new Vector3(x * 1.08f, 0.88f, -0.01f), new Vector3(0.16f, 0.055f, 0.18f), accent);
            AddPart(root, name + "_Hand", PrimitiveType.Sphere, new Vector3(x * 1.08f, 0.8f, -0.01f), new Vector3(0.14f, 0.14f, 0.14f), black);
        }

        private static void AddLeg(Transform root, string name, float x, Material black, Material accent)
        {
            AddPart(root, name + "_Pants", PrimitiveType.Capsule, new Vector3(x, 0.56f, 0f), new Vector3(0.24f, 0.72f, 0.24f), black);
            AddPart(root, name + "_AnkleWrap", PrimitiveType.Cube, new Vector3(x, 0.28f, -0.02f), new Vector3(0.25f, 0.07f, 0.22f), accent);
            AddPart(root, name + "_Foot", PrimitiveType.Cube, new Vector3(x, 0.1f, -0.09f), new Vector3(0.28f, 0.13f, 0.42f), black);
            AddPart(root, name + "_ShoeTrim", PrimitiveType.Cube, new Vector3(x, 0.16f, -0.29f), new Vector3(0.22f, 0.045f, 0.035f), accent);
        }

        private static void AddHair(Transform root, Material black)
        {
            for (int i = 0; i < 18; i++)
            {
                float angle = -105f + i * 12f;
                float radius = i % 2 == 0 ? 0.25f : 0.18f;
                float length = i % 3 == 0 ? 0.46f : 0.34f;
                Vector3 pos = new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad) * radius, 2.34f + (i % 4) * 0.025f, Mathf.Cos(angle * Mathf.Deg2Rad) * radius * 0.6f);
                AddPart(root, "Hair_Spike_" + i, PrimitiveType.Cylinder, pos, new Vector3(0.045f, length, 0.045f), black, new Vector3(42f, angle, 0f));
            }
        }

        private static void AddMoonMark(Transform root, Material accent)
        {
            AddPart(root, "Back_MoonMark_Top", PrimitiveType.Cube, new Vector3(0f, 1.6f, 0.245f), new Vector3(0.24f, 0.045f, 0.03f), accent, new Vector3(0f, 0f, 18f));
            AddPart(root, "Back_MoonMark_Left", PrimitiveType.Cube, new Vector3(-0.1f, 1.49f, 0.245f), new Vector3(0.045f, 0.2f, 0.03f), accent, new Vector3(0f, 0f, -24f));
            AddPart(root, "Back_MoonMark_Bottom", PrimitiveType.Cube, new Vector3(0.02f, 1.39f, 0.245f), new Vector3(0.19f, 0.045f, 0.03f), accent, new Vector3(0f, 0f, -18f));
        }

        private static void AddHeadband(Transform root, Material accent)
        {
            AddPart(root, "Headband_Front", PrimitiveType.Cube, new Vector3(0f, 2.22f, -0.245f), new Vector3(0.48f, 0.05f, 0.03f), accent);
            AddPart(root, "Headband_Side", PrimitiveType.Cube, new Vector3(-0.22f, 2.22f, 0.03f), new Vector3(0.45f, 0.045f, 0.03f), accent, new Vector3(0f, 55f, 0f));
            AddPart(root, "Headband_LeftTail", PrimitiveType.Cube, new Vector3(-0.34f, 2.18f, 0.08f), new Vector3(0.05f, 0.56f, 0.025f), accent, new Vector3(0f, 0f, 52f));
            AddPart(root, "Headband_RightTail", PrimitiveType.Cube, new Vector3(-0.42f, 2.11f, 0.1f), new Vector3(0.045f, 0.46f, 0.025f), accent, new Vector3(0f, 0f, 42f));
        }

        private static void AddSword(Transform root, Material blade, Material accent)
        {
            AddPart(root, "Katana_Sheath", PrimitiveType.Cube, new Vector3(0.47f, 0.95f, 0.19f), new Vector3(0.08f, 0.78f, 0.08f), blade, new Vector3(38f, 0f, -24f));
            AddPart(root, "Katana_Handle", PrimitiveType.Cube, new Vector3(0.31f, 1.33f, 0.05f), new Vector3(0.09f, 0.28f, 0.09f), blade, new Vector3(38f, 0f, -24f));
            AddPart(root, "Katana_HandleWrap", PrimitiveType.Cube, new Vector3(0.3f, 1.36f, 0.045f), new Vector3(0.11f, 0.04f, 0.11f), accent, new Vector3(38f, 0f, -24f));
            AddPart(root, "Katana_Guard", PrimitiveType.Cube, new Vector3(0.37f, 1.19f, 0.11f), new Vector3(0.24f, 0.035f, 0.12f), accent, new Vector3(38f, 0f, -24f));
            AddPart(root, "Drawn_Katana_Blade", PrimitiveType.Cube, new Vector3(0.58f, 0.88f, -0.34f), new Vector3(0.045f, 1.08f, 0.025f), blade, new Vector3(72f, 0f, -68f));
            AddPart(root, "Drawn_Katana_Edge_Cyan", PrimitiveType.Cube, new Vector3(0.58f, 0.91f, -0.37f), new Vector3(0.018f, 0.96f, 0.018f), accent, new Vector3(72f, 0f, -68f));
        }

        private static GameObject AddPart(Transform parent, string name, PrimitiveType type, Vector3 position, Vector3 scale, Material material, Vector3 rotation = default)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.transform.localScale = scale;

            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localEulerAngles = rotation;

            Object.DestroyImmediate(part.GetComponent<Collider>());
            part.GetComponent<Renderer>().sharedMaterial = material;
            return part;
        }

        private static Material CreateMaterial(string name, Color color, float smoothness)
        {
            string path = $"{Root}/Materials/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = color;
            material.SetColor("_BaseColor", color);
            if (name.Contains("Cyan"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 0.75f);
            }
            material.SetFloat("_Smoothness", smoothness);
            material.SetFloat("_Metallic", 0f);
            return material;
        }

        private static void BuildArenaPreview(GameObject characterPrefab)
        {
            GameObject previewRoot = new GameObject("Inkronin_ArenaPreview");
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(characterPrefab);
            instance.transform.SetParent(previewRoot.transform, false);
            instance.transform.localPosition = Vector3.zero;

            Material floorMaterial = CreateMaterial("M_Arena_FlatWarmGray", new Color(0.42f, 0.42f, 0.39f), 0.08f);
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Prototype_Floor";
            floor.transform.SetParent(previewRoot.transform, false);
            floor.transform.localPosition = new Vector3(0f, -0.04f, 0f);
            floor.transform.localScale = new Vector3(6f, 0.08f, 6f);
            floor.GetComponent<Renderer>().sharedMaterial = floorMaterial;
            Object.DestroyImmediate(floor.GetComponent<Collider>());

            PrefabUtility.SaveAsPrefabAsset(previewRoot, ScenePrefabPath);
            Object.DestroyImmediate(previewRoot);
        }

        private static void EnsureFolders()
        {
            string[] folders =
            {
                Root,
                Root + "/Editor",
                Root + "/Scripts",
                Root + "/Scripts/Visuals",
                Root + "/Prefabs",
                Root + "/Materials",
                Root + "/Scenes"
            };

            foreach (string folder in folders)
            {
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }
        }
    }
}
