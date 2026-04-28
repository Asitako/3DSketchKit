using ThreeDSketchKit.Core.Components;
using UnityEditor;

namespace ThreeDSketchKit.Editor.Inspectors
{
    [CustomEditor(typeof(CharacterSystemHost), true)]
    public sealed class CharacterSystemHostEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var host = (CharacterSystemHost)target;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("System Kind", host.SystemKind.ToString());
        }
    }
}
