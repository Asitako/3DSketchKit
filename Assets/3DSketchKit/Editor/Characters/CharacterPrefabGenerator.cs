using System.IO;
using ThreeDSketchKit.Core.Components;
using ThreeDSketchKit.Core.Data.Characters;
using ThreeDSketchKit.Core.Data.Combat;
using ThreeDSketchKit.Core.Data.Control;
using ThreeDSketchKit.Core.Data.Equipment;
using ThreeDSketchKit.Core.Data.Interaction;
using ThreeDSketchKit.Core.Data.Inventory;
using ThreeDSketchKit.Core.Data.Locomotion;
using UnityEditor;
using UnityEngine;

namespace ThreeDSketchKit.Editor.Characters
{
    public static class CharacterPrefabGenerator
    {
        public static string Generate(CharacterPrefabGenerationRequest request)
        {
            var validation = CharacterSourceValidator.Validate(request.ModelPrefab, request.AnimatorController, request.OutputFolder, request.PrefabName);
            if (validation.HasErrors)
                throw new System.InvalidOperationException(string.Join("\n", validation.Errors));

            EnsureAssetFolder(request.OutputFolder);
            EnsureAssetFolder(request.ProfileFolder);

            var root = new GameObject(request.PrefabName);
            try
            {
                var modelRoot = new GameObject("Model").transform;
                modelRoot.SetParent(root.transform, false);

                var modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(request.ModelPrefab);
                modelInstance.name = request.ModelPrefab.name;
                modelInstance.transform.SetParent(modelRoot, false);

                ApplyMaterial(modelInstance, request.Material, request.BaseTexture, request.ProfileFolder, request.PrefabName);

                var animator = modelInstance.GetComponentInChildren<Animator>(true);
                if (animator == null)
                    animator = root.AddComponent<Animator>();
                if (request.AnimatorController != null)
                    animator.runtimeAnimatorController = request.AnimatorController;

                root.AddComponent<AbilityManager>();
                root.AddComponent<MovementComponent>();
                root.AddComponent<HealthComponent>();

                var entity = root.AddComponent<CharacterEntity>();
                var preset = CreateProfiles(request);

                entity.Configure(
                    System.Guid.NewGuid().ToString("N"),
                    request.PrefabName,
                    ResolveRole(request.Template),
                    animator,
                    modelRoot,
                    preset);

                entity.SetSockets(CharacterSocketBuilder.BuildSockets(root, modelRoot));

                var systemsRoot = new GameObject("Systems").transform;
                systemsRoot.SetParent(root.transform, false);
                AddSystem<LocomotionSystem>(systemsRoot);
                AddSystem<AbilitySystem>(systemsRoot);
                AddSystem<CombatSystem>(systemsRoot);
                AddSystem<InventorySystem>(systemsRoot);
                AddSystem<EquipmentSystem>(systemsRoot);
                AddSystem<ControlSystem>(systemsRoot);
                AddSystem<InteractableSystem>(systemsRoot);

                new GameObject("Colliders").transform.SetParent(root.transform, false);

                var prefabPath = AssetDatabase.GenerateUniqueAssetPath($"{request.OutputFolder}/{request.PrefabName}.prefab");
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return prefabPath;
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        static T AddSystem<T>(Transform systemsRoot) where T : Component
        {
            var systemObject = new GameObject(typeof(T).Name);
            systemObject.transform.SetParent(systemsRoot, false);
            return systemObject.AddComponent<T>();
        }

        static CharacterPreset CreateProfiles(CharacterPrefabGenerationRequest request)
        {
            var preset = ScriptableObject.CreateInstance<CharacterPreset>();
            var locomotion = ScriptableObject.CreateInstance<LocomotionProfile>();
            var abilityLoadout = ScriptableObject.CreateInstance<AbilityLoadout>();
            var combat = ScriptableObject.CreateInstance<CombatProfile>();
            var inventory = ScriptableObject.CreateInstance<InventoryProfile>();
            var equipment = ScriptableObject.CreateInstance<EquipmentProfile>();
            var control = ScriptableObject.CreateInstance<ControlProfile>();
            var interaction = ScriptableObject.CreateInstance<InteractionProfile>();

            preset.Configure(ResolveRole(request.Template), locomotion, abilityLoadout, combat, inventory, equipment, control, interaction);

            CreateAsset(locomotion, request.ProfileFolder, $"LP_{request.PrefabName}_Locomotion.asset");
            CreateAsset(abilityLoadout, request.ProfileFolder, $"AL_{request.PrefabName}_AbilityLoadout.asset");
            CreateAsset(combat, request.ProfileFolder, $"CP_{request.PrefabName}_Combat.asset");
            CreateAsset(inventory, request.ProfileFolder, $"IP_{request.PrefabName}_Inventory.asset");
            CreateAsset(equipment, request.ProfileFolder, $"EP_{request.PrefabName}_Equipment.asset");
            CreateAsset(control, request.ProfileFolder, $"CP_{request.PrefabName}_Control.asset");
            CreateAsset(interaction, request.ProfileFolder, $"IP_{request.PrefabName}_Interaction.asset");
            CreateAsset(preset, request.ProfileFolder, $"CP_{request.PrefabName}_Preset.asset");

            return preset;
        }

        static void CreateAsset(Object asset, string folder, string fileName)
        {
            var path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{fileName}");
            AssetDatabase.CreateAsset(asset, path);
        }

        static void ApplyMaterial(GameObject modelInstance, Material material, Texture2D baseTexture, string folder, string prefabName)
        {
            var materialToApply = material;
            if (materialToApply == null && baseTexture != null)
            {
                materialToApply = new Material(FindLitShader()) { name = $"M_{prefabName}_Generated" };
                if (materialToApply.HasProperty("_BaseMap"))
                    materialToApply.SetTexture("_BaseMap", baseTexture);
                else if (materialToApply.HasProperty("_MainTex"))
                    materialToApply.SetTexture("_MainTex", baseTexture);
                CreateAsset(materialToApply, folder, $"{materialToApply.name}.mat");
            }

            if (materialToApply == null)
                return;

            foreach (var renderer in modelInstance.GetComponentsInChildren<Renderer>(true))
                renderer.sharedMaterial = materialToApply;
        }

        static Shader FindLitShader()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader != null)
                return shader;
            shader = Shader.Find("HDRP/Lit");
            if (shader != null)
                return shader;
            return Shader.Find("Standard");
        }

