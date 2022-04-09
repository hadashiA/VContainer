namespace VContainer.Internal
{
    public sealed class ContainerLocal<T>
    {
        public readonly T Value;

        [Inject]
        public ContainerLocal(T value)
        {
            Value = value;
        }
    }
}
