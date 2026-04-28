using ThreeDSketchKit.Core.Components;
using UnityEditor;
using UnityEngine;

namespace ThreeDSketchKit.Editor.Inspectors
{
    [CustomEditor(typeof(CharacterEntity))]
    public sealed class CharacterEntityEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Validate Character Systems"))
            {
                var entity = (CharacterEntity)target;
                var report = entity.ValidateSystems();
                foreach (var error in report.Errors)
                    Debug.LogError("[3D Sketch Kit] " + error, entity);
                foreach (var warning in report.Warnings)
                    Debug.LogWarning("[3D Sketch Kit] " + warning, entity);
                if (!report.HasErrors && report.Warnings.Count == 0)
                    Debug.Log("[3D Sketch Kit] Character systems validation passed.", entity);
            }
        }
    }
}
