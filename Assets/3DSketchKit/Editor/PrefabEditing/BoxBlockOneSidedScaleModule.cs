using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ThreeDSketchKit.Editor.PrefabEditing
{
    /// <summary>
    /// One-sided scaling for box-shaped blocks (Cube Mesh + BoxCollider).
    /// Initially wired to Floor/Beam/Door/Wall. Pillar/Ramp should get dedicated modules.
    /// </summary>
    public sealed class BoxBlockOneSidedScaleModule : ICustomPrefabEditorModule
    {
        readonly HashSet<string> _supportedPrefabNames = new(StringComparer.Ordinal)
        {
            "Floor",
            "Beam",
            "Door",
            "Wall",
        };

        public bool CanEdit(GameObject gameObject)
        {
            if (gameObject == null)
                return false;

            var source = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
            if (source == null)
                return false;

            if (!_supportedPrefabNames.Contains(source.name))
                return false;

            return gameObject.GetComponent<BoxCollider>() != null;
        }

        public bool TryGetHover(Ray ray, out FaceHover hover)
        {
            hover = default;

            if (!Physics.Raycast(ray, out var hit, 1000f))
                return false;

            var go = hit.collider != null ? hit.collider.gameObject : null;
            if (go == null || !CanEdit(go))
                return false;

            var box = go.GetComponent<BoxCollider>();
            if (box == null)
                return false;

            var targetTransform = go.transform;
            var normalWorld = hit.normal;

            // Axis selection in local space (x/y/z) + sign (positive/negative face).
            var normalLocal = targetTransform.InverseTransformDirection(normalWorld);
            var axis = GetDominantAxis(normalLocal, out var sign);
            sign = sign >= 0f ? 1f : -1f;

            hover = BuildFaceHover(go, box, hit.point, axis, sign);
            return true;
        }

        public bool TryGetDragFace(in FaceDrag drag, out FaceHover hover)
        {
            hover = default;
            var target = drag.Hover.Target;
            if (target == null || !CanEdit(target))
                return false;

            if (drag.Hover.Collider is not BoxCollider box)
                return false;

            var axis = drag.Hover.LocalAxis;
            var sign = drag.Hover.AxisSign >= 0f ? 1f : -1f;
            hover = BuildFaceHover(target, box, drag.Hover.HitPointWorld, axis, sign);
            return true;
        }

        static FaceHover BuildFaceHover(GameObject go, BoxCollider box, Vector3 hitPointWorld, int axis, float sign)
        {
            var targetTransform = go.transform;
            var size = box.size;
            var center = box.center;

            var extents = size * 0.5f;
            var faceCenterLocal = center;
            faceCenterLocal[axis] += sign * extents[axis];

            var cornersLocal = new Vector3[4];
            var uAxis = (axis + 1) % 3;
            var vAxis = (axis + 2) % 3;

            cornersLocal[0] = faceCenterLocal;
            cornersLocal[0][uAxis] += extents[uAxis];
            cornersLocal[0][vAxis] += extents[vAxis];

            cornersLocal[1] = faceCenterLocal;
            cornersLocal[1][uAxis] += extents[uAxis];
            cornersLocal[1][vAxis] -= extents[vAxis];

            cornersLocal[2] = faceCenterLocal;
            cornersLocal[2][uAxis] -= extents[uAxis];
            cornersLocal[2][vAxis] -= extents[vAxis];

            cornersLocal[3] = faceCenterLocal;
            cornersLocal[3][uAxis] -= extents[uAxis];
            cornersLocal[3][vAxis] += extents[vAxis];

            var faceCenterWorld = targetTransform.TransformPoint(faceCenterLocal);
            var cornersWorld = new Vector3[4];
            for (var i = 0; i < 4; i++)
                cornersWorld[i] = targetTransform.TransformPoint(cornersLocal[i]);

            return new FaceHover(
                go,
                box,
                hitPointWorld,
                targetTransform.TransformDirection(AxisToLocalNormal(axis, sign)),
                faceCenterWorld,
                cornersWorld,
                axis,
                sign,
                dragAxisCount: 1,
                dragAxisAWorld: targetTransform.TransformDirection(AxisToLocalNormal(axis, sign)).normalized,
                dragAxisBWorld: Vector3.zero);
        }

        public bool TryBeginDrag(in FaceHover hover, out FaceDrag drag)
        {
            drag = default;
            if (hover.Target == null)
                return false;

            var targetTransform = hover.Target.transform;
            var faceCenterLocal = targetTransform.InverseTransformPoint(hover.FaceCenterWorld);
            var facePolygonLocal = new Vector3[hover.FacePolygonWorld.Length];
            for (var i = 0; i < hover.FacePolygonWorld.Length; i++)
                facePolygonLocal[i] = targetTransform.InverseTransformPoint(hover.FacePolygonWorld[i]);
            drag = new FaceDrag(hover, targetTransform.localScale, targetTransform.position, targetTransform.lossyScale, faceCenterLocal, facePolygonLocal);
            return true;
        }

        public void ApplyDrag(in FaceDrag drag, Vector3 dragAxisWorld, float deltaAlongAxisWorld)
        {
            var target = drag.Hover.Target;
            if (target == null)
                return;

            var targetTransform = target.transform;
            if (drag.Hover.Collider is not BoxCollider box)
                return;

            var normalWorld = drag.Hover.NormalWorld.normalized;
            if (normalWorld.sqrMagnitude < 0.9f)
                return;

            var normalLocal = targetTransform.InverseTransformDirection(normalWorld);
            var axis = GetDominantAxis(normalLocal, out _);

            // Use the scale at drag start to avoid accumulating small errors.
            var lossyAxis = Mathf.Abs(drag.StartLossyScale[axis]);
            if (lossyAxis <= 1e-6f)
                return;

            // Convert world delta to local distance along the axis.
            var deltaLocal = deltaAlongAxisWorld / lossyAxis;

            var sizeAxis = box.size[axis];
            if (Mathf.Abs(sizeAxis) <= 1e-6f)
                sizeAxis = 1f;

            // Important: localScale is multiplicative for world size, so the delta in "scale units"
            // must be proportional to the starting localScale. Without that, non-1 scales (e.g. 0.25 on Y)
            // behave like a symmetric scale tool and the opposite face drifts.
            var scaleDelta = (deltaLocal / sizeAxis) * drag.StartLocalScale[axis];
            var newLocalScale = drag.StartLocalScale;
            newLocalScale[axis] = Mathf.Max(0.01f, drag.StartLocalScale[axis] + scaleDelta);

            Undo.RecordObject(targetTransform, "One-sided Scale");
            targetTransform.localScale = newLocalScale;

            // Shift the object so the opposite face stays fixed (pivot is assumed centered).
            targetTransform.position = drag.StartWorldPosition + normalWorld * (deltaAlongAxisWorld * 0.5f);
            EditorUtility.SetDirty(targetTransform);
        }

        static int GetDominantAxis(Vector3 v, out float sign)
        {
            var ax = Mathf.Abs(v.x);
            var ay = Mathf.Abs(v.y);
            var az = Mathf.Abs(v.z);

            if (ax >= ay && ax >= az)
            {
                sign = Mathf.Sign(v.x == 0f ? 1f : v.x);
                return 0;
            }

            if (ay >= ax && ay >= az)
            {
                sign = Mathf.Sign(v.y == 0f ? 1f : v.y);
                return 1;
            }

            sign = Mathf.Sign(v.z == 0f ? 1f : v.z);
            return 2;
        }

        static Vector3 AxisToLocalNormal(int axis, float sign)
        {
            return axis switch
            {
                0 => Vector3.right * sign,
                1 => Vector3.up * sign,
                _ => Vector3.forward * sign,
            };
        }
    }
}

