using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ThreeDSketchKit.Editor.Diagnostics;

namespace ThreeDSketchKit.Editor.PrefabEditing
{
    [InitializeOnLoad]
    public static class SketchKitCustomPrefabEditor
    {
        static readonly List<ICustomPrefabEditorModule> Modules = new()
        {
            new BoxBlockOneSidedScaleModule(),
            new PillarScaleModule(),
            new RampScaleModule(),
        };

        static FaceHover? _hover;
        static bool _dragging;
        static FaceDrag _drag;
        static Vector2 _dragStartMouse;

        static readonly Color HoverFill = new(0.30f, 0.75f, 1.00f, 0.18f);
        static readonly Color HoverOutline = new(0.30f, 0.75f, 1.00f, 0.65f);
        static readonly Color ArrowColor = new(1f, 1f, 1f, 0.9f);

        static SketchKitCustomPrefabEditor()
        {
            SketchKitInitTimings.Begin("EditorInit.CustomPrefabEditor.SceneViewHook");
            try
            {
                SceneView.duringSceneGui += OnSceneGUI;
            }
            finally
            {
                SketchKitInitTimings.End("EditorInit.CustomPrefabEditor.SceneViewHook");
            }
        }

        static void OnSceneGUI(SceneView sceneView)
        {
            var e = Event.current;
            if (e == null)
                return;

            // Don't fight Unity camera controls.
            if (e.alt)
                return;

            var shift = e.shift;
            if (!shift && !_dragging)
            {
                _hover = null;
                return;
            }

            var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (!_dragging)
                _hover = GetHover(ray);
            else
                _hover = GetDragFaceHover();

            if (_hover.HasValue)
                DrawHover(_hover.Value);

            if (!_dragging && shift && e.type == EventType.MouseDown && e.button == 0 && _hover.HasValue)
            {
                if (TryBeginDrag(_hover.Value, out _drag))
                {
                    _dragging = true;
                    _dragStartMouse = e.mousePosition;
                    GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
                    e.Use();
                }
            }

            if (_dragging)
            {
                HandleDrag(e);
            }
        }

        static FaceHover? GetHover(Ray ray)
        {
            foreach (var module in Modules)
            {
                if (module.TryGetHover(ray, out var hover))
                    return hover;
            }

            return null;
        }

        static bool TryBeginDrag(in FaceHover hover, out FaceDrag drag)
        {
            foreach (var module in Modules)
            {
                if (!module.CanEdit(hover.Target))
                    continue;
                if (module.TryBeginDrag(hover, out drag))
                    return true;
            }

            drag = default;
            return false;
        }

        static void DrawHover(in FaceHover hover)
        {
            if (hover.FacePolygonWorld == null || hover.FacePolygonWorld.Length < 3)
                return;

            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
            using (new Handles.DrawingScope(HoverFill))
            {
                Handles.DrawAAConvexPolygon(hover.FacePolygonWorld);
            }
            using (new Handles.DrawingScope(HoverOutline))
            {
                for (var i = 0; i < hover.FacePolygonWorld.Length; i++)
                {
                    var edgeStart = hover.FacePolygonWorld[i];
                    var edgeEnd = hover.FacePolygonWorld[(i + 1) % hover.FacePolygonWorld.Length];
                    Handles.DrawLine(edgeStart, edgeEnd);
                }
            }

            var center = hover.FaceCenterWorld;
            var size = HandleUtility.GetHandleSize(center) * 0.5f;
            using (new Handles.DrawingScope(ArrowColor))
            {
                DrawArrow(center, hover.DragAxisAWorld, size);
                if (hover.DragAxisCount >= 2)
                    DrawArrow(center, hover.DragAxisBWorld, size);
            }
        }

        static void HandleDrag(Event e)
        {
            var anchor = _drag.Hover.FaceCenterWorld;
            EditorSnapUtility.TryGetGridSnap(out var snapStep);

            // Allow dragging from ANY point on the highlighted face:
            // translate the mouse movement into a 1D movement along the face normal.
            if (e.type == EventType.MouseDrag && e.button == 0)
            {
                var hover = GetDragFaceHover() ?? _drag.Hover;
                var axisA = hover.DragAxisAWorld.normalized;
                var rawA = HandleUtility.CalcLineTranslation(_dragStartMouse, e.mousePosition, anchor, axisA);
                var bestAxis = axisA;
                var bestRaw = rawA;

                if (hover.DragAxisCount >= 2)
                {
                    var axisB = hover.DragAxisBWorld.normalized;
                    var rawB = HandleUtility.CalcLineTranslation(_dragStartMouse, e.mousePosition, anchor, axisB);
                    if (Mathf.Abs(rawB) > Mathf.Abs(rawA))
                    {
                        bestAxis = axisB;
                        bestRaw = rawB;
                    }
                }

                var snapped = EditorSnapUtility.SnapDelta(bestRaw, snapStep);
                ApplyDrag(bestAxis, snapped);
                _hover = GetDragFaceHover();
                SceneView.RepaintAll();
                e.Use();
            }

            if (e.type == EventType.MouseUp && e.button == 0)
            {
                _dragging = false;
                _hover = null;
                GUIUtility.hotControl = 0;
                e.Use();
            }
        }

        static void ApplyDrag(Vector3 axisWorld, float snappedDeltaWorld)
        {
            foreach (var module in Modules)
            {
                if (!module.CanEdit(_drag.Hover.Target))
                    continue;
                module.ApplyDrag(_drag, axisWorld, snappedDeltaWorld);
                return;
            }
        }

        static void DrawArrow(Vector3 center, Vector3 axisWorld, float size)
        {
            if (axisWorld.sqrMagnitude < 1e-6f)
                return;
            Handles.ArrowHandleCap(
                0,
                center,
                Quaternion.LookRotation(axisWorld.normalized),
                size,
                EventType.Repaint);
        }

        static FaceHover? GetDragFaceHover()
        {
            foreach (var module in Modules)
            {
                if (!module.CanEdit(_drag.Hover.Target))
                    continue;
                if (module.TryGetDragFace(_drag, out var hover))
                    return hover;
            }

            return null;
        }
    }
}

