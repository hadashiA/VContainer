using UnityEngine;

public class MouseUpdate : MonoBehaviour
{
    FakeBaseInput m_FakeBaseInput;

    void Awake()
    {
        m_FakeBaseInput = GetComponent<FakeBaseInput>();
    }

    void Update()
    {
        Debug.Assert(m_FakeBaseInput, "FakeBaseInput component has not been added to the EventSystem");

        // Update mouse position
        m_FakeBaseInput.MousePosition.x += 10f;
        m_FakeBaseInput.MousePosition.y += 10f;
    }
}
