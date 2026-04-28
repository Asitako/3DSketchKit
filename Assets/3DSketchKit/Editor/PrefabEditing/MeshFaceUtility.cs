using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ThreeDSketchKit.Editor.PrefabEditing
{
    static class MeshFaceUtility
    {
        public static bool TryGetConnectedCoplanarFacePolygon(MeshCollider collider, RaycastHit hit, float normalDotThreshold, out Vector3[] polygonWorld, out Vector3 normalWorld)
        {
            polygonWorld = null;
            normalWorld = default;
            if (collider == null || collider.sharedMesh == null)
                return false;
            if (hit.triangleIndex < 0)
                return false;

            var sharedMesh = collider.sharedMesh;
            var triangleVertexIndices = sharedMesh.triangles;
            var meshVerticesLocal = sharedMesh.vertices;
            if (triangleVertexIndices == null || triangleVertexIndices.Length < 3)
                return false;

            var triStart = hit.triangleIndex * 3;
            if (triStart + 2 >= triangleVertexIndices.Length)
                return false;

            var colliderTransform = collider.transform;
            var hitNormalLocal = colliderTransform.InverseTransformDirection(hit.normal).normalized;

            // Flood-fill triangles that are (a) coplanar by normal and (b) vertex-connected.
            var visitedTriangleIndices = new HashSet<int>();
            var trianglesToVisit = new Queue<int>();
            trianglesToVisit.Enqueue(hit.triangleIndex);
            visitedTriangleIndices.Add(hit.triangleIndex);

            // Build vertex->triangle map (small meshes only).
            var vertexToTriangleIndices = new Dictionary<int, List<int>>();
            var triangleCount = triangleVertexIndices.Length / 3;
            for (var triangleIndex = 0; triangleIndex < triangleCount; triangleIndex++)
            {
                var triangleStart = triangleIndex * 3;
                for (var k = 0; k < 3; k++)
                {
                    var vertexIndex = triangleVertexIndices[triangleStart + k];
                    if (!vertexToTriangleIndices.TryGetValue(vertexIndex, out var triangleList))
                    {
                        triangleList = new List<int>();
                        vertexToTriangleIndices[vertexIndex] = triangleList;
                    }
                    triangleList.Add(triangleIndex);
                }
            }

            while (trianglesToVisit.Count > 0)
            {
                var currentTriangleIndex = trianglesToVisit.Dequeue();
                var triangleStart = currentTriangleIndex * 3;
                var v0Local = meshVerticesLocal[triangleVertexIndices[triangleStart + 0]];
                var v1Local = meshVerticesLocal[triangleVertexIndices[triangleStart + 1]];
                var v2Local = meshVerticesLocal[triangleVertexIndices[triangleStart + 2]];
                var triangleNormalLocal = Vector3.Cross(v1Local - v0Local, v2Local - v0Local).normalized;
                if (Mathf.Abs(Vector3.Dot(triangleNormalLocal, hitNormalLocal)) < normalDotThreshold)
                    continue;

                // Expand to all triangles sharing any of the vertices.
                for (var k = 0; k < 3; k++)
                {
                    var vertexIndex = triangleVertexIndices[triangleStart + k];
                    if (!vertexToTriangleIndices.TryGetValue(vertexIndex, out var neighborTriangles))
                        continue;
                    for (var neighborIndex = 0; neighborIndex < neighborTriangles.Count; neighborIndex++)
                    {
                        var neighborTriangleIndex = neighborTriangles[neighborIndex];
                        if (visitedTriangleIndices.Add(neighborTriangleIndex))
                            trianglesToVisit.Enqueue(neighborTriangleIndex);
                    }
                }
            }

            // Collect boundary directed edges using cancellation: if both directions exist, it's internal.
            var boundaryDirectedEdges = new HashSet<(int from, int to)>();
            foreach (var triangleIndex in visitedTriangleIndices)
            {
                var triangleStart = triangleIndex * 3;
                var v0 = triangleVertexIndices[triangleStart + 0];
                var v1 = triangleVertexIndices[triangleStart + 1];
                var v2 = triangleVertexIndices[triangleStart + 2];
                AddOrCancelDirectedEdge(boundaryDirectedEdges, v0, v1);
                AddOrCancelDirectedEdge(boundaryDirectedEdges, v1, v2);
                AddOrCancelDirectedEdge(boundaryDirectedEdges, v2, v0);
            }

            if (boundaryDirectedEdges.Count < 3)
                return false;

            // Unique boundary vertices.
            var boundaryVertexIndices = new HashSet<int>();
            foreach (var edge in boundaryDirectedEdges)
            {
                boundaryVertexIndices.Add(edge.from);
                boundaryVertexIndices.Add(edge.to);
            }

            if (boundaryVertexIndices.Count < 3)
                return false;

            // Order vertices by angle around the face normal in LOCAL space.
            var boundaryVerticesLocal = boundaryVertexIndices
                .Select(vertexIndex => meshVerticesLocal[vertexIndex])
                .ToList();

            var centroidLocal = Vector3.zero;
            for (var i = 0; i < boundaryVerticesLocal.Count; i++)
                centroidLocal += boundaryVerticesLocal[i];
            centroidLocal /= boundaryVerticesLocal.Count;

            var axisU = Vector3.Cross(hitNormalLocal, Vector3.up);
            if (axisU.sqrMagnitude < 1e-6f)
                axisU = Vector3.Cross(hitNormalLocal, Vector3.right);
            axisU.Normalize();
            var axisV = Vector3.Cross(hitNormalLocal, axisU).normalized;

            boundaryVerticesLocal.Sort((left, right) =>
            {
                var leftOffset = left - centroidLocal;
                var rightOffset = right - centroidLocal;
                var leftAngle = Mathf.Atan2(Vector3.Dot(leftOffset, axisV), Vector3.Dot(leftOffset, axisU));
                var rightAngle = Mathf.Atan2(Vector3.Dot(rightOffset, axisV), Vector3.Dot(rightOffset, axisU));
                return leftAngle.CompareTo(rightAngle);
            });

            var polygonVerticesWorld = new Vector3[boundaryVerticesLocal.Count];
            for (var i = 0; i < boundaryVerticesLocal.Count; i++)
                polygonVerticesWorld[i] = colliderTransform.TransformPoint(boundaryVerticesLocal[i]);

            polygonWorld = polygonVerticesWorld;
            normalWorld = hit.normal.normalized;
            return true;
        }

        static void AddOrCancelDirectedEdge(HashSet<(int from, int to)> edges, int from, int to)
        {
            var opposite = (from: to, to: from);
            if (!edges.Remove(opposite))
                edges.Add((from, to));
        }
    }
}

