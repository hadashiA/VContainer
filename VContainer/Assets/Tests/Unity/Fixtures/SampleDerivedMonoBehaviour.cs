namespace VContainer.Tests.Unity
{
    public sealed class SampleDerivedMonoBehaviour : SampleBaseMonoBehaviour
    {
        public ServiceA Service;
        
        [Inject]
        public void Construct(ServiceA service)
        {
            Service = service;
        }
    }
}