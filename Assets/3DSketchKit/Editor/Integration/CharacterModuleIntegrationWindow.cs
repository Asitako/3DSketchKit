using System.Collections.Generic;
using ThreeDSketchKit.Core.Data.Characters;
using UnityEditor;
using UnityEngine;

namespace ThreeDSketchKit.Editor.Integration
{
    public sealed class CharacterModuleIntegrationWindow : EditorWindow
    {
        readonly List<CharacterModuleDescriptor> _modules = new();
        string _assetFolder = "Assets/3DSketchKit/Runtime/Modules/Characters";
        Vector2 _scroll;

        [MenuItem("3D Sketch Kit/Characters/Integrate Character Module", priority = 41)]
        public static void Open()
        {
            GetWindow<CharacterModuleIntegrationWindow>("Module Integration");
        }

        void OnEnable() => Refresh();

        void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawFolderField("Asset Folder", ref _assetFolder);
                if (GUILayout.Button("Refresh", GUILayout.Width(90)))
                    Refresh();
            }

            EditorGUILayout.Space();
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (var descriptor in _modules)
                DrawModule(descriptor);
            EditorGUILayout.EndScrollView();
        }

        void Refresh()
        {
            _modules.Clear();
            _modules.AddRange(CharacterModuleDiscovery.Discover());
        }

        void DrawModule(CharacterModuleDescriptor descriptor)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"{descriptor.SystemKind}: {descriptor.DisplayName}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(descriptor.Type.FullName);

            if (!string.IsNullOrWhiteSpace(descriptor.Description))
                EditorGUILayout.LabelField(descriptor.Description, EditorStyles.wordWrappedMiniLabel);

            foreach (var error in descriptor.Errors)
                EditorGUILayout.HelpBox(error, MessageType.Error);
            foreach (var warning in descriptor.Warnings)
                EditorGUILayout.HelpBox(warning, MessageType.Warning);

            using (new EditorGUI.DisabledScope(!descriptor.IsValid || !AssetDatabase.IsValidFolder(_assetFolder)))
            {
                if (GUILayout.Button("Create Module Asset"))
                    CreateModuleAsset(descriptor);
            }

            EditorGUILayout.EndVertical();
        }

        void CreateModuleAsset(CharacterModuleDescriptor descriptor)
        {
            var asset = CreateInstance(descriptor.Type) as CharacterModuleAsset;
            if (asset == null)
                return;

            var path = AssetDatabase.GenerateUniqueAssetPath($"{_assetFolder}/{descriptor.Type.Name}.asset");
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = asset;
            Debug.Log("[3D Sketch Kit] Character module asset created: " + path);
        }

        static void DrawFolderField(string label, ref string folder)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                folder = EditorGUILayout.TextField(label, folder);
                if (GUILayout.Button("...", GUILayout.Width(32)))
                {
                    var selected = EditorUtility.OpenFolderPanel(label, "Assets", "");
                    if (!string.IsNullOrEmpty(selected) && selected.StartsWith(Application.dataPath))
                        folder = "Assets" + selected.Substring(Application.dataPath.Length).Replace('\\', '/');
                }
            }
        }
    }
}
