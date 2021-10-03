using System;

namespace VContainer.Internal
{
    public interface IRegistry
    {
        // void Add(Registration registration);
        bool TryGet(Type interfaceType, out IRegistration registration);
        bool Exists(Type implementationType);
    }
}