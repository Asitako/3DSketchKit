using NUnit.Framework;
using ThreeDSketchKit.Core.Components;
using ThreeDSketchKit.Editor.Rooms;
using UnityEngine;

namespace ThreeDSketchKit.Editor.Tests
{
    public sealed class RoomCommandValidationTests
    {
        [Test]
        public void Create_FailsWithSingleObject()
        {
            var a = new GameObject("A");
            try
            {
                var ok = RoomCommandValidation.TryValidateCreateFromSelection(
                    new[] { a },
                    out _,
                    out var err);
                Assert.IsFalse(ok);
                Assert.IsNotNull(err);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(a);
            }
        }

        [Test]
        public void Create_TwoUnownedObjects_Succeeds()
        {
            var a = new GameObject("A");
            var b = new GameObject("B");
            try
            {
                var ok = RoomCommandValidation.TryValidateCreateFromSelection(
                    new[] { a, b },
                    out var plan,
                    out var err);
                Assert.IsTrue(ok, err);
                Assert.IsNull(plan!.ParentForNewRoom);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(a);
                UnityEngine.Object.DestroyImmediate(b);
            }
        }

        [Test]
        public void Create_DifferentOwners_Fails()
        {
            var outerA = new GameObject("outerA");
            var outerB = new GameObject("outerB");
            var roomA = outerA.AddComponent<Room>();
            var roomB = outerB.AddComponent<Room>();
            var w1 = new GameObject("m1");
            var w2 = new GameObject("m2");
            w1.AddComponent<RoomMemberComponent>();
            w2.AddComponent<RoomMemberComponent>();
            w1.GetComponent<RoomMemberComponent>().OwnerRoom = roomA;
            w2.GetComponent<RoomMemberComponent>().OwnerRoom = roomB;
            try
            {
                var ok = RoomCommandValidation.TryValidateCreateFromSelection(
                    new[] { w1, w2 },
                    out _,
                    out _);
                Assert.IsFalse(ok);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(outerA);
                UnityEngine.Object.DestroyImmediate(outerB);
            }
        }

        [Test]
        public void Create_DifferentOwners_ResolvedFromChildMembers_Fails()
        {
            var o1 = new GameObject("O1");
            var r1 = o1.AddComponent<Room>();
            var g1 = new GameObject("G1");
            g1.transform.SetParent(o1.transform);
            var m1 = new GameObject("M1");
            m1.transform.SetParent(g1.transform);
            m1.AddComponent<RoomMemberComponent>();
            r1.AddMember(m1.GetComponent<RoomMemberComponent>());

            var o2 = new GameObject("O2");
            var r2 = o2.AddComponent<Room>();
            var g2 = new GameObject("G2");
            g2.transform.SetParent(o2.transform);
            var m2 = new GameObject("M2");
            m2.transform.SetParent(g2.transform);
            m2.AddComponent<RoomMemberComponent>();
            r2.AddMember(m2.GetComponent<RoomMemberComponent>());

            try
            {
                var ok = RoomCommandValidation.TryValidateCreateFromSelection(
                    new[] { g1, g2 },
                    out _,
                    out var err);
                Assert.IsFalse(ok, "Must not allow merging when wrappers belong to different rooms (members on children).");
                StringAssert.Contains("different room owners", err);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(o1);
                UnityEngine.Object.DestroyImmediate(o2);
            }
        }

        [Test]
        public void TryGetRegistration_OneObjectSubtree_TwoNonNullConflictingOwners_Fails()
        {
            var o1 = new GameObject("O1");
            var o2 = new GameObject("O2");
            var r1 = o1.AddComponent<Room>();
            var r2 = o2.AddComponent<Room>();
            var wrap = new GameObject("wrap");
            var a = new GameObject("a");
            var b = new GameObject("b");
            a.transform.SetParent(wrap.transform);
            b.transform.SetParent(wrap.transform);
            a.AddComponent<RoomMemberComponent>();
            b.AddComponent<RoomMemberComponent>();
            r1.AddMember(a.GetComponent<RoomMemberComponent>());
            r2.AddMember(b.GetComponent<RoomMemberComponent>());

            try
            {
                var ok = RoomCommandValidation.TryGetRegistrationOwnerForSelectionRoot(
                    wrap, out var owner, out var err);
                Assert.IsFalse(ok);
                Assert.IsNull(owner);
                Assert.IsNotNull(err);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(o1);
                UnityEngine.Object.DestroyImmediate(o2);
                UnityEngine.Object.DestroyImmediate(wrap);
            }
        }

