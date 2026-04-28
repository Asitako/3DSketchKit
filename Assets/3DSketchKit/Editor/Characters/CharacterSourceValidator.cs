using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ThreeDSketchKit.Editor.Characters
{
    public static class CharacterSourceValidator
    {
        public static CharacterSourceValidationReport Validate(GameObject modelPrefab, RuntimeAnimatorController animatorController, string outputFolder, string prefabName)
        {
            var report = new CharacterSourceValidationReport();

            if (modelPrefab == null)
            {
                report.Error("Model prefab or model asset is required.");
                return report;
            }

            var renderers = modelPrefab.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                report.Error("Model has no Renderer or SkinnedMeshRenderer.");

            var skinnedRenderers = modelPrefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (var skinnedRenderer in skinnedRenderers)
            {
                if (skinnedRenderer.bones == null || skinnedRenderer.bones.Length == 0)
                    report.Error($"SkinnedMeshRenderer '{skinnedRenderer.name}' has no bones.");
            }

            var animator = modelPrefab.GetComponentInChildren<Animator>(true);
            if (animator == null)
                report.Warning("Model has no Animator. The generator will add an Animator to CharacterRoot.");
            else if (animator.avatar == null && animatorController != null)
                report.Warning("Model Animator has no Avatar. Humanoid animation may not work until Avatar is assigned in import settings.");

            if (!HasLikelySocket(modelPrefab, "RightHand"))
                report.Warning("RightHand socket/bone was not found. A fallback socket will be created.");
            if (!HasLikelySocket(modelPrefab, "LeftHand"))
                report.Warning("LeftHand socket/bone was not found. A fallback socket will be created.");

            outputFolder = outputFolder?.Replace('\\', '/').TrimEnd('/');
            if (string.IsNullOrWhiteSpace(outputFolder) || !outputFolder.StartsWith("Assets"))
                report.Error("Output folder must be inside the Unity Assets folder.");

            if (string.IsNullOrWhiteSpace(prefabName))
                report.Error("Prefab name is required.");
            else if (prefabName.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
                report.Error("Prefab name contains invalid file name characters.");

            return report;
        }

        static bool HasLikelySocket(GameObject modelPrefab, string socketName)
        {
            return modelPrefab
                .GetComponentsInChildren<Transform>(true)
                .Any(t => t.name.IndexOf(socketName, System.StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }

    public sealed class CharacterSourceValidationReport
    {
        readonly List<string> _errors = new();
        readonly List<string> _warnings = new();

        public IReadOnlyList<string> Errors => _errors;
        public IReadOnlyList<string> Warnings => _warnings;
        public bool HasErrors => _errors.Count > 0;

        public void Error(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
                _errors.Add(message);
        }

        public void Warning(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
                _warnings.Add(message);
        }
    }
}
