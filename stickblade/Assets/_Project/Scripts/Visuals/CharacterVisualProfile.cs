using UnityEngine;

namespace StickBlade.Visuals
{
    public sealed class CharacterVisualProfile : MonoBehaviour
    {
        [Header("Style")]
        public string characterName = "Inkronin";
        public Color primaryColor = new(0.015f, 0.015f, 0.018f);
        public Color accentColor = new(0.0f, 0.68f, 0.9f);

        [Header("Prototype Budget")]
        public int approximateMeshParts = 0;
        public bool usesFlatMaterials = true;
        public bool usesSkinnedMesh = false;
    }
}
