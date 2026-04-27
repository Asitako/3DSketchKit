using ThreeDSketchKit.Core.Components;
using UnityEditor;
using UnityEngine;

namespace ThreeDSketchKit.Editor.Inspectors
{
    [CustomEditor(typeof(AbilityManager))]
    public sealed class AbilityManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var abilityManager = (AbilityManager)target;
            if (GUILayout.Button("Rebuild runtime abilities"))
            {
                Undo.RecordObject(abilityManager, "Rebuild Abilities");
                abilityManager.RebuildAbilities();
                EditorUtility.SetDirty(abilityManager);
            }
        }
    }
}