        [Test]
        public void Create_SameOwner_MembersOnChildTransforms_SucceedsWithThatOwnerAsParent()
        {
            var outer = new GameObject("outer");
            var roomO = outer.AddComponent<Room>();
            var g1 = new GameObject("g1");
            var g2 = new GameObject("g2");
            g1.transform.SetParent(outer.transform);
            g2.transform.SetParent(outer.transform);
            var m1 = new GameObject("m1");
            var m2 = new GameObject("m2");
            m1.transform.SetParent(g1.transform);
            m2.transform.SetParent(g2.transform);
            m1.AddComponent<RoomMemberComponent>();
            m2.AddComponent<RoomMemberComponent>();
            roomO.AddMember(m1.GetComponent<RoomMemberComponent>());
            roomO.AddMember(m2.GetComponent<RoomMemberComponent>());
            var extra = new GameObject("g3");
            extra.transform.SetParent(outer.transform);
            try
            {
                var ok = RoomCommandValidation.TryValidateCreateFromSelection(
                    new[] { g1, g2, extra },
                    out var plan,
                    out var err);
                Assert.IsTrue(ok, err);
                Assert.AreSame(roomO, plan!.ParentForNewRoom, "k>=2 and same room → nest under that Room.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(outer);
            }
        }

        [Test]
        public void Create_TwoOfSameOwner_NotAllMembers_SucceedsWithParent()
        {
            var outer = new GameObject("outer");
            var roomO = outer.AddComponent<Room>();
            var w1 = new GameObject("w1");
            var w2 = new GameObject("w2");
            var w3 = new GameObject("w3");
            w1.AddComponent<RoomMemberComponent>();
            w2.AddComponent<RoomMemberComponent>();
            w3.AddComponent<RoomMemberComponent>();
            roomO.AddMember(w1.GetComponent<RoomMemberComponent>());
            roomO.AddMember(w2.GetComponent<RoomMemberComponent>());
            roomO.AddMember(w3.GetComponent<RoomMemberComponent>());

            try
            {
                var ok = RoomCommandValidation.TryValidateCreateFromSelection(
                    new[] { w1, w2 },
                    out var plan,
                    out var err);
                Assert.IsTrue(ok, err);
                Assert.AreSame(roomO, plan!.ParentForNewRoom);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(outer);
            }
        }

        [Test]
        public void Create_AllMembersOfRoom_Fails()
        {
            var outer = new GameObject("outer");
            var roomO = outer.AddComponent<Room>();
            var w1 = new GameObject("w1");
            var w2 = new GameObject("w2");
            w1.AddComponent<RoomMemberComponent>();
            w2.AddComponent<RoomMemberComponent>();
            roomO.AddMember(w1.GetComponent<RoomMemberComponent>());
            roomO.AddMember(w2.GetComponent<RoomMemberComponent>());

            try
            {
                var ok = RoomCommandValidation.TryValidateCreateFromSelection(
                    new[] { w1, w2 },
                    out _,
                    out _);
                Assert.IsFalse(ok);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(outer);
            }
        }

        [Test]
        public void NestingIndex_MatchesOnSameRoomDepth()
        {
            var a = new GameObject("A");
            var b = new GameObject("B");
            try
            {
                var i1 = RoomCommandValidation.NestingIndex(a.transform);
                var i2 = RoomCommandValidation.NestingIndex(b.transform);
                Assert.AreEqual(i1, i2);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(a);
                UnityEngine.Object.DestroyImmediate(b);
            }
        }
    }
}
