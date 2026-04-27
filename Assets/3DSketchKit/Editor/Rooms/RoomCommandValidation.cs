using System;
using System.Collections.Generic;
using System.Linq;
using ThreeDSketchKit.Core.Components;
using UnityEngine;

namespace ThreeDSketchKit.Editor.Rooms
{
    /// <summary>
    /// Invariants for Create / Unpack (used by the editor window and unit tests; no <see cref="UnityEditor.Undo"/>).
    /// </summary>
    public static class RoomCommandValidation
    {
        /// <summary>Max <see cref="Room"/> on one branch after Create (A.4).</summary>
        public const int MaxRoomCountAlongSingleBranchAfterCreate = 3;

        public static bool TryValidateCreateFromSelection(
            IReadOnlyList<GameObject> selectedRootObjects,
            out RoomCreatePlan plan,
            out string errorMessage)
        {
            plan = null;
            errorMessage = null;

            if (selectedRootObjects == null)
            {
                errorMessage = "No selection.";
                return false;
            }

            if (selectedRootObjects.Count < 2)
            {
                errorMessage = "Select at least two root objects. You cannot form a room from a single object.";
                return false;
            }

            var roots = new List<Transform>(selectedRootObjects.Count);
            foreach (var o in selectedRootObjects)
            {
                if (o == null)
                {
                    errorMessage = "Selection contains a null object.";
                    return false;
                }

                roots.Add(o.transform);
            }

            var firstNesting = NestingIndex(roots[0]);
            for (var i = 1; i < roots.Count; i++)
            {
                if (NestingIndex(roots[i]) != firstNesting)
                {
                    errorMessage = "All selected objects must be on the same room nesting level.";
                    return false;
                }
            }

            Room singleNonNullOwner = null;
            var k = 0;
            foreach (var o in selectedRootObjects)
            {
                if (!TryGetRegistrationOwnerForSelectionRoot(o, out var w, out var perObjectError))
                {
                    errorMessage = perObjectError;
                    return false;
                }

                if (w == null)
                    continue;
                k++;
                if (singleNonNullOwner == null)
                    singleNonNullOwner = w;
                else if (w != singleNonNullOwner)
                {
                    errorMessage = "Selected objects belong to different room owners. Unpack or adjust the selection.";
                    return false;
                }
            }
            if (k > 0)
            {
                if (singleNonNullOwner == null)
                {
                    errorMessage = "Internal: expected a single room owner when members are present.";
                    return false;
                }

                if (k == selectedRootObjects.Count
                    && IsSelectionExactlyTheMemberListOf(singleNonNullOwner, selectedRootObjects)
                    && k >= 1)
                {
                    errorMessage =
                        "You cannot form a new room from every member of another room. " +
                        "Select a subset, add unregistered objects, or use Unpack first.";
                    return false;
                }
            }

            Room parent;
            if (k == 0)
                parent = null;
            else if (k == 1)
                parent = null;
            else
                parent = singleNonNullOwner;

            plan = new RoomCreatePlan(parent);
            if (!IsDepthValidAfterCreate(selectedRootObjects, plan, out var maxDepth))
            {
                errorMessage =
                    $"The deepest branch would contain {maxDepth} room components (max {MaxRoomCountAlongSingleBranchAfterCreate}). " +
                    "Reduce nesting, unpack a room, or use fewer nested rooms in the selection.";
                plan = null;
                return false;
            }

            return true;
        }

        public static int NestingIndex(Transform transform)
        {
            if (transform == null)
                return 0;
            var n = 0;
            for (var t = transform; t != null; t = t.parent)
            {
                if (t.GetComponent<Room>() != null)
                    n++;
            }

            return n;
        }

        /// <summary>
        /// Resolves which <see cref="Room"/> registers this selection root (or children). Uses room references, not names.
        /// Fails if the subtree has two <see cref="RoomMemberComponent"/>s with different non-null <see cref="RoomMemberComponent.OwnerRoom"/>.
        /// </summary>
        public static bool TryGetRegistrationOwnerForSelectionRoot(GameObject go, out Room owner, out string errorMessage)
        {
            owner = null;
            errorMessage = null;
            if (go == null)
                return true;
            var members = go.GetComponentsInChildren<RoomMemberComponent>(true);
            if (members == null || members.Length == 0)
                return true;

            Room firstNonNullOwner = null;
            foreach (var m in members)
            {
                if (m == null)
                    continue;
                var r = m.OwnerRoom as Room;
                if (r == null)
                    continue;
                if (firstNonNullOwner == null)
                    firstNonNullOwner = r;
                else if (r != firstNonNullOwner)
                {
                    errorMessage =
                        "One of the selected objects contains multiple room members registered under different rooms. " +
                        "Unpack or adjust the selection.";
                    return false;
                }
            }

            owner = firstNonNullOwner;
            return true;
        }

        public static bool IsSelectionExactlyTheMemberListOf(Room room, IReadOnlyList<GameObject> selected)
        {
            if (room == null || selected == null)
                return false;
            var memberObjects = new HashSet<GameObject>();
            foreach (var m in room.Members)
            {
                var roomMember = m as RoomMemberComponent;
                if (roomMember != null)
                    memberObjects.Add(roomMember.gameObject);
            }

            if (memberObjects.Count == 0 || memberObjects.Count != selected.Count)
                return false;
            return selected.All(g => g != null && memberObjects.Contains(g)) && memberObjects.All(g => selected.Contains(g));
        }

        public static int CountRoomComponentsInSelfAndAncestors(Transform t)
        {
            var n = 0;
            for (var p = t; p != null; p = p.parent)
            {
                if (p.GetComponent<Room>() != null)
                    n++;
            }

            return n;
        }

        public static int GetMaxRoomCountOnPathInSubtree(Transform subtreeRoot)
        {
            if (subtreeRoot == null)
                return 0;
            var best = 0;
            foreach (var descendant in subtreeRoot.GetComponentsInChildren<Transform>(true))
            {
                if (descendant == null)
                    continue;
                if (!IsDescendantOrSelf(subtreeRoot, descendant))
                    continue;
                best = Math.Max(best, CountRoomComponentsOnPathFromAncestorToDescendant(subtreeRoot, descendant));
            }

            return best;
        }

        public static bool IsDescendantOrSelf(Transform ancestor, Transform candidate)
        {
            for (var t = candidate; t != null; t = t.parent)
            {
                if (t == ancestor)
                    return true;
            }

            return false;
        }

        public static int CountRoomComponentsOnPathFromAncestorToDescendant(Transform ancestor, Transform descendant)
        {
            if (ancestor == null || descendant == null)
                return 0;
            if (!IsDescendantOrSelf(ancestor, descendant))
                return 0;
            var n = 0;
            for (var t = descendant; t != null; t = t.parent)
            {
                if (t.GetComponent<Room>() != null)
                    n++;
                if (t == ancestor)
                    return n;
            }

            return 0;
        }

        public static int MaxDepthAfterPlacingNewRoom(
            IReadOnlyList<GameObject> selectedRootObjects,
            Room parentForNew)
        {
            if (selectedRootObjects == null || selectedRootObjects.Count == 0)
                return 0;
            var prefix = parentForNew == null
                ? 0
                : CountRoomComponentsInSelfAndAncestors(parentForNew.transform);
            var maxD = 0;
            foreach (var go in selectedRootObjects)
            {
                if (go == null)
                    continue;
                var down = GetMaxRoomCountOnPathInSubtree(go.transform);
                var branch = prefix + 1 + down;
                if (branch > maxD)
                    maxD = branch;
            }

            return maxD;
        }

        public static bool IsDepthValidAfterCreate(
            IReadOnlyList<GameObject> selected,
            RoomCreatePlan plan,
            out int maxDepth)
        {
            return IsDepthValidAfterCreate(selected, plan == null ? null : plan.ParentForNewRoom, out maxDepth);
        }

        public static bool IsDepthValidAfterCreate(
            IReadOnlyList<GameObject> selected,
            Room parentForNew,
            out int maxDepth)
        {
            maxDepth = MaxDepthAfterPlacingNewRoom(selected, parentForNew);
            return maxDepth <= MaxRoomCountAlongSingleBranchAfterCreate;
        }
    }
}
