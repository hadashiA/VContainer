using System;
using System.Linq;
using System.Collections.Generic;

namespace Reflex
{
    public class BindingGenericContractDefinition
    {
        private readonly Type _genericContract;
        private readonly Container _container;

        internal BindingGenericContractDefinition(Type genericContract, Container container)
        {
            _genericContract = genericContract;
            _container = container;
        }

        public BindingScopeDefinition To(params Type[] concretes)
        {
            var bindings = new List<Binding>(concretes.Length);

            foreach (var concrete in concretes)
            {
                var genericTypes = GetGenericTypes(_genericContract, concrete);
                var contract = _genericContract.MakeGenericType(genericTypes);
                var binding = new Binding
                {
                    Contract = contract,
                    Concrete = concrete
                };
                bindings.Add(binding);
                _container.Bindings.Add(contract, binding);
            }

            return new BindingScopeDefinition(bindings.ToArray());
        }

        private static Type[] GetGenericTypes(Type genericContract, Type type)
        {
            if (TryGetTypeGenericTypesAsInterface(type, genericContract, out var interfaceGenericArguments))
            {
                return interfaceGenericArguments;
            }

            if (TryGetTypeGenericTypesAsAbstract(type, genericContract, out var abstractGenericArguments))
            {
                return abstractGenericArguments;
            }

            throw new Exception($"Could not retrieve generic types from type '{type.Name}'.");
        }

        private static bool TryGetTypeGenericTypesAsInterface(Type type, Type contract, out Type[] genericArguments)
        {
            var genericInterfaces = type.GetInterfaces().Where(i => i.IsGenericType);
            var contractInterface = genericInterfaces.FirstOrDefault(i => i.GetGenericTypeDefinition() == contract);
            genericArguments = contractInterface?.GetGenericArguments();
            return genericArguments != null;
        }

        private static bool TryGetTypeGenericTypesAsAbstract(Type type, Type contract, out Type[] genericArguments)
        {
            genericArguments = type.BaseType != null &&
                               type.BaseType.IsGenericType &&
                               type.BaseType.GetGenericTypeDefinition() == contract
                ? type.BaseType.GetGenericArguments()
                : null;

            return genericArguments != null;
        }
    }
}