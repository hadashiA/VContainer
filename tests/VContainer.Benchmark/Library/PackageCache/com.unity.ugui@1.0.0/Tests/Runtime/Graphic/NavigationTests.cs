using System.Reflection;
using System.Collections;
using NUnit.Framework;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

namespace UnityEngine.UI.Tests
{
    [TestFixture]
    class NavigationTests
    {
        GameObject canvasRoot;
        Selectable topLeftSelectable;
        Selectable bottomLeftSelectable;
        Selectable topRightSelectable;
        Selectable bottomRightSelectable;

        [SetUp]
        public void TestSetup()
        {
            canvasRoot = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            GameObject topLeftGO = new GameObject("topLeftGO", typeof(RectTransform), typeof(CanvasRenderer), typeof(Selectable));
            topLeftGO.transform.SetParent(canvasRoot.transform);
            (topLeftGO.transform as RectTransform).anchoredPosition = new Vector2(50, 200);
            topLeftSelectable = topLeftGO.GetComponent<Selectable>();

            GameObject bottomLeftGO = new GameObject("bottomLeftGO", typeof(RectTransform), typeof(CanvasRenderer), typeof(Selectable));
            bottomLeftGO.transform.SetParent(canvasRoot.transform);
            (bottomLeftGO.transform as RectTransform).anchoredPosition = new Vector2(50, 50);
            bottomLeftSelectable = bottomLeftGO.GetComponent<Selectable>();

            GameObject topRightGO = new GameObject("topRightGO", typeof(RectTransform), typeof(CanvasRenderer), typeof(Selectable));
            topRightGO.transform.SetParent(canvasRoot.transform);
            (topRightGO.transform as RectTransform).anchoredPosition = new Vector2(200, 200);
            topRightSelectable = topRightGO.GetComponent<Selectable>();

            GameObject bottomRightGO = new GameObject("bottomRightGO", typeof(RectTransform), typeof(CanvasRenderer), typeof(Selectable));
            bottomRightGO.transform.SetParent(canvasRoot.transform);
            (bottomRightGO.transform as RectTransform).anchoredPosition = new Vector2(200, 50);
            bottomRightSelectable = bottomRightGO.GetComponent<Selectable>();
        }

        [TearDown]
        public void TearDown()
        {
            GameObject.DestroyImmediate(canvasRoot);
        }

        [Test]
        public void FindSelectableOnRight_ReturnsNextSelectableRightOfTarget()
        {
            Selectable selectableRightOfTopLeft = topLeftSelectable.FindSelectableOnRight();
            Selectable selectableRightOfBottomLeft = bottomLeftSelectable.FindSelectableOnRight();

            Assert.AreEqual(topRightSelectable, selectableRightOfTopLeft, "Wrong selectable to right of Top Left Selectable");
            Assert.AreEqual(bottomRightSelectable, selectableRightOfBottomLeft, "Wrong selectable to right of Bottom Left Selectable");
        }

        [Test]
        public void FindSelectableOnLeft_ReturnsNextSelectableLeftOfTarget()
        {
            Selectable selectableLeftOfTopRight = topRightSelectable.FindSelectableOnLeft();
            Selectable selectableLeftOfBottomRight = bottomRightSelectable.FindSelectableOnLeft();

            Assert.AreEqual(topLeftSelectable, selectableLeftOfTopRight, "Wrong selectable to left of Top Right Selectable");
            Assert.AreEqual(bottomLeftSelectable, selectableLeftOfBottomRight, "Wrong selectable to left of Bottom Right Selectable");
        }

        [Test]
        public void FindSelectableOnRDown_ReturnsNextSelectableBelowTarget()
        {
            Selectable selectableDownOfTopLeft = topLeftSelectable.FindSelectableOnDown();
            Selectable selectableDownOfTopRight = topRightSelectable.FindSelectableOnDown();

            Assert.AreEqual(bottomLeftSelectable, selectableDownOfTopLeft, "Wrong selectable to Bottom of Top Left Selectable");
            Assert.AreEqual(bottomRightSelectable, selectableDownOfTopRight, "Wrong selectable to Bottom of top Right Selectable");
        }

