using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ThreeDSketchKit.Editor.PrefabEditing
{
    [InitializeOnLoad]
    public static class SketchKitCustomPrefabEditor
    {
        static readonly List<ICustomPrefabEditorModule> Modules = new()
        {
            new BoxBlockOneSidedScaleModule(),
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
            SceneView.duringSceneGui += OnSceneGUI;
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
            if (hover.FaceCornersWorld == null || hover.FaceCornersWorld.Length != 4)
                return;

            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
            Handles.DrawSolidRectangleWithOutline(hover.FaceCornersWorld, HoverFill, HoverOutline);

            // Show a "move-like" arrow so the user understands the face can be dragged.
            var normal = hover.NormalWorld.normalized;
            var center = hover.FaceCenterWorld;
            var size = HandleUtility.GetHandleSize(center) * 0.5f;
            using (new Handles.DrawingScope(ArrowColor))
            {
                Handles.ArrowHandleCap(
                    0,
                    center,
                    Quaternion.LookRotation(normal),
                    size,
                    EventType.Repaint);
            }
        }

        static void HandleDrag(Event e)
        {
            var normal = _drag.Hover.NormalWorld.normalized;
            var anchor = _drag.Hover.FaceCenterWorld;
            EditorSnapUtility.TryGetGridSnap(out var snapStep);

            // Allow dragging from ANY point on the highlighted face:
            // translate the mouse movement into a 1D movement along the face normal.
            if (e.type == EventType.MouseDrag && e.button == 0)
            {
                var rawDelta = HandleUtility.CalcLineTranslation(_dragStartMouse, e.mousePosition, anchor, normal);
                var snapped = EditorSnapUtility.SnapDelta(rawDelta, snapStep);
                ApplyDrag(snapped);
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

        static void ApplyDrag(float snappedDeltaWorld)
        {
            foreach (var module in Modules)
            {
                if (!module.CanEdit(_drag.Hover.Target))
                    continue;
                module.ApplyDrag(_drag, snappedDeltaWorld);
                return;
            }
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

