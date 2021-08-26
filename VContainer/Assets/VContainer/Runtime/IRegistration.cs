using System;
using System.Collections.Generic;

namespace VContainer
{
    /// <summary>
    /// A record of a dependency, how long it should exist, and the types that
    /// can be used to resolve it.
    /// </summary>
    /// <remarks>
    /// Application code likely won't need to use this interface (or its implementations)
    /// explicitly.
    /// </remarks>
    /// <seealso cref="IContainerBuilder"/>
    /// <seealso cref="IObjectResolver"/>
    /// <seealso cref="Lifetime"/>
    public interface IRegistration
    {
        /// <summary>
        /// The actual <see cref="Type"/> of the object that's managed by this registration.
        /// </summary>
        /// <remarks>
        /// The registration might or might not be resolvable by this <see cref="Type"/>,
        /// depending on how the originating <see cref="IContainerBuilder"/> was
        /// configured.
        /// </remarks>
        Type ImplementationType { get; }

        /// <summary>
        /// The <see cref="Type"/>s that can be used to resolve this registration's
        /// object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This list might or might not include <see cref="ImplementationType"/>,
        /// depending on how the originating <see cref="IContainerBuilder"/> was
        /// configured.
        /// </para>
        /// <para>
        /// <see cref="ImplementationType"/> must be assignable to all types given in this list.
        /// </para>
        /// <para>
        /// No particular order of the elements is guaranteed.
        /// </para>
        /// </remarks>
        IReadOnlyList<Type> InterfaceTypes { get; }

        /// <summary>
        /// The lifetime of the object returned by <see cref="SpawnInstance"/>.
        /// </summary>
        /// <remarks>
        /// This property should not affect the logic of <see cref="SpawnInstance"/>.
        /// Lifetime management should be left to implementations of <see cref="IObjectResolver"/>.
        /// </remarks>
        Lifetime Lifetime { get; }

        /// <summary>
        /// Constructs a new object of the type given in <see cref="ImplementationType"/>
        /// and injects it with all dependencies it requires.
        /// </summary>
        /// <remarks>
        /// Implementations of <see cref="IObjectResolver"/> are responsible for
        /// managing the lifetime of returned objects. Therefore, the value of <see cref="Lifetime"/>
        /// should not affect the logic of this method.
        /// </remarks>
        /// <param name="resolver">
        /// The container that will be used to inject the spawned object's dependencies.
        /// </param>
        /// <returns>
        /// A new object of whatever type is referred to by <see cref="ImplementationType"/>.
        /// </returns>
        /// <exception cref="VContainerException">
        /// A dependency couldn't be resolved.
        /// </exception>
        /// <seealso cref="InjectAttribute"/>
        /// <seealso cref="IObjectResolver.Inject"/>
        object SpawnInstance(IObjectResolver resolver);
    }
}
