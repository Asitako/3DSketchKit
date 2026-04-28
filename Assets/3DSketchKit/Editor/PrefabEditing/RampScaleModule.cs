using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ThreeDSketchKit.Editor.PrefabEditing
{
    /// <summary>
    /// Ramp editing (MeshCollider):
    /// - Horizontal and vertical faces: one-sided scaling along the dominant axis (fixed opposite face).
    /// - Sloped face: two-mode drag:
    ///   - Mouse movement mostly along Y adjusts height (one-sided along +Y, bottom fixed).
    ///   - Mouse movement mostly in XZ adjusts length (one-sided along +X, back face fixed).
    /// </summary>
    public sealed class RampScaleModule : ICustomPrefabEditorModule
    {
        readonly HashSet<string> _supportedPrefabNames = new(StringComparer.Ordinal)
        {
            "Ramp",
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

            return gameObject.GetComponent<MeshCollider>() != null;
        }

        public bool TryGetHover(Ray ray, out FaceHover hover)
        {
            hover = default;
            if (!Physics.Raycast(ray, out var hit, 1000f))
                return false;

            var go = hit.collider != null ? hit.collider.gameObject : null;
            if (go == null || !CanEdit(go))
                return false;

            var meshCollider = go.GetComponent<MeshCollider>();
            if (meshCollider == null)
                return false;

            var targetTransform = go.transform;
            var hitNormalLocal = targetTransform.InverseTransformDirection(hit.normal).normalized;

            // Identify faces in ramp-local space.
            var isSlopedFace = Mathf.Abs(hitNormalLocal.y) > 0.25f && (Mathf.Abs(hitNormalLocal.x) > 0.25f || Mathf.Abs(hitNormalLocal.z) > 0.25f);

            if (!MeshFaceUtility.TryGetConnectedCoplanarFacePolygon(meshCollider, hit, normalDotThreshold: 0.995f, out var poly, out var nWorld))
                return false;

            // Face center for arrow placement.
            var center = Vector3.zero;
            for (var i = 0; i < poly.Length; i++)
                center += poly[i];
            center /= poly.Length;

            if (isSlopedFace)
            {
                var objectUpAxisWorld = targetTransform.TransformDirection(Vector3.up).normalized;
                var rampForwardAxisWorld = targetTransform.TransformDirection(Vector3.right);
                var rampLengthAxisWorld = Vector3.ProjectOnPlane(rampForwardAxisWorld, objectUpAxisWorld);
                if (rampLengthAxisWorld.sqrMagnitude < 1e-6f)
                    rampLengthAxisWorld = Vector3.ProjectOnPlane(targetTransform.TransformDirection(Vector3.forward), objectUpAxisWorld);
                rampLengthAxisWorld.Normalize();

                var faceNormalWorld = nWorld.normalized;

                // Split the SLOPED face into two rectangles stacked "top/bottom" (horizontal midline):
                // We use object Up projected into the face plane as the stacking axis.
                var faceUpAxisWorld = Vector3.ProjectOnPlane(objectUpAxisWorld, faceNormalWorld);
                if (faceUpAxisWorld.sqrMagnitude < 1e-6f)
                    faceUpAxisWorld = Vector3.ProjectOnPlane(targetTransform.TransformDirection(Vector3.forward), faceNormalWorld);
                faceUpAxisWorld.Normalize();

                // The other axis lies in face plane and is orthogonal to faceUpAxisWorld.
                var faceSideAxisWorld = Vector3.Cross(faceNormalWorld, faceUpAxisWorld).normalized;

                var minUpCoord = float.PositiveInfinity;
                var maxUpCoord = float.NegativeInfinity;
                var minSideCoord = float.PositiveInfinity;
                var maxSideCoord = float.NegativeInfinity;
                for (var vertexIndex = 0; vertexIndex < poly.Length; vertexIndex++)
                {
                    var fromCenter = poly[vertexIndex] - center;
                    var upCoord = Vector3.Dot(fromCenter, faceUpAxisWorld);
                    var sideCoord = Vector3.Dot(fromCenter, faceSideAxisWorld);
                    minUpCoord = Mathf.Min(minUpCoord, upCoord);
                    maxUpCoord = Mathf.Max(maxUpCoord, upCoord);
                    minSideCoord = Mathf.Min(minSideCoord, sideCoord);
                    maxSideCoord = Mathf.Max(maxSideCoord, sideCoord);
                }

                var midUpCoord = (minUpCoord + maxUpCoord) * 0.5f;
                var hitUpCoord = Vector3.Dot(hit.point - center, faceUpAxisWorld);
                var isUpperHalf = hitUpCoord >= midUpCoord; // upper rectangle (as seen on the face)

                var upA = isUpperHalf ? midUpCoord : minUpCoord;
                var upB = isUpperHalf ? maxUpCoord : midUpCoord;

                var halfRect = new Vector3[4];
                halfRect[0] = center + faceUpAxisWorld * upB + faceSideAxisWorld * maxSideCoord;
                halfRect[1] = center + faceUpAxisWorld * upA + faceSideAxisWorld * maxSideCoord;
                halfRect[2] = center + faceUpAxisWorld * upA + faceSideAxisWorld * minSideCoord;
                halfRect[3] = center + faceUpAxisWorld * upB + faceSideAxisWorld * minSideCoord;

                // AxisSign==0 marks "special mode" (sloped face split).
                hover = new FaceHover(
                    go,
                    meshCollider,
                    hit.point,
                    faceNormalWorld,
                    center,
                    halfRect,
                    localAxis: isUpperHalf ? 1 : 0,
                    axisSign: 0f,
                    dragAxisCount: 1,
                    // Upper half: drag up (height). Lower half: drag along XZ length axis.
                    dragAxisAWorld: isUpperHalf ? objectUpAxisWorld : rampLengthAxisWorld,
                    dragAxisBWorld: Vector3.zero);
                return true;
            }

            var axis = GetDominantAxis(hitNormalLocal, out var sign);
            sign = sign >= 0f ? 1f : -1f;

            // For non-slope ramp faces: one-sided along the face normal.
            hover = new FaceHover(
                go,
                meshCollider,
                hit.point,
                nWorld,
                center,
                poly,
                axis,
                sign,
                dragAxisCount: 1,
                dragAxisAWorld: nWorld,
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
            hover = default;
            var go = drag.Hover.Target;
            if (go == null || !CanEdit(go))
                return false;
            if (drag.Hover.Collider is not MeshCollider mc)
                return false;

            if (drag.StartFacePolygonLocal == null || drag.StartFacePolygonLocal.Length < 3)
                return false;

            var targetTransform = go.transform;
            var poly = new Vector3[drag.StartFacePolygonLocal.Length];
            for (var i = 0; i < drag.StartFacePolygonLocal.Length; i++)
                poly[i] = targetTransform.TransformPoint(drag.StartFacePolygonLocal[i]);

            var center = Vector3.zero;
            for (var i = 0; i < poly.Length; i++)
                center += poly[i];
            center /= poly.Length;

            if (Mathf.Abs(drag.Hover.AxisSign) < 1e-6f)
            {
                // Keep the same slope half/mode as at drag start.
                hover = new FaceHover(
                    go,
                    mc,
                    drag.Hover.HitPointWorld,
                    drag.Hover.NormalWorld,
                    targetTransform.TransformPoint(drag.StartFaceCenterLocal),
                    poly,
                    drag.Hover.LocalAxis,
                    0f,
                    1,
                    drag.Hover.DragAxisAWorld,
                    Vector3.zero);
                return true;
            }

            hover = new FaceHover(go, mc, drag.Hover.HitPointWorld, drag.Hover.NormalWorld, targetTransform.TransformPoint(drag.StartFaceCenterLocal), poly, drag.Hover.LocalAxis, drag.Hover.AxisSign, 1, drag.Hover.DragAxisAWorld, Vector3.zero);
            return true;
        }

        public void ApplyDrag(in FaceDrag drag, Vector3 dragAxisWorld, float deltaAlongAxisWorld)
        {
            var go = drag.Hover.Target;
            if (go == null)
                return;

            var targetTransform = go.transform;

            // Sloped face special:
            if (Mathf.Abs(drag.Hover.AxisSign) < 1e-6f)
            {
                // For the sloped face we use the chosen half:
                // LocalAxis==1 → height mode, LocalAxis==0 → length mode.
                if (drag.Hover.LocalAxis == 1)
                    ApplyOneSidedAxis(drag, axis: 1, axisSign: +1f, deltaWorld: deltaAlongAxisWorld);
                else
                    ApplyOneSidedAxis(drag, axis: 0, axisSign: +1f, deltaWorld: deltaAlongAxisWorld);

                return;
            }

            ApplyOneSidedAxis(drag, drag.Hover.LocalAxis, drag.Hover.AxisSign, deltaAlongAxisWorld);
        }

        static void ApplyOneSidedAxis(in FaceDrag drag, int axis, float axisSign, float deltaWorld)
        {
            var go = drag.Hover.Target;
            if (go == null)
                return;
            var targetTransform = go.transform;

            var sign = axisSign >= 0f ? 1f : -1f;
            var normalLocal = axis switch
            {
                0 => Vector3.right * sign,
                1 => Vector3.up * sign,
                _ => Vector3.forward * sign,
            };
            var normalWorld = targetTransform.TransformDirection(normalLocal).normalized;

            var lossyAxis = Mathf.Abs(drag.StartLossyScale[axis]);
            if (lossyAxis <= 1e-6f)
                return;

            var deltaLocal = deltaWorld / lossyAxis;

            // Use the mesh local bounds so the fixed face stays in place even for non-centered pivots.
            var meshCollider = drag.Hover.Collider as MeshCollider;
            var localBounds = meshCollider != null && meshCollider.sharedMesh != null
                ? meshCollider.sharedMesh.bounds
                : new Bounds(Vector3.zero, Vector3.one);
            var localSize = localBounds.size;
            var sizeLocalAlongAxis = Mathf.Abs(localSize[axis]);
            if (sizeLocalAlongAxis <= 1e-6f)
                sizeLocalAlongAxis = 1f;

            var scaleDelta = (deltaLocal / sizeLocalAlongAxis) * drag.StartLocalScale[axis];
            var newScale = drag.StartLocalScale;
            newScale[axis] = Mathf.Max(0.01f, drag.StartLocalScale[axis] + scaleDelta);

            Undo.RecordObject(targetTransform, "One-sided Scale (Ramp)");
            targetTransform.localScale = newScale;

            // Keep the opposite face fixed in world space.
            // If we drag the +axis face (sign>0) we fix the min plane; if we drag the -axis face we fix the max plane.
            var fixedPlaneLocalCoordinate = sign > 0f ? localBounds.min[axis] : localBounds.max[axis];
            var translationAlongAxisLocal = fixedPlaneLocalCoordinate * (drag.StartLocalScale[axis] - newScale[axis]);
            var axisUnitLocal = axis switch
            {
                0 => Vector3.right,
                1 => Vector3.up,
                _ => Vector3.forward,
            };
            var translationWorld = targetTransform.TransformDirection(axisUnitLocal) * translationAlongAxisLocal;
            targetTransform.position = drag.StartWorldPosition + translationWorld;
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

