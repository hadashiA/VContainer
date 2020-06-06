namespace VContainer
{
    public interface IInjector
    {
        void Inject(object instance, IObjectResolver resolver);
        object CreateInstance(IObjectResolver resolver);
    }
}
