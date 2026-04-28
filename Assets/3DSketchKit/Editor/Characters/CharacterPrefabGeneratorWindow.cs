using UnityEditor;
using UnityEngine;

namespace ThreeDSketchKit.Editor.Characters
{
    public sealed class CharacterPrefabGeneratorWindow : EditorWindow
    {
        GameObject _modelPrefab;
        Material _material;
        Texture2D _baseTexture;
        RuntimeAnimatorController _animatorController;
        CharacterGenerationTemplate _template = CharacterGenerationTemplate.NeutralShell;
        string _outputFolder = "Assets/3DSketchKit/Prefabs/Characters/Shells";
        string _profileFolder = "Assets/3DSketchKit/Runtime/Core/Data/Characters/Generated";
        string _prefabName = "PF_NewCharacter_Shell";
        CharacterSourceValidationReport _lastValidation;

        [MenuItem("3D Sketch Kit/Characters/Character Prefab Generator", priority = 40)]
        public static void Open()
        {
            GetWindow<CharacterPrefabGeneratorWindow>("Character Generator");
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Source Materials", EditorStyles.boldLabel);
            _modelPrefab = (GameObject)EditorGUILayout.ObjectField("Model Prefab/Asset", _modelPrefab, typeof(GameObject), false);
            _material = (Material)EditorGUILayout.ObjectField("Material", _material, typeof(Material), false);
            _baseTexture = (Texture2D)EditorGUILayout.ObjectField("Base Texture", _baseTexture, typeof(Texture2D), false);
            _animatorController = (RuntimeAnimatorController)EditorGUILayout.ObjectField("Animator Controller", _animatorController, typeof(RuntimeAnimatorController), false);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Generation", EditorStyles.boldLabel);
            _template = (CharacterGenerationTemplate)EditorGUILayout.EnumPopup("Template", _template);
            _prefabName = EditorGUILayout.TextField("Prefab Name", _prefabName);
            DrawFolderField("Output Folder", ref _outputFolder);
            DrawFolderField("Profile Folder", ref _profileFolder);

            EditorGUILayout.Space();
            if (GUILayout.Button("Validate"))
                _lastValidation = CharacterSourceValidator.Validate(_modelPrefab, _animatorController, _outputFolder, _prefabName);

            DrawValidationReport();

            using (new EditorGUI.DisabledScope(_modelPrefab == null))
            {
                if (GUILayout.Button("Generate Character Shell Prefab"))
                    Generate();
            }
        }

        void Generate()
        {
            _lastValidation = CharacterSourceValidator.Validate(_modelPrefab, _animatorController, _outputFolder, _prefabName);
            if (_lastValidation.HasErrors)
                return;

            var request = new CharacterPrefabGenerationRequest
            {
                ModelPrefab = _modelPrefab,
                Material = _material,
                BaseTexture = _baseTexture,
                AnimatorController = _animatorController,
                Template = _template,
                OutputFolder = _outputFolder,
                ProfileFolder = _profileFolder,
                PrefabName = _prefabName
            };

            var prefabPath = CharacterPrefabGenerator.Generate(request);
            Debug.Log("[3D Sketch Kit] Character shell prefab generated: " + prefabPath);
        }

        void DrawValidationReport()
        {
            if (_lastValidation == null)
                return;

            foreach (var error in _lastValidation.Errors)
                EditorGUILayout.HelpBox(error, MessageType.Error);
            foreach (var warning in _lastValidation.Warnings)
                EditorGUILayout.HelpBox(warning, MessageType.Warning);

            if (!_lastValidation.HasErrors && _lastValidation.Warnings.Count == 0)
                EditorGUILayout.HelpBox("Source materials are valid for shell generation.", MessageType.Info);
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
