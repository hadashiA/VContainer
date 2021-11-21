namespace VContainer.Internal
{
    public sealed class ContainerLocal<T>
    {
        public readonly T Value;

        public ContainerLocal(T value)
        {
            Value = value;
        }
    }
}
