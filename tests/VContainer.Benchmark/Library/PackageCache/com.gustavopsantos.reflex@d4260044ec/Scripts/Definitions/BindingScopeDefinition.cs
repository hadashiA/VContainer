namespace Reflex
{
    public class BindingScopeDefinition
    {
        private readonly Binding[] _bindings;

        internal BindingScopeDefinition(params Binding[] bindings)
        {
            _bindings = bindings;
        }

        public void AsTransient()
        {
            foreach (var binding in _bindings)
            {
                binding.Scope = BindingScope.Transient;
            }
        }

        public void AsSingletonLazy()
        {
            foreach (var binding in _bindings)
            {
                binding.Scope = BindingScope.SingletonLazy;
            }
        }

        public void AsSingletonNonLazy()
        {
            foreach (var binding in _bindings)
            {
                binding.Scope = BindingScope.SingletonNonLazy;
            }
        }
    }
}