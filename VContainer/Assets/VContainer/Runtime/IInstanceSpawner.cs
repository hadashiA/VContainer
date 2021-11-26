namespace VContainer
{
    public interface IInstanceSpawner
    {
        object Spawn(IObjectResolver resolver);
    }
}
