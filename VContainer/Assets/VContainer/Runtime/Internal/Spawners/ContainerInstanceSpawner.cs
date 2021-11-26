namespace VContainer.Internal.Spawners
{
    sealed class ContainerInstanceSpawner : IInstanceSpawner
    {
        public static readonly ContainerInstanceSpawner Default = new ContainerInstanceSpawner();

        public object Spawn(IObjectResolver resolver) => resolver;
    }
}
