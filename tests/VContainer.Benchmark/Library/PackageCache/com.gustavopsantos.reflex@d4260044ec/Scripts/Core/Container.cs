using System;
using System.Linq;
using System.Collections.Generic;
using Reflex.Injectors;
using Reflex.Scripts.Utilities;

namespace Reflex
{
    public class Container
    {
        internal readonly Dictionary<Type, Binding> Bindings = new Dictionary<Type, Binding>(); // TContract, Binding
        internal readonly Dictionary<Type, object> Singletons = new Dictionary<Type, object>(); // TContract, Instance

        internal Resolver SingletonNonLazyResolver = null;
        private readonly Resolver MethodResolver = new MethodResolver();
        private readonly Resolver TransientResolver = new TransientResolver();
        private readonly Resolver SingletonLazyResolver = new SingletonLazyResolver();

        public Container()
        {
            BindSingleton<Container>(this);
        }

        public T Construct<T>()
        {
            return ConstructorInjector.ConstructAndInject<T>(this);
        }
        
        public object Construct(Type concrete)
        {
            return ConstructorInjector.ConstructAndInject(concrete, this);
        }

        public void Clear()
        {
            Bindings.Clear();
            Singletons.Clear();
        }
        
        public BindingContractDefinition<TContract> Bind<TContract>()
        {
            return new BindingContractDefinition<TContract>(this);
        }

        public void BindSingleton<TContract>(TContract instance)
        {
            var binding = new Binding
            {
                Contract = typeof(TContract),
                Concrete = instance.GetType(),
                Scope = BindingScope.SingletonLazy
            };
            
            Bindings.Add(typeof(TContract), binding);
            Singletons.Add(typeof(TContract), instance);
        }

        public void BindSingleton<T>(Type contract, T instance)
        {
            var binding = new Binding
            {
                Contract = contract,
                Concrete = instance.GetType(),
                Scope = BindingScope.SingletonLazy
            };
            
            Bindings.Add(contract, binding);
            Singletons.Add(contract, instance);
        }

        public BindingGenericContractDefinition BindGenericContract(Type genericContract)
        {
            return new BindingGenericContractDefinition(genericContract, this);
        }

        public TContract Resolve<TContract>()
        {
            return (TContract) Resolve(typeof(TContract));
        }

        public object Resolve(Type contract)
        {
            if (Bindings.TryGetValue(contract, out var binding))
            {
                switch (binding.Scope)
                {
                    case BindingScope.Method: return MethodResolver.Resolve(contract, this);
                    case BindingScope.Transient: return TransientResolver.Resolve(contract, this);
                    case BindingScope.SingletonLazy: return SingletonLazyResolver.Resolve(contract, this);
                    case BindingScope.SingletonNonLazy: return SingletonNonLazyResolver.Resolve(contract, this);
                    default: throw new ScopeNotHandledException($"BindingScope '{binding.Scope}' not handled.");
                }
            }

            throw BuildException(contract);
        }

        public TCast ResolveGenericContract<TCast>(Type genericContract, params Type[] genericConcrete)
        {
            var contract = genericContract.MakeGenericType(genericConcrete);
            return (TCast) Resolve(contract);
        }

        private static UnknownContractException BuildException(Type contract)
        {
            if (!contract.IsGenericType)
            {
                return new UnknownContractException($"Cannot resolve contract type '{contract}'.");
            }

            var genericContract = contract.Name.Remove(contract.Name.IndexOf('`'));
            var genericArguments = contract.GenericTypeArguments.Select(args => args.FullName);
            var commaSeparatedArguments = string.Join(", ", genericArguments);
            var message = $"Cannot resolve contract type '{genericContract}<{commaSeparatedArguments}>'.";
            return new UnknownContractException(message);
        }

        internal Type GetConcreteTypeFor(Type contract)
        {
            return Bindings[contract].Concrete;
        }

        internal object RegisterSingletonInstance(Type contract, object concrete)
        {
            Singletons.Add(contract, concrete);
            return concrete;
        }

        internal bool TryGetSingletonInstance(Type contract, out object instance)
        {
            return Singletons.TryGetValue(contract, out instance);
        }

        internal bool TryGetMethod(Type contract, out Func<object> method)
        {
            if (Bindings.TryGetValue(contract, out var binding))
            {
                if (binding.Scope == BindingScope.Method)
                {
                    method = binding.Method;
                    return true;
                }
            }

            method = null;
            return false;
        }
        
        internal void InstantiateNonLazySingletons()
        {
            SingletonNonLazyResolver = new SingletonLazyResolver();
            Bindings.Values.Where(IsSingletonNonLazy).ForEach(binding => Resolve(binding.Contract));
            SingletonNonLazyResolver = new SingletonNonLazyResolver();
        }

        private static bool IsSingletonNonLazy(Binding binding)
        {
            return binding.Scope == BindingScope.SingletonNonLazy;
        }
    }
}