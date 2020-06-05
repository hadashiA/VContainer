using System;
using System.Collections.Concurrent;
using VContainer.Internal;

namespace VContainer
{
    public interface IObjectResolver
    {
        object Resolve(Type type);
        IScopedObjectResolver BeginScope(Action<ContainerBuilder> configuration = null);
    }

    public static class ObjectResolverExtensions
    {
        public static object Resolve<T>(this IObjectResolver resolver) => resolver.Resolve(typeof(T));
    }

    public interface IScopedObjectResolver : IObjectResolver, IDisposable
    {
        IObjectResolver Root { get; }
        IScopedObjectResolver Parent { get; }
    }

    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class InjectAttribute : Attribute
    {
    }

    public enum Lifetime
    {
        Transient,
        Singleton,
        Scoped,
    }

    public sealed class ScopedContainer : IScopedObjectResolver
    {
        public IObjectResolver Root { get; }
        public IScopedObjectResolver Parent { get; }

        readonly IRegistry registry;
        readonly ConcurrentDictionary<Type, Lazy<object>> sharedInstances = new ConcurrentDictionary<Type, Lazy<object>>();
        readonly CompositeDisposable disposables = new CompositeDisposable();

        internal ScopedContainer(
            IRegistry registry,
            IObjectResolver root,
            IScopedObjectResolver parent = null)
        {
            this.registry = registry;
            Root = root;
            Parent = parent;
        }

        public object Resolve(Type type)
        {
            var registration = registry.Get(type);
            switch (registration.Lifetime)
            {
                case Lifetime.Transient:
                    return registration.Injector.CreateInstance(this);

                case Lifetime.Singleton:
                    return Root.Resolve(type);

                case Lifetime.Scoped:
                    var lazy = sharedInstances.GetOrAdd(registration.ImplementationType, _ =>
                    {
                        return new Lazy<object>(() => registration.Injector.CreateInstance(this));
                    });
                    if (!lazy.IsValueCreated && lazy.Value is IDisposable disposable)
                    {
                        disposables.Add(disposable);
                    }
                    return lazy.Value;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public IScopedObjectResolver BeginScope(Action<ContainerBuilder> configuration = null)
            => throw new NotImplementedException();

        public void Dispose() => disposables.Dispose();
    }

    public sealed class Container : IObjectResolver, IDisposable
    {
        readonly IRegistry registry;
        readonly IScopedObjectResolver rootScope;
        readonly ConcurrentDictionary<Type, Lazy<object>> sharedInstances = new ConcurrentDictionary<Type, Lazy<object>>();

        internal Container(IRegistry registry)
        {
            this.registry = registry;
            rootScope = new ScopedContainer(registry, this);
        }

        public object Resolve(Type type)
        {
            var registration = registry.Get(type);
            switch (registration.Lifetime)
            {
                case Lifetime.Transient:
                    return registration.Injector.CreateInstance(this);

                case Lifetime.Singleton:
                    return sharedInstances.GetOrAdd(registration.ImplementationType, _ =>
                    {
                        return new Lazy<object>(() => registration.Injector.CreateInstance(this));
                    }).Value;

                case Lifetime.Scoped:
                    return rootScope.Resolve(type);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public IScopedObjectResolver BeginScope(Action<ContainerBuilder> configuration = null)
            => rootScope.BeginScope(configuration);

        public void Dispose() => rootScope.Dispose();
    }
}
