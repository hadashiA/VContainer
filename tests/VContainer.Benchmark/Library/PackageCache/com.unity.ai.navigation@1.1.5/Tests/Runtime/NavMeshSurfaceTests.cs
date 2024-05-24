#if UNITY_EDITOR || UNITY_STANDALONE

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;

namespace Unity.AI.Navigation.Tests
{
    [TestFixture]
    class NavMeshSurfaceTests
    {
        GameObject plane;
        NavMeshSurface surface;

        [SetUp]
        public void CreatePlaneWithSurface()
        {
            plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            surface = new GameObject().AddComponent<NavMeshSurface>();
            Assert.IsFalse(HasNavMeshAtOrigin());
        }

        [TearDown]
        public void DestroyPlaneWithSurface()
        {
            GameObject.DestroyImmediate(plane);
            GameObject.DestroyImmediate(surface.gameObject);
            Assert.IsFalse(HasNavMeshAtOrigin());
        }

        [Test]
        public void NavMeshIsAvailableAfterBuild()
        {
            surface.BuildNavMesh();
            Assert.IsTrue(HasNavMeshAtOrigin());
        }

        [Test]
        public void NavMeshCanBeRemovedAndAdded()
        {
            surface.BuildNavMesh();
            Assert.IsTrue(HasNavMeshAtOrigin());

            surface.RemoveData();
            Assert.IsFalse(HasNavMeshAtOrigin());

            surface.AddData();
            Assert.IsTrue(HasNavMeshAtOrigin());
        }

        [Test]
        public void NavMeshIsNotAvailableWhenDisabled()
        {
            surface.BuildNavMesh();

            surface.enabled = false;
            Assert.IsFalse(HasNavMeshAtOrigin());

            surface.enabled = true;
            Assert.IsTrue(HasNavMeshAtOrigin());
        }

        [Test]
        public void CanBuildWithCustomArea()
        {
            surface.defaultArea = 4;
            var expectedAreaMask = 1 << 4;

            surface.BuildNavMesh();
            Assert.IsTrue(HasNavMeshAtOrigin(expectedAreaMask));
        }

        [Test]
        public void CanBuildWithCustomAgentTypeID()
        {
            surface.agentTypeID = 1234;
            surface.BuildNavMesh();

            Assert.IsTrue(HasNavMeshAtOrigin(NavMesh.AllAreas, 1234));
        }

        [Test]
        public void CanBuildCollidersAndIgnoreRenderMeshes()
        {
            plane.GetComponent<MeshRenderer>().enabled = false;

            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.BuildNavMesh();
            Assert.IsTrue(HasNavMeshAtOrigin());

            surface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
            surface.BuildNavMesh();
            Assert.IsFalse(HasNavMeshAtOrigin());
        }

        [Test]
        public void CanBuildRenderMeshesAndIgnoreColliders()
        {
#if NMC_CAN_ACCESS_PHYSICS
            plane.GetComponent<Collider>().enabled = false;
#else
        Assert.Inconclusive("This test requires the com.unity.modules.physics package in order to run. Make sure to reference it in the project.");
#endif
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.BuildNavMesh();
            Assert.IsFalse(HasNavMeshAtOrigin());

            surface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
            surface.BuildNavMesh();
            Assert.IsTrue(HasNavMeshAtOrigin());
        }

        [Test]
        public void BuildIgnoresGeometryOutsideBounds()
        {
            surface.collectObjects = CollectObjects.Volume;
            surface.center = new Vector3(20, 0, 0);
            surface.size = new Vector3(10, 10, 10);

            surface.BuildNavMesh();
            Assert.IsFalse(HasNavMeshAtOrigin());
        }

        [Test]
        public void BuildIgnoresGeometrySiblings()
        {
            surface.collectObjects = CollectObjects.Children;

            surface.BuildNavMesh();
            Assert.IsFalse(HasNavMeshAtOrigin());
        }

        [Test]
        public void BuildDoesntCullAreaBiggerThanMinRegionArea()
        {
            // Move plane away from NavMesh tile's boundaries
            plane.transform.localScale = new Vector3(0.25f, 0, 0.25f);
            plane.transform.position = new Vector3(2.5f, 0, 7.5f);

            surface.minRegionArea = 1f;

            surface.BuildNavMesh();

            Assert.IsTrue(HasNavMeshAtPosition(plane.transform.position));
        }

