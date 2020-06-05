namespace VContainer
{
    public interface IInjector
    {
        object CreateInstance(IObjectResolver resolver);
    }
}
