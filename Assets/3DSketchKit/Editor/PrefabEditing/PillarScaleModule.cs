using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ThreeDSketchKit.Editor.PrefabEditing
{
    /// <summary>
    /// Pillar (cylinder) editing:
    /// - Dragging a cap (flat face) performs one-sided scaling along Y (opposite cap stays fixed).
    /// - Dragging the curved side performs uniform radius scaling (X/Z together), keeping position fixed.
    /// </summary>
    public sealed class PillarScaleModule : ICustomPrefabEditorModule
    {
        readonly HashSet<string> _supportedPrefabNames = new(StringComparer.Ordinal)
        {
            "Pillar",
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

            return gameObject.GetComponent<CapsuleCollider>() != null;
        }

        public bool TryGetHover(Ray ray, out FaceHover hover)
        {
            hover = default;
            if (!Physics.Raycast(ray, out var hit, 1000f))
                return false;

            var go = hit.collider != null ? hit.collider.gameObject : null;
            if (go == null || !CanEdit(go))
                return false;

            var capsule = go.GetComponent<CapsuleCollider>();
            if (capsule == null)
                return false;

            var targetTransform = go.transform;
            var normalLocal = targetTransform.InverseTransformDirection(hit.normal);
            var axis = GetDominantAxis(normalLocal, out var sign);
            sign = sign >= 0f ? 1f : -1f;

            // For pillar:
            // - Caps are +/-Y (flat faces)
            // - Side is anything else → treat as radial
            if (axis == 1)
            {
                // Cap hover: build in LOCAL space, then transform to world (so rotation works).
                var meshFilter = go.GetComponent<MeshFilter>();
                var localBounds = meshFilter != null && meshFilter.sharedMesh != null
                    ? meshFilter.sharedMesh.bounds
                    : new Bounds(Vector3.zero, Vector3.one);
                var localBoundsCenter = localBounds.center;
                var localBoundsExtents = localBounds.extents;

                var faceCenterLocal = localBoundsCenter;
                faceCenterLocal.y += sign * localBoundsExtents.y;
                var faceCenterWorld = targetTransform.TransformPoint(faceCenterLocal);

                // Build an actual disc polygon for the cap highlight (approx circle).
                var radiusXLocal = localBoundsExtents.x;
                var radiusZLocal = localBoundsExtents.z;
                const int segments = 24;
                var disc = new Vector3[segments];
                for (var i = 0; i < segments; i++)
                {
                    var angleRadians = (i / (float)segments) * Mathf.PI * 2f;
                    var pointLocal = new Vector3(
                        faceCenterLocal.x + Mathf.Cos(angleRadians) * radiusXLocal,
                        faceCenterLocal.y,
                        faceCenterLocal.z + Mathf.Sin(angleRadians) * radiusZLocal);
                    disc[i] = targetTransform.TransformPoint(pointLocal);
                }

                hover = new FaceHover(
                    go,
                    capsule,
                    hit.point,
                    targetTransform.TransformDirection(Vector3.up * sign),
                    faceCenterWorld,
                    disc,
                    localAxis: 1,
                    axisSign: sign,
                    dragAxisCount: 1,
                    dragAxisAWorld: targetTransform.TransformDirection(Vector3.up * sign).normalized,
                    dragAxisBWorld: Vector3.zero);
                return true;
            }

            // Side hover: use normal direction to show arrow (no real plane corners).
            // We'll still provide a tiny quad around the hit point for highlight.
            var hitNormalWorld = hit.normal.normalized;
            var hoverQuadHalfSize = HandleUtility.GetHandleSize(hit.point) * 0.05f;
            var tangentWorld = Vector3.Cross(hitNormalWorld, Vector3.up);
            if (tangentWorld.sqrMagnitude < 1e-6f)
                tangentWorld = Vector3.Cross(hitNormalWorld, Vector3.right);
            tangentWorld.Normalize();
            var bitangentWorld = Vector3.Cross(hitNormalWorld, tangentWorld).normalized;

            var quad = new Vector3[4];
            quad[0] = hit.point + tangentWorld * hoverQuadHalfSize + bitangentWorld * hoverQuadHalfSize;
            quad[1] = hit.point + tangentWorld * hoverQuadHalfSize - bitangentWorld * hoverQuadHalfSize;
            quad[2] = hit.point - tangentWorld * hoverQuadHalfSize - bitangentWorld * hoverQuadHalfSize;
            quad[3] = hit.point - tangentWorld * hoverQuadHalfSize + bitangentWorld * hoverQuadHalfSize;

            hover = new FaceHover(
                go,
                capsule,
                hit.point,
                hitNormalWorld,
                hit.point,
                quad,
                localAxis: axis,
                axisSign: sign,
                dragAxisCount: 1,
                dragAxisAWorld: hitNormalWorld,
                dragAxisBWorld: Vector3.zero);
            return true;
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

        public bool TryGetDragFace(in FaceDrag drag, out FaceHover hover)
        {
            // Recompute hover by doing a synthetic hover from current bounds (caps) or keep as-hit for side.
            hover = default;
            var go = drag.Hover.Target;
            if (go == null || !CanEdit(go))
                return false;
            var capsule = go.GetComponent<CapsuleCollider>();
            if (capsule == null)
                return false;

            if (drag.Hover.LocalAxis == 1)
            {
                var meshFilter = go.GetComponent<MeshFilter>();
                var localBounds = meshFilter != null && meshFilter.sharedMesh != null
                    ? meshFilter.sharedMesh.bounds
                    : new Bounds(Vector3.zero, Vector3.one);
                var localBoundsCenter = localBounds.center;
                var localBoundsExtents = localBounds.extents;
                var sign = drag.Hover.AxisSign >= 0f ? 1f : -1f;
                var faceCenterLocal = localBoundsCenter;
                faceCenterLocal.y += sign * localBoundsExtents.y;
                var faceCenterWorld = go.transform.TransformPoint(faceCenterLocal);

                var radiusXLocal = localBoundsExtents.x;
                var radiusZLocal = localBoundsExtents.z;
                const int segments = 24;
                var disc = new Vector3[segments];
                for (var i = 0; i < segments; i++)
                {
                    var angleRadians = (i / (float)segments) * Mathf.PI * 2f;
                    var pointLocal = new Vector3(
                        faceCenterLocal.x + Mathf.Cos(angleRadians) * radiusXLocal,
                        faceCenterLocal.y,
                        faceCenterLocal.z + Mathf.Sin(angleRadians) * radiusZLocal);
                    disc[i] = go.transform.TransformPoint(pointLocal);
                }

                hover = new FaceHover(
                    go,
                    capsule,
                    drag.Hover.HitPointWorld,
                    go.transform.TransformDirection(Vector3.up * sign),
                    go.transform.TransformPoint(drag.StartFaceCenterLocal),
                    TransformPolygon(go.transform, drag.StartFacePolygonLocal),
                    localAxis: 1,
                    axisSign: sign,
                    dragAxisCount: 1,
                    dragAxisAWorld: go.transform.TransformDirection(Vector3.up * sign).normalized,
                    dragAxisBWorld: Vector3.zero);
                return true;
            }

            // Side: keep original normal and center at the current hit point.
            hover = new FaceHover(
                go,
                capsule,
                drag.Hover.HitPointWorld,
                drag.Hover.NormalWorld,
                go.transform.TransformPoint(drag.StartFaceCenterLocal),
                TransformPolygon(go.transform, drag.StartFacePolygonLocal),
                drag.Hover.LocalAxis,
                drag.Hover.AxisSign,
                drag.Hover.DragAxisCount,
                drag.Hover.DragAxisAWorld,
                drag.Hover.DragAxisBWorld);
            return true;
        }

        static Vector3[] TransformPolygon(Transform transform, Vector3[] polygonLocal)
        {
            if (polygonLocal == null)
                return null;
            var polygonWorld = new Vector3[polygonLocal.Length];
            for (var i = 0; i < polygonLocal.Length; i++)
                polygonWorld[i] = transform.TransformPoint(polygonLocal[i]);
            return polygonWorld;
        }

        public void ApplyDrag(in FaceDrag drag, Vector3 dragAxisWorld, float deltaAlongAxisWorld)
        {
            var go = drag.Hover.Target;
            if (go == null)
                return;

            var targetTransform = go.transform;

            // If dragging a cap: one-sided scale along Y (fixed opposite cap).
            if (drag.Hover.LocalAxis == 1)
            {
                var sign = drag.Hover.AxisSign >= 0f ? 1f : -1f;
                var normalWorld = targetTransform.TransformDirection(Vector3.up * sign).normalized;

                var lossyAxis = Mathf.Abs(drag.StartLossyScale.y);
                if (lossyAxis <= 1e-6f)
                    return;

                var capDeltaLocal = deltaAlongAxisWorld / lossyAxis;

                // Capsule height in local units (before scaling)
                var capsule = drag.Hover.Collider as CapsuleCollider;
                var heightLocal = capsule != null ? capsule.height : 2f;
                if (heightLocal <= 1e-6f)
                    heightLocal = 2f;

                var capScaleDelta = (capDeltaLocal / heightLocal) * drag.StartLocalScale.y;
                var capNewScale = drag.StartLocalScale;
                capNewScale.y = Mathf.Max(0.01f, drag.StartLocalScale.y + capScaleDelta);

                Undo.RecordObject(targetTransform, "One-sided Scale (Pillar Cap)");
                targetTransform.localScale = capNewScale;
                targetTransform.position = drag.StartWorldPosition + normalWorld * (deltaAlongAxisWorld * 0.5f);
                EditorUtility.SetDirty(targetTransform);
                return;
            }

            // Side: uniform radius scaling (X/Z), keep object center fixed.
            var normal = drag.Hover.NormalWorld.normalized;
            if (normal.sqrMagnitude < 1e-6f)
                return;

            // Map delta along hit normal to radius change.
            var lossyRadX = Mathf.Abs(drag.StartLossyScale.x);
            var lossyRadZ = Mathf.Abs(drag.StartLossyScale.z);
            var lossyRad = Mathf.Max(lossyRadX, lossyRadZ);
            if (lossyRad <= 1e-6f)
                return;

            var radialDeltaLocal = deltaAlongAxisWorld / lossyRad;

            // Approximate diameter in local units (capsule radius is 0.5 for unit cylinder-ish).
            var capsuleCollider = drag.Hover.Collider as CapsuleCollider;
            var diameterLocal = capsuleCollider != null ? capsuleCollider.radius * 2f : 1f;
            if (diameterLocal <= 1e-6f)
                diameterLocal = 1f;

            var radialScaleDelta = (radialDeltaLocal / diameterLocal) * Mathf.Max(drag.StartLocalScale.x, drag.StartLocalScale.z);
            var radialNewScale = drag.StartLocalScale;
            var newRad = Mathf.Max(0.01f, Mathf.Max(drag.StartLocalScale.x, drag.StartLocalScale.z) + radialScaleDelta);
            radialNewScale.x = newRad;
            radialNewScale.z = newRad;

            Undo.RecordObject(targetTransform, "Uniform Radius Scale (Pillar Side)");
            targetTransform.localScale = radialNewScale;
            targetTransform.position = drag.StartWorldPosition; // keep center fixed
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
    }
}

