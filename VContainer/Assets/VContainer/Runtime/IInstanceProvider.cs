namespace VContainer
{
    public interface IInstanceProvider
    {
        object SpawnInstance(IObjectResolver resolver);
    }
}
