using System;

namespace VContainer
{
    public interface IClosedRegistrationProvider
    {
        Registration GetClosedRegistration(Type closedInterfaceType, Type[] typeParameters, object key = null);
    }
}