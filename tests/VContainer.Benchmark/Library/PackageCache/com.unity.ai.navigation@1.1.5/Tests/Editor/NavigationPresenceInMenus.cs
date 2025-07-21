using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Unity.AI.Navigation.Editor.Tests
{
    [TestFixture]
    [Description("Verifies that the desired Navigation editor menus are accessible with the package.")]
    public class NavigationPresenceInMenus
    {
        GameObject m_ComponentsReceiver;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // Create an empty game object and select it in order for components menus to be available
            m_ComponentsReceiver = new GameObject("ComponentsReceiver");
            Selection.activeObject = m_ComponentsReceiver;
        }

        static IEnumerable<string> NavigationMenuItemProvider()
        {
            yield return "Component/Navigation/Nav Mesh Agent";
            yield return "Component/Navigation/Nav Mesh Obstacle";
            yield return "Component/Navigation/Off Mesh Link";
            yield return "Component/Navigation/NavMeshSurface";
            yield return "Component/Navigation/NavMeshModifierVolume";
            yield return "Component/Navigation/NavMeshModifier";
            yield return "Component/Navigation/NavMeshLink";
            yield return "Window/AI/Navigation";
#if UNITY_2022_2_OR_NEWER
            yield return "Window/AI/Navigation (Obsolete)";
#endif
        }

        [Test]
        [TestCaseSource(nameof(NavigationMenuItemProvider))]
        public void MenuIsEnabled(string menuPath)
        {
            var menuEnabled = Menu.GetEnabled(menuPath);
            Assert.That(menuEnabled, Is.True, $"Navigation component menu '{menuPath}' should be available");
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Object.DestroyImmediate(m_ComponentsReceiver);
        }
    }
}
