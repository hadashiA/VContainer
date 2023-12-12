using UnityEngine;

namespace VContainer.Tests.Unity
{
    public interface IComponent
    {
    }

    public class ServiceA
    {
    }

    public sealed class SampleMonoBehaviour : MonoBehaviour, IComponent
    {
        public ServiceA ServiceA;
        public ServiceA ServiceAInAwake;
        public bool StartCalled;
        public int UpdateCalls;

        void Start() => StartCalled = true;
        void Update() => UpdateCalls += 1;

        void Awake()
        {
            ServiceAInAwake = ServiceA;
        }

        [Inject]
        public void Construct(ServiceA serviceA)
        {
            ServiceA = serviceA;
        }
    }
}