        [Test]
        public void BuildCullsAreaSmallerThanMinRegionArea()
        {
            // Move plane away from NavMesh tile's boundaries
            plane.transform.localScale = new Vector3(0.25f, 0, 0.25f);
            plane.transform.position = new Vector3(2.5f, 0, 7.5f);

            surface.minRegionArea = 5;

            surface.BuildNavMesh();

            Assert.IsFalse(HasNavMeshAtPosition(plane.transform.position));
        }

        [Test]
        public void BuildUsesOnlyIncludedLayers()
        {
            plane.layer = 4;
            surface.layerMask = ~(1 << 4);

            surface.BuildNavMesh();
            Assert.IsFalse(HasNavMeshAtOrigin());
        }

        [Test]
        public void DefaultSettingsMatchBuiltinSettings()
        {
            var bs = surface.GetBuildSettings();
            Assert.AreEqual(NavMesh.GetSettingsByIndex(0), bs);
        }

        [Test]
        public void ActiveSurfacesContainsOnlyActiveAndEnabledSurface()
        {
            Assert.IsTrue(NavMeshSurface.activeSurfaces.Contains(surface));
            Assert.AreEqual(1, NavMeshSurface.activeSurfaces.Count);

            surface.enabled = false;
            Assert.IsFalse(NavMeshSurface.activeSurfaces.Contains(surface));
            Assert.AreEqual(0, NavMeshSurface.activeSurfaces.Count);

            surface.enabled = true;
            surface.gameObject.SetActive(false);
            Assert.IsFalse(NavMeshSurface.activeSurfaces.Contains(surface));
            Assert.AreEqual(0, NavMeshSurface.activeSurfaces.Count);
        }

        [UnityTest]
        public IEnumerator NavMeshMovesToSurfacePositionNextFrame()
        {
            plane.transform.position = new Vector3(100, 0, 0);
            surface.transform.position = new Vector3(100, 0, 0);
            surface.BuildNavMesh();
            Assert.IsFalse(HasNavMeshAtOrigin());

            surface.transform.position = Vector3.zero;
            Assert.IsFalse(HasNavMeshAtOrigin());

            yield return null;

            Assert.IsTrue(HasNavMeshAtOrigin());
        }

        [UnityTest]
        public IEnumerator UpdatingAndAddingNavMesh()
        {
            var navmeshData = new NavMeshData();
            var oper = surface.UpdateNavMesh(navmeshData);
            Assert.IsFalse(HasNavMeshAtOrigin());

            do
            {
                yield return null;
            } while (!oper.isDone);

            surface.RemoveData();
            surface.navMeshData = navmeshData;
            surface.AddData();

            Assert.IsTrue(HasNavMeshAtOrigin());
        }

        [Test]
        public void BuildTakesIntoAccountAdjacentWalkableSurfacesOutsideBounds()
        {
            surface.collectObjects = CollectObjects.Volume;
            surface.center = new Vector3(0, 0, 0);
            surface.size = new Vector3(10, 10, 10);

            var adjacentPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            adjacentPlane.transform.position = new Vector3(10f, 0, 0);

            surface.BuildNavMesh();

            try
            {
                Assert.IsTrue(HasNavMeshAtPosition(new Vector3(surface.size.x / 2f, 0, 0)),
                    "A NavMesh should exists at the desired position.");
            }
            finally
            {
                Object.DestroyImmediate(adjacentPlane);
            }
        }

        static bool HasNavMeshAtPosition(Vector3 position, int areaMask = NavMesh.AllAreas, int agentTypeID = 0)
        {
            var filter = new NavMeshQueryFilter {areaMask = areaMask, agentTypeID = agentTypeID};
            return NavMesh.SamplePosition(position, out _, 0.1f, filter);
        }

        public static bool HasNavMeshAtOrigin(int areaMask = NavMesh.AllAreas, int agentTypeID = 0)
        {
            return HasNavMeshAtPosition(Vector3.zero, areaMask, agentTypeID);
        }
    }
}
#endif
