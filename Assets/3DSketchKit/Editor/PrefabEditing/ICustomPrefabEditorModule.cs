using UnityEngine;

namespace ThreeDSketchKit.Editor.PrefabEditing
{
    public interface ICustomPrefabEditorModule
    {
        bool CanEdit(GameObject gameObject);

        /// <summary>Returns hover info for face under cursor (used for highlight).</summary>
        bool TryGetHover(Ray ray, out FaceHover hover);

        /// <summary>Begins a drag operation if the current hover is draggable.</summary>
        bool TryBeginDrag(in FaceHover hover, out FaceDrag drag);

        /// <summary>
        /// Returns an updated hover for the dragged face (used to keep highlight/arrow on the moved face).
        /// </summary>
        bool TryGetDragFace(in FaceDrag drag, out FaceHover hover);

        /// <summary>Applies scaling using a world-space delta along a selected drag axis.</summary>
        void ApplyDrag(in FaceDrag drag, Vector3 dragAxisWorld, float deltaAlongAxisWorld);
    }

    public readonly struct FaceHover
    {
        public readonly GameObject Target;
        public readonly Collider Collider;
        public readonly Vector3 HitPointWorld;
        public readonly Vector3 NormalWorld;
        public readonly Vector3 FaceCenterWorld;
        public readonly Vector3[] FacePolygonWorld; // 3+ vertices (convex), world-space
        public readonly int LocalAxis; // 0=x,1=y,2=z (dominant axis for the face normal in local space)
        public readonly float AxisSign; // +1 / -1 for the selected face along LocalAxis
        public readonly int DragAxisCount; // 1 or 2
        public readonly Vector3 DragAxisAWorld;
        public readonly Vector3 DragAxisBWorld;

        public FaceHover(
            GameObject target,
            Collider collider,
            Vector3 hitPointWorld,
            Vector3 normalWorld,
            Vector3 faceCenterWorld,
            Vector3[] facePolygonWorld,
            int localAxis,
            float axisSign,
            int dragAxisCount,
            Vector3 dragAxisAWorld,
            Vector3 dragAxisBWorld)
        {
            Target = target;
            Collider = collider;
            HitPointWorld = hitPointWorld;
            NormalWorld = normalWorld;
            FaceCenterWorld = faceCenterWorld;
            FacePolygonWorld = facePolygonWorld;
            LocalAxis = localAxis;
            AxisSign = axisSign;
            DragAxisCount = dragAxisCount;
            DragAxisAWorld = dragAxisAWorld;
            DragAxisBWorld = dragAxisBWorld;
        }
    }

    public readonly struct FaceDrag
    {
        public readonly FaceHover Hover;
        public readonly Vector3 StartLocalScale;
        public readonly Vector3 StartWorldPosition;
        public readonly Vector3 StartLossyScale;
        public readonly Vector3 StartFaceCenterLocal;
        public readonly Vector3[] StartFacePolygonLocal;

        public FaceDrag(
            FaceHover hover,
            Vector3 startLocalScale,
            Vector3 startWorldPosition,
            Vector3 startLossyScale,
            Vector3 startFaceCenterLocal,
            Vector3[] startFacePolygonLocal)
        {
            Hover = hover;
            StartLocalScale = startLocalScale;
            StartWorldPosition = startWorldPosition;
            StartLossyScale = startLossyScale;
            StartFaceCenterLocal = startFaceCenterLocal;
            StartFacePolygonLocal = startFacePolygonLocal;
        }
    }
}