        [Test]
        public void FindSelectableOnUp_ReturnsNextSelectableAboveTarget()
        {
            Selectable selectableUpOfBottomLeft = bottomLeftSelectable.FindSelectableOnUp();
            Selectable selectableUpOfBottomRight = bottomRightSelectable.FindSelectableOnUp();

            Assert.AreEqual(topLeftSelectable, selectableUpOfBottomLeft, "Wrong selectable to Up of bottom Left Selectable");
            Assert.AreEqual(topRightSelectable, selectableUpOfBottomRight, "Wrong selectable to Up of bottom Right Selectable");
        }

        [Test]
        public void FindSelectableOnRight__WrappingEnabled_ReturnsFurthestSelectableOnLeft()
        {
            Navigation nav = topRightSelectable.navigation;
            nav.wrapAround = true;
            nav.mode = Navigation.Mode.Horizontal;
            topRightSelectable.navigation = nav;

            nav = bottomRightSelectable.navigation;
            nav.wrapAround = true;
            nav.mode = Navigation.Mode.Horizontal;
            bottomRightSelectable.navigation = nav;

            Selectable selectableRightOfTopRight = topRightSelectable.FindSelectableOnRight();
            Selectable selectableRightOfBottomRight = bottomRightSelectable.FindSelectableOnRight();

            Assert.AreEqual(bottomLeftSelectable, selectableRightOfTopRight, "Wrong selectable to right of Top Right Selectable");
            Assert.AreEqual(topLeftSelectable, selectableRightOfBottomRight, "Wrong selectable to right of Bottom Right Selectable");
        }

        [Test]
        public void FindSelectableOnLeft_WrappingEnabled_ReturnsFurthestSelectableOnRight()
        {
            Navigation nav = topLeftSelectable.navigation;
            nav.wrapAround = true;
            nav.mode = Navigation.Mode.Horizontal;
            topLeftSelectable.navigation = nav;

            nav = bottomLeftSelectable.navigation;
            nav.wrapAround = true;
            nav.mode = Navigation.Mode.Horizontal;
            bottomLeftSelectable.navigation = nav;

            Selectable selectableLeftOfTopLeft = topLeftSelectable.FindSelectableOnLeft();
            Selectable selectableLeftOfBottomLeft = bottomLeftSelectable.FindSelectableOnLeft();

            Assert.AreEqual(bottomRightSelectable, selectableLeftOfTopLeft, "Wrong selectable to left of Top Left Selectable");
            Assert.AreEqual(topRightSelectable, selectableLeftOfBottomLeft, "Wrong selectable to left of Bottom Left Selectable");
        }

        [Test]
        public void FindSelectableOnDown_WrappingEnabled_ReturnsFurthestSelectableAbove()
        {
            Navigation nav = bottomLeftSelectable.navigation;
            nav.wrapAround = true;
            nav.mode = Navigation.Mode.Vertical;
            bottomLeftSelectable.navigation = nav;

            nav = bottomRightSelectable.navigation;
            nav.wrapAround = true;
            nav.mode = Navigation.Mode.Vertical;
            bottomRightSelectable.navigation = nav;

            Selectable selectableDownOfBottomLeft = bottomLeftSelectable.FindSelectableOnDown();
            Selectable selectableDownOfBottomRight = bottomRightSelectable.FindSelectableOnDown();

            Assert.AreEqual(topRightSelectable, selectableDownOfBottomLeft, "Wrong selectable to Bottom of Bottom Left Selectable");
            Assert.AreEqual(topLeftSelectable, selectableDownOfBottomRight, "Wrong selectable to Bottom of Bottom Right Selectable");
        }

        [Test]
        public void FindSelectableOnUp_WrappingEnabled_ReturnsFurthestSelectableBelow()
        {
            Navigation nav = topLeftSelectable.navigation;
            nav.wrapAround = true;
            nav.mode = Navigation.Mode.Vertical;
            topLeftSelectable.navigation = nav;

            nav = topRightSelectable.navigation;
            nav.wrapAround = true;
            nav.mode = Navigation.Mode.Vertical;
            topRightSelectable.navigation = nav;

            Selectable selectableUpOfTopLeft = topLeftSelectable.FindSelectableOnUp();
            Selectable selectableUpOfTopRight = topRightSelectable.FindSelectableOnUp();

            Assert.AreEqual(bottomRightSelectable, selectableUpOfTopLeft, "Wrong selectable to Up of Top Left Selectable");
            Assert.AreEqual(bottomLeftSelectable, selectableUpOfTopRight, "Wrong selectable to Up of Top Right Selectable");
        }
    }
}
