using UnityEngine;

namespace VContainer.Tests.Unity
{
    public interface IComponent2
    {
    }

    public class ServiceB
    {
    }

    public sealed class SampleMonoBehaviour2 : MonoBehaviour, IComponent
    {
        public ServiceB ServiceB;
        public bool StartCalled;
        public int UpdateCalls;

        void Start() => StartCalled = true;
        void Update() => UpdateCalls += 1;

        [Inject]
        public void Construct(ServiceB serviceA)
        {
            ServiceB = serviceA;
        }
    }
}