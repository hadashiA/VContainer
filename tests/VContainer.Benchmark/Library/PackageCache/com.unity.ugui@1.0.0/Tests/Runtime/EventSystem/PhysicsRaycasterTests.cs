using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class PhysicsRaycasterTests
{
    GameObject m_CamGO;
    GameObject m_Collider;

    [SetUp]
    public void TestSetup()
    {
        m_CamGO = new GameObject("PhysicsRaycaster Camera");
        m_Collider = GameObject.CreatePrimitive(PrimitiveType.Cube);
    }

    [Test]
    public void PhysicsRaycasterDoesNotCastOutsideCameraViewRect()
    {
        m_CamGO.transform.position = new Vector3(0, 0, -10);
        m_CamGO.transform.LookAt(Vector3.zero);
        var cam = m_CamGO.AddComponent<Camera>();
        cam.rect = new Rect(0.5f, 0, 0.5f, 1);
        m_CamGO.AddComponent<PhysicsRaycaster>();
        var eventSystem = m_CamGO.AddComponent<EventSystem>();

        // Create an object that will be hit if a raycast does occur.
        m_Collider.transform.localScale = new Vector3(100, 100, 1);
        List<RaycastResult> results = new List<RaycastResult>();
        var pointerEvent = new PointerEventData(eventSystem)
        {
            position = new Vector2(0, 0) // Raycast from the left side of the screen which is outside of the camera's view rect.
        };

        eventSystem.RaycastAll(pointerEvent, results);
        Assert.IsEmpty(results, "Expected no results from a raycast that is outside of the camera's viewport.");
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(m_CamGO);
        GameObject.DestroyImmediate(m_Collider);
    }
}
