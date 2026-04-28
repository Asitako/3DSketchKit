using NUnit.Framework;
using ThreeDSketchKit.Editor.PrefabEditing;
using UnityEngine;

namespace ThreeDSketchKit.Editor.Tests
{
    public sealed class OneSidedScalePillarRampTests
    {
        const float StepWorld = 0.25f;
        const float Tolerance = 1e-4f;

        [Test]
        public void Pillar_Caps_OneSidedScale_FixedOppositeCap_AllDirections()
        {
            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            try
            {
                pillar.name = "Pillar_Test";
                pillar.transform.position = new Vector3(1.3f, -0.7f, 2.1f);
                pillar.transform.rotation = Quaternion.Euler(15f, 37f, 8f);
                pillar.transform.localScale = new Vector3(0.25f, 0.5f, 0.25f);

                var capsuleCollider = pillar.GetComponent<CapsuleCollider>();
                Assert.IsNotNull(capsuleCollider);
                var meshFilter = pillar.GetComponent<MeshFilter>();
                Assert.IsNotNull(meshFilter);
                Assert.IsNotNull(meshFilter.sharedMesh);

                var module = new PillarScaleModule();
                var meshBoundsLocal = meshFilter.sharedMesh.bounds;

                // Drag top and bottom caps.
                for (var capSign = -1f; capSign <= 1f; capSign += 2f)
                {
                    var capNormalWorld = pillar.transform.TransformDirection(Vector3.up * capSign).normalized;
                    var capHover = CreateHoverForTest(
                        pillar,
                        capsuleCollider,
                        normalWorld: capNormalWorld,
                        localAxis: 1,
                        axisSign: capSign);

                    Assert.IsTrue(module.TryBeginDrag(capHover, out var drag));

                    // If dragging +Y, fixed plane is minY; if dragging -Y, fixed plane is maxY.
                    var fixedPlaneLocalY = capSign > 0f ? meshBoundsLocal.min.y : meshBoundsLocal.max.y;
                    var fixedPointWorldStart = GetPlaneAnchorWorld(pillar.transform, meshBoundsLocal, axis: 1, fixedPlaneLocalCoordinate: fixedPlaneLocalY);

                    RunStretchThenShrink(module, drag, capNormalWorld, fixedPointWorldStart, meshBoundsLocal, axis: 1, fixedPlaneLocalCoordinate: fixedPlaneLocalY);
                    RunShrinkThenStretch(module, drag, capNormalWorld, fixedPointWorldStart, meshBoundsLocal, axis: 1, fixedPlaneLocalCoordinate: fixedPlaneLocalY);
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(pillar);
            }
        }

        [Test]
        public void Pillar_Side_UniformRadiusScale_NoPositionDrift_And_XZStayEqual()
        {
            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            try
            {
                pillar.name = "Pillar_Test";
                pillar.transform.position = new Vector3(-2.2f, 1.1f, 0.4f);
                pillar.transform.rotation = Quaternion.Euler(10f, 15f, 5f);
                pillar.transform.localScale = new Vector3(0.25f, 0.5f, 0.25f);

                var capsuleCollider = pillar.GetComponent<CapsuleCollider>();
                Assert.IsNotNull(capsuleCollider);

                var module = new PillarScaleModule();

                // Simulate grabbing the curved side: normal is some horizontal-ish direction.
                var radialDirectionWorld = pillar.transform.TransformDirection(Vector3.right).normalized;
                var sideHover = CreateHoverForTest(
                    pillar,
                    capsuleCollider,
                    normalWorld: radialDirectionWorld,
                    localAxis: 0,
                    axisSign: 1f);

                Assert.IsTrue(module.TryBeginDrag(sideHover, out var drag));

                var startPosition = pillar.transform.position;
                var startScale = pillar.transform.localScale;

                // Stretch 3 steps.
                for (var stepIndex = 1; stepIndex <= 3; stepIndex++)
                {
                    module.ApplyDrag(drag, radialDirectionWorld, StepWorld * stepIndex);
                    Assert.That(pillar.transform.position, Is.EqualTo(startPosition).Using(Vector3Comparer(Tolerance)));
                    Assert.That(pillar.transform.localScale.x, Is.EqualTo(pillar.transform.localScale.z).Within(Tolerance));
                    Assert.That(pillar.transform.localScale.x, Is.GreaterThanOrEqualTo(startScale.x));
                }

                // Shrink back 2 steps.
                for (var stepIndex = 2; stepIndex >= 1; stepIndex--)
                {
                    module.ApplyDrag(drag, radialDirectionWorld, StepWorld * stepIndex);
                    Assert.That(pillar.transform.position, Is.EqualTo(startPosition).Using(Vector3Comparer(Tolerance)));
                    Assert.That(pillar.transform.localScale.x, Is.EqualTo(pillar.transform.localScale.z).Within(Tolerance));
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(pillar);
            }
        }

        [Test]
        public void Ramp_AxisFaces_OneSidedScale_FixedOppositeFace_AllAxesBothSides_WithRotation()
        {
            var ramp = new GameObject("Ramp_Test");
            try
            {
                ramp.transform.position = new Vector3(3.4f, 0.2f, -1.8f);
                ramp.transform.rotation = Quaternion.Euler(23f, 61f, 11f);
                ramp.transform.localScale = new Vector3(1f, 0.5f, 0.5f);

                var rampMesh = CreateRampWedgeMesh();
                var meshCollider = ramp.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = rampMesh;

                var module = new RampScaleModule();
                var meshBoundsLocal = rampMesh.bounds;

                // Test +/- on X/Y/Z (excluding the sloped split mode; we cover it separately below).
                for (var axis = 0; axis < 3; axis++)
                {
                    for (var axisSign = -1f; axisSign <= 1f; axisSign += 2f)
                    {
                        var faceNormalLocal = AxisUnitLocal(axis) * axisSign;
                        var faceNormalWorld = ramp.transform.TransformDirection(faceNormalLocal).normalized;

                        var hover = CreateHoverForTest(
                            ramp,
                            meshCollider,
                            normalWorld: faceNormalWorld,
                            localAxis: axis,
                            axisSign: axisSign);

                        Assert.IsTrue(module.TryBeginDrag(hover, out var drag));

                        var fixedPlaneLocalCoordinate = axisSign > 0f ? meshBoundsLocal.min[axis] : meshBoundsLocal.max[axis];
                        var fixedPointWorldStart = GetPlaneAnchorWorld(ramp.transform, meshBoundsLocal, axis, fixedPlaneLocalCoordinate);

                        RunStretchThenShrink(module, drag, faceNormalWorld, fixedPointWorldStart, meshBoundsLocal, axis, fixedPlaneLocalCoordinate);
                        RunShrinkThenStretch(module, drag, faceNormalWorld, fixedPointWorldStart, meshBoundsLocal, axis, fixedPlaneLocalCoordinate);
                    }
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(ramp);
            }
        }

        [Test]
        public void Ramp_SlopedFace_SplitModes_HeightAndLength_FixedOppositeReferencePlanes_WithRotation()
        {
            var ramp = new GameObject("Ramp_Test");
            try
            {
                ramp.transform.position = new Vector3(-1.2f, 2.3f, 0.7f);
                ramp.transform.rotation = Quaternion.Euler(17f, 44f, 5f);
                ramp.transform.localScale = new Vector3(1f, 0.5f, 0.5f);

                var rampMesh = CreateRampWedgeMesh();
                var meshCollider = ramp.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = rampMesh;

                var module = new RampScaleModule();
                var meshBoundsLocal = rampMesh.bounds;

                // Height mode: LocalAxis==1, AxisSign==0.
                {
                    var hover = CreateHoverForTest(
                        ramp,
                        meshCollider,
                        normalWorld: ramp.transform.TransformDirection(new Vector3(1f, 1f, 0.2f)).normalized, // any "sloped-ish" normal
                        localAxis: 1,
                        axisSign: 0f);

                    Assert.IsTrue(module.TryBeginDrag(hover, out var drag));

                    // Fixed bottom plane for height: local minY.
                    var fixedPlaneLocalY = meshBoundsLocal.min.y;
                    var fixedPointWorldStart = GetPlaneAnchorWorld(ramp.transform, meshBoundsLocal, axis: 1, fixedPlaneLocalCoordinate: fixedPlaneLocalY);

                    RunStretchThenShrink(module, drag, ramp.transform.TransformDirection(Vector3.up).normalized, fixedPointWorldStart, meshBoundsLocal, axis: 1, fixedPlaneLocalCoordinate: fixedPlaneLocalY);
                }

                // Length mode: LocalAxis==0, AxisSign==0.
                {
                    var hover = CreateHoverForTest(
                        ramp,
                        meshCollider,
                        normalWorld: ramp.transform.TransformDirection(new Vector3(1f, 1f, 0.2f)).normalized,
                        localAxis: 0,
                        axisSign: 0f);

                    Assert.IsTrue(module.TryBeginDrag(hover, out var drag));

                    // Fixed back plane for length: local minX.
                    var fixedPlaneLocalX = meshBoundsLocal.min.x;
                    var fixedPointWorldStart = GetPlaneAnchorWorld(ramp.transform, meshBoundsLocal, axis: 0, fixedPlaneLocalCoordinate: fixedPlaneLocalX);

                    RunStretchThenShrink(module, drag, ramp.transform.TransformDirection(Vector3.right).normalized, fixedPointWorldStart, meshBoundsLocal, axis: 0, fixedPlaneLocalCoordinate: fixedPlaneLocalX);
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(ramp);
            }
        }

        [Test]
        public void Ramp_SideFaces_OneSidedScale_FixedOppositeSideFace_BothSides_WithRotation()
        {
            var ramp = new GameObject("Ramp_Test");
            try
            {
                ramp.transform.position = new Vector3(0.8f, -0.4f, 3.2f);
                ramp.transform.rotation = Quaternion.Euler(12f, 73f, 19f);
                ramp.transform.localScale = new Vector3(1f, 0.5f, 0.5f);

                var rampMesh = CreateRampWedgeMesh();
                var meshCollider = ramp.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = rampMesh;

                var module = new RampScaleModule();
                var meshBoundsLocal = rampMesh.bounds;

                const int sideAxis = 2; // local Z
                for (var sideSign = -1f; sideSign <= 1f; sideSign += 2f)
                {
                    var faceNormalLocal = AxisUnitLocal(sideAxis) * sideSign;
                    var faceNormalWorld = ramp.transform.TransformDirection(faceNormalLocal).normalized;

                    var hover = CreateHoverForTest(
                        ramp,
                        meshCollider,
                        normalWorld: faceNormalWorld,
                        localAxis: sideAxis,
                        axisSign: sideSign);

                    Assert.IsTrue(module.TryBeginDrag(hover, out var drag));

                    var fixedPlaneLocalCoordinate = sideSign > 0f ? meshBoundsLocal.min.z : meshBoundsLocal.max.z;
                    var fixedPointWorldStart = GetPlaneAnchorWorld(ramp.transform, meshBoundsLocal, sideAxis, fixedPlaneLocalCoordinate);

                    RunStretchThenShrink(module, drag, faceNormalWorld, fixedPointWorldStart, meshBoundsLocal, sideAxis, fixedPlaneLocalCoordinate);
                    RunShrinkThenStretch(module, drag, faceNormalWorld, fixedPointWorldStart, meshBoundsLocal, sideAxis, fixedPlaneLocalCoordinate);
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(ramp);
            }
        }

        static void RunStretchThenShrink(ICustomPrefabEditorModule module, FaceDrag drag, Vector3 dragAxisWorld, Vector3 fixedPointWorldStart, Bounds meshBoundsLocal, int axis, float fixedPlaneLocalCoordinate)
        {
            for (var stepIndex = 1; stepIndex <= 3; stepIndex++)
                ApplyAndAssertFixedPlane(module, drag, dragAxisWorld, fixedPointWorldStart, meshBoundsLocal, axis, fixedPlaneLocalCoordinate, StepWorld * stepIndex);

            for (var stepIndex = 2; stepIndex >= 1; stepIndex--)
                ApplyAndAssertFixedPlane(module, drag, dragAxisWorld, fixedPointWorldStart, meshBoundsLocal, axis, fixedPlaneLocalCoordinate, StepWorld * stepIndex);
        }

        static void RunShrinkThenStretch(ICustomPrefabEditorModule module, FaceDrag drag, Vector3 dragAxisWorld, Vector3 fixedPointWorldStart, Bounds meshBoundsLocal, int axis, float fixedPlaneLocalCoordinate)
        {
            for (var stepIndex = 1; stepIndex <= 2; stepIndex++)
                ApplyAndAssertFixedPlane(module, drag, dragAxisWorld, fixedPointWorldStart, meshBoundsLocal, axis, fixedPlaneLocalCoordinate, -StepWorld * stepIndex);

            for (var stepIndex = 1; stepIndex <= 3; stepIndex++)
                ApplyAndAssertFixedPlane(module, drag, dragAxisWorld, fixedPointWorldStart, meshBoundsLocal, axis, fixedPlaneLocalCoordinate, StepWorld * stepIndex);
        }

        static void ApplyAndAssertFixedPlane(
            ICustomPrefabEditorModule module,
            FaceDrag drag,
            Vector3 dragAxisWorld,
            Vector3 fixedPointWorldStart,
            Bounds meshBoundsLocal,
            int axis,
            float fixedPlaneLocalCoordinate,
            float deltaWorld)
        {
            module.ApplyDrag(drag, dragAxisWorld, deltaWorld);
            var fixedPointWorldNow = GetPlaneAnchorWorld(drag.Hover.Target.transform, meshBoundsLocal, axis, fixedPlaneLocalCoordinate);
            Assert.That(fixedPointWorldNow, Is.EqualTo(fixedPointWorldStart).Using(Vector3Comparer(Tolerance)),
                $"Fixed plane drifted on axis={axis} for delta={deltaWorld}. start={fixedPointWorldStart}, now={fixedPointWorldNow}");
        }

        static FaceHover CreateHoverForTest(GameObject target, Collider collider, Vector3 normalWorld, int localAxis, float axisSign)
        {
            // Face polygon is not used by the scaling math; we provide a minimal triangle to satisfy drag state.
            var facePolygonWorld = new[]
            {
                target.transform.position,
                target.transform.position + target.transform.right * 0.1f,
                target.transform.position + target.transform.up * 0.1f,
            };

            return new FaceHover(
                target,
                collider,
                hitPointWorld: target.transform.position,
                normalWorld: normalWorld.normalized,
                faceCenterWorld: target.transform.position,
                facePolygonWorld: facePolygonWorld,
                localAxis: localAxis,
                axisSign: axisSign,
                dragAxisCount: 1,
                dragAxisAWorld: normalWorld.normalized,
                dragAxisBWorld: Vector3.zero);
        }

        static Vector3 GetPlaneAnchorWorld(Transform targetTransform, Bounds meshBoundsLocal, int axis, float fixedPlaneLocalCoordinate)
        {
            var localPoint = meshBoundsLocal.center;
            localPoint[axis] = fixedPlaneLocalCoordinate;
            return targetTransform.TransformPoint(localPoint);
        }

        static Vector3 AxisUnitLocal(int axis)
        {
            return axis switch
            {
                0 => Vector3.right,
                1 => Vector3.up,
                _ => Vector3.forward,
            };
        }

        static Mesh CreateRampWedgeMesh()
        {
            var mesh = new Mesh { name = "RampWedge_Test" };

            var vertex0 = new Vector3(0f, 0f, 0f);
            var vertex1 = new Vector3(1f, 0f, 0f);
            var vertex2 = new Vector3(0f, 0f, 1f);
            var vertex3 = new Vector3(0f, 1f, 0f);
            var vertex4 = new Vector3(1f, 1f, 0f);
            var vertex5 = new Vector3(0f, 1f, 1f);

            var vertices = new[] { vertex0, vertex1, vertex2, vertex3, vertex4, vertex5 };
            mesh.vertices = vertices;

            mesh.triangles = new[]
            {
                0, 1, 2,
                3, 5, 4,
                0, 3, 5,
                0, 5, 2,
                0, 1, 4,
                0, 4, 3,
                1, 2, 5,
                1, 5, 4
            };

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        static System.Collections.IComparer Vector3Comparer(float tolerance)
        {
            return new Vector3EqualityComparer(tolerance);
        }

        sealed class Vector3EqualityComparer : System.Collections.IComparer
        {
            readonly float _tolerance;
            public Vector3EqualityComparer(float tolerance) => _tolerance = tolerance;

            public int Compare(object x, object y)
            {
                var a = (Vector3)x;
                var b = (Vector3)y;
                if (Vector3.Distance(a, b) <= _tolerance)
                    return 0;
                return a.sqrMagnitude.CompareTo(b.sqrMagnitude);
            }
        }
    }
}

