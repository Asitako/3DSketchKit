using System;
using UnityEditor;
using UnityEngine;

namespace ThreeDSketchKit.Editor.PrefabEditing
{
    static class EditorSnapUtility
    {
        public static bool TryGetGridSnap(out float step)
        {
            step = 0f;

            // Unity 2021+ / 6.x: EditorSnapSettings.move (Vector3)
            try
            {
                var move = EditorSnapSettings.move;
                step = Mathf.Max(Mathf.Abs(move.x), Mathf.Abs(move.y), Mathf.Abs(move.z));
            }
            catch (Exception)
            {
                // ignored
            }

            if (step <= 0f)
                step = 0.25f;

            return true;
        }

        public static float SnapDelta(float delta, float step)
        {
            if (step <= 0f)
                return delta;
            return Mathf.Round(delta / step) * step;
        }
    }
}

