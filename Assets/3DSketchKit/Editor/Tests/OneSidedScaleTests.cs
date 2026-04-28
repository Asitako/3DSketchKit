using NUnit.Framework;
using ThreeDSketchKit.Editor.PrefabEditing;
using UnityEngine;

namespace ThreeDSketchKit.Editor.Tests
{
    public sealed class OneSidedScaleTests
    {
        static readonly Vector3[] AxisVectors =
        {
            Vector3.right,
            Vector3.up,
            Vector3.forward
        };

        [Test]
        public void OneSidedScale_OppositeFaceStaysFixed_AllAxes_StretchThenShrink_And_ShrinkThenStretch()
        {
            // Phase A: stretch 3 steps, then shrink 2 steps (still relative to initial grab).
            RunScenario(initialScale: new Vector3(1f, 0.25f, 1f), stretchSteps: 3, shrinkSteps: 2, startWithStretch: true);

            // Phase B: shrink 2 steps, then stretch 3 steps.
            RunScenario(initialScale: new Vector3(1f, 0.25f, 1f), stretchSteps: 3, shrinkSteps: 2, startWithStretch: false);
        }

        static void RunScenario(Vector3 initialScale, int stretchSteps, int shrinkSteps, bool startWithStretch)
        {
            const float step = 0.25f;

            for (var axis = 0; axis < 3; axis++)
            {
                for (var sign = -1f; sign <= 1f; sign += 2f)
                {
                    var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    try
                    {
                        go.transform.position = new Vector3(2.3f, -1.7f, 4.9f);
                        go.transform.rotation = Quaternion.identity;
                        go.transform.localScale = initialScale;

                        var box = go.GetComponent<BoxCollider>();
                        Assert.IsNotNull(box);
                        box.size = Vector3.one;
                        box.center = Vector3.zero;

                        var module = new BoxBlockOneSidedScaleModule();

                        var normalWorld = AxisVectors[axis] * sign;
                        var hover = new FaceHover(
                            go,
                            box,
                            hitPointWorld: Vector3.zero,
                            normalWorld: normalWorld,
                            faceCenterWorld: Vector3.zero,
                            faceCornersWorld: new Vector3[4],
                            localAxis: axis,
                            axisSign: sign);

                        Assert.IsTrue(module.TryBeginDrag(hover, out var drag));

                        var oppositeStart = GetFaceCoordinateAlongAxis(go, axis, -sign);

                        if (startWithStretch)
                        {
                            // Stretch: delta 1,2,3 steps
                            for (var i = 1; i <= stretchSteps; i++)
                                ApplyAndAssert(module, drag, axis, -sign, oppositeStart, step * i);

                            // Shrink: delta 2,1 steps (back towards start)
                            for (var i = stretchSteps - 1; i >= stretchSteps - shrinkSteps; i--)
                                ApplyAndAssert(module, drag, axis, -sign, oppositeStart, step * i);
                        }
                        else
                        {
                            // Shrink: negative deltas -1, -2 steps
                            for (var i = 1; i <= shrinkSteps; i++)
                                ApplyAndAssert(module, drag, axis, -sign, oppositeStart, -step * i);

                            // Stretch: back to -1, 0, +1, +2, +3 (but we only care the opposite face stays put).
                            for (var i = shrinkSteps - 1; i >= 0; i--)
                                ApplyAndAssert(module, drag, axis, -sign, oppositeStart, -step * i);
                            for (var i = 1; i <= stretchSteps; i++)
                                ApplyAndAssert(module, drag, axis, -sign, oppositeStart, step * i);
                        }
                    }
                    finally
                    {
                        UnityEngine.Object.DestroyImmediate(go);
                    }
                }
            }
        }

        static void ApplyAndAssert(BoxBlockOneSidedScaleModule module, FaceDrag drag, int axis, float oppositeSign, float oppositeStart, float delta)
        {
            module.ApplyDrag(drag, delta);
            var oppositeNow = GetFaceCoordinateAlongAxis(drag.Hover.Target, axis, oppositeSign);
            Assert.That(oppositeNow, Is.EqualTo(oppositeStart).Within(1e-4f),
                $"Opposite face drifted on axis={axis} for delta={delta}. start={oppositeStart}, now={oppositeNow}");
        }

        static float GetFaceCoordinateAlongAxis(GameObject go, int axis, float sign)
        {
            var b = go.GetComponent<Collider>().bounds; // world AABB is fine for identity rotation
            var center = b.center;
            var ext = b.extents;
            var p = center;
            p[axis] += (sign >= 0f ? 1f : -1f) * ext[axis];
            return p[axis];
        }
    }
}