        static CharacterEntityRole ResolveRole(CharacterGenerationTemplate template)
        {
            return template switch
            {
                CharacterGenerationTemplate.PlayerReady => CharacterEntityRole.Player,
                CharacterGenerationTemplate.MobReady => CharacterEntityRole.Mob,
                CharacterGenerationTemplate.NpcReady => CharacterEntityRole.NPC,
                _ => CharacterEntityRole.Neutral
            };
        }

        static void EnsureAssetFolder(string folder)
        {
            folder = folder.Replace('\\', '/').TrimEnd('/');
            if (AssetDatabase.IsValidFolder(folder))
                return;

            if (string.IsNullOrWhiteSpace(folder) || !folder.StartsWith("Assets"))
                throw new DirectoryNotFoundException($"Unity asset folder must be under Assets: {folder}");

            var parts = folder.Split('/');
            var current = parts[0];
            for (var index = 1; index < parts.Length; index++)
            {
                var next = $"{current}/{parts[index]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[index]);
                current = next;
            }
        }
    }

    public sealed class CharacterPrefabGenerationRequest
    {
        public GameObject ModelPrefab;
        public Material Material;
        public Texture2D BaseTexture;
        public RuntimeAnimatorController AnimatorController;
        public CharacterGenerationTemplate Template;
        public string OutputFolder;
        public string ProfileFolder;
        public string PrefabName;
    }

    public enum CharacterGenerationTemplate
    {
        NeutralShell,
        PlayerReady,
        MobReady,
        NpcReady
    }
}
