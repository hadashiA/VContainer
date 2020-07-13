# VContainer

![](https://github.com/hadashiA/VContainer/workflows/Test/badge.svg)
[![openupm](https://img.shields.io/npm/v/jp.hadashikick.vcontainer?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/jp.hadashikick.vcontainer/)

**VContainer** is an DI (Dependency Injection) library running on Unity (Game Engine).

"V" means making Unity's initial "U" more thinner and solid ... !

- Fast resolve. Minimum GC allocation. (For using reflection. Because it targets Unity IL2CPP.)
- Extra small code size. Few internal types.
- Application can freely create nested Lifetime Scope
- Dispatch your custom types to Unity's PlayerLoopSystem. (It is greatly influenced by Zenject)
- Immutable Container.

![](docs/unity_performance_test_result.png)

- This benchmark was run by [Performance Testing Extension for Unity Test Runner](https://docs.unity3d.com/Packages/com.unity.test-framework.performance@1.0/manual/index.html).  Test cases is [here](VContainer.Benchmark).
- :warning: This benchmark does not use Zenject's Reflection baking.
  - Currently, VContainer does not support pre-build IL weaving or C# code generation optimizations, but it is planned for a future version.
  - VContainer does all type-analyze in Container Build phase. To reduce main thread blokcing time, you can consider [Async Contaienr Build](#async-contaienr-build).


And in resolve, We have zero allocation (without resolved instances).

Following is a deep profiler Unity result sample.

![](docs/screenshot_profiler_vcontainer.png)

![](docs/screenshot_profiler_zenject.png)

## Breaking Changes to v0.1.x

- From 0.0.x, the API of LifetimeScope has changed.
  - [Obsolete MonoInstaller/ScriptableObjectInstaller and instead inherit LifetimeScope](https://github.com/hadashiA/VContainer/pull/15)
  - If you are using an earlier version, please check [Getting Started](#getting-started) again.

## Index

- [What is DI ?](#what-is-di-)
- [Installation](#installation)
- [Getting Started](#getting-started)
- [Resolving](#resolving)
- [Registering](#registering)
- [Controlling Scope and Lifetime](#controlling-scope-and-lifetime)
- [Dispatching Unity Lifecycle](#dispatching-unity-lifecycle)
- [Optimization](#optimization)
- [Best Practices and Recommendations](#best-practices-and-recommendations)

## What is DI ?

DI (Dependency Injection) is a general technique in OOP that all about removing unconcerned dependencies from your code.
It brings testability, maintainability, extensibility or any kind of exchangeability to your object graph.

In all programming paradigms, the basic design is, weak module coupling and strong module cohesion.
As you know, OOP(Object Oriented Programming) does it through objects.
1. Objects hides the details of its responsibility (encapsulation).
2. Objects transfer work outside their responsibilities to other objects

Actually, There is a problem in doing this fundamentally.
If you write the delegation object in the class code, it means tight coupling at the source code level.
The only way to exclude a unconcerned dependency from a class is to pass from outside.

Then, if your class receives dependencies externally, need help from outside.
DI is a technique that facilitates a place to resolve dependencies completely outside.

Terminology:
- **DI Container:** Have dependent references all at once and execute auto-wiring.
- **Composition Root:** The place where you make settings to resolve dependencies.
- **Auto-wiring:** It is a function that allows you to manage services in the container with minimal configuration. DI library usually does this.
- **IoC (Inversion of Control):** Make the object with control flow responsibility an entry point. In simple and traditional programming, the entry point is where the responsibility for interrupting user input is. What this means for "Inversion".

Note:
- Don't try to DI what an object should hide inside. What's hidden is leaking and the design is getting worse.
- The word DI does not necessarily mean magic or library. Creating your own Composition Root without using the library is called `Poor man's DI` or `DIY DI`.  This is **not** an anti-pattern.

Further reading:
- [Lightweight IoC Container for Unity - Seba's Lab](https://www.sebaslab.com/ioc-container-unity-part-1/)
- [Manning | Dependency Injection in .NET](https://www.manning.com/books/dependency-injection-in-dot-net)


## Installation

- Unity Package Manager (upm)
  - `Window -> Package Manager -> +▼ -> Add package from git url`
    - `https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer`
  - Or add this to your `Package/manifest.json`
    - `"jp.hadashikick.vcontainer": "https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer"`
- unitypackage
  - The [releases](https://github.com/hadashiA/VContainer/releases) page provides downloadable .unitypackage files.

## Getting Started

The basic way for integrating VContainer into your application is:

- Create a component that inherits `LifetimeScope` in your scene. It has one container and one scope.
- Register dependencies with C# code in a subclass of LifetimeScope. This is the composition root.
- When playing scene, LifetimeScope automatically build Container and dispatch to the own PlayerLoopSystem.

Note:
- Normally, "scope" is repeatedly created and destroyed during the game. `LifetimeScope` assumes this and has a parent-child relationship.

**1. Write a class that depends on other classes**

Let's say Hello world.

```csharp
namespace MyGame
{
    public class HelloWorldService
    {
        public void Hello()
        {
            UnityEngine.Debug.Log("Hello world");
        }
    }
}
```

**2. Define composition root**

Next, let's write a setting that can auto-wiring the class.

- Right click in a folder within the Project Tab and Choose **Create -> C# Script**.
- Name it `GameLifetimeScope.cs`.

Note:
- VContainer will automatically template C# scripts ending in `*LifetimeScope`.

You instruct `builder` and register the class above.

```diff
using VContainer;
using VContainer.Unity;

namespace MyGame
{
    public sealed class GameLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
+            builder.Register<HelloWorldService>(Lifetime.Singleton);
        }
    }
}
```

Note:
- VContainer always required a `Lifetime` argument explicitly. This gives us transparency and consistency.

**3. Create GameObject attached your LifetimeScope**

Right Click inside the Hierarchy tab and select **Create Empty**. And name `GameLifetimeScope` it.

Then attach the Component created above.

![](docs/screenshot_gamelifetimescope.png)

**4. How to use your new HelloWorldService  ?**

Registered objects will automatically have dependency injection.
Like below:

```csharp
using VContainer;
using VContainer.Unity;

namespace MyGame
{
    public class GamePresenter
    {
        readonly HelloWorldService helloWorldService;

        public GamePresenter(HelloWorldService helloWorldService)
        {
            this.helloWorldService = helloWorldService;
        }
    }
}
```

And let's also register this class.

```diff
builder.Register<HelloWorldService>(Lifetime.Singleton);
+ builder.Register<GamePresenter>(Lifetime.Singleton);
```

**5. Execute your registerd object on PlayerLoopSystem**

To write an application in Unity, we have to interrupt Unity's lifecycle events.
(Typically MonoBehaviour's Start / Update / OnDestroy / etc..)

Objects registered with VContainer can do this independently of MonoBehaviour.
This is done automatically by implementing and registering some marker interfaces.

```diff
using VContainer;
using VContainer.Unity;

 namespace MyGame
 {
-    public class GamePresenter
+    public class GamePresenter : ITickable
     {
         readonly HelloWorldService helloWorldService;

         public GamePresenter(HelloWorldService helloWorldService)
         {
             this.helloWorldService = helloWorldService;
         }

+        void ITickable.Tick()
+        {
+            helloWorldService.Hello();
+        }
     }
 }
```

Now, `Tick()` will be executed at the timing of Unity's Update.

As such, it's a good practice to keep any side effect entry points through the marker interface.  

( By design, for MonoBehaviour is enough to use Start / Update etc. The marker interface of VContainer is a function to separate the entry point of domain logic and presentation logic. )

We should register this as running on Unity's life cycle events.

```diff
- builder.Register<GamePresenter>(Lifetime.Singleton);
+ builder.RegisterEntryPoint<GamePresenter>(Lifetime.Singleton)
```

Note:
- `RegisterEntryPoint<GamePresenter>(Lifetime.Singleton)` is an alias to register interfaces related to Unity's PlayerLoop event. ( Simular to `Register<GamePresenter>(Lifetime.Singleton).As<ITickable>()`)
- Registering lifecycle events without relying on MonoBehaviour facilitates decupling of domain logic and presentation !

If you have multiple EntryPoints, you can also use the following declaration as grouping.

```csharp
builder.UseEntryPoints(Lifetime.Singleton, entryPoints =>
{
    entryPoints.Add<GamePresenter>();
    // entryPoints.Add<OtherSingletonEntryPointA>();
    // entryPoints.Add<OtherSingletonEntryPointB>();
    // entryPoints.Add<OtherSingletonEntryPointC>();
})
```

This makes it clearer that EntryPoints are given special treatment by design.

## Resolving

### Constructor Injection

VContainer automatically collects and calls registered class constructors.

Note:
- At this time, all the parameters of the constructor must be registered.
- If the dependency cannot be resolved, throws exception when validating LifetimeScope or building Container.

Here is basic idiom with DI.

```csharp
class ClassA
{
    readonly IServiceA serviceA;
    readonly IServiceB serviceB;
    readonly SomeUnityComponent component;

    public ClassA(
        IServiceA serviceA,
        IServiceB serviceB,
        SomeUnityComponent component)
    {
        this.serviceA = serviceA;
        this.serviceB = serviceB;
        this.component = component;
    }
}
```

:warning: Constructors are often stripped in the IL2CPP environment.
To prevent this problem, add the `[Inject]` Attribute explicitly.

```csharp
    [Inject]
    public ClassA(
        IServiceA serviceA,
        IServiceB serviceB,
        SomeUnityComponent component)
    {
        // ...
    }
```

Note:
- If class has multiple constructors, the one with `[Inject]` has priority.

**Recommendation:**
Use Constructor Injection whenever possible.
The constructor & readonly field idiom is:
- The instantiated object has a compiler-level guarantee that the dependencies have been resolved.
- No magic in the class code. Instantiate easily without DI container. (e.g. Unit testing)
- If you look at the constructor, the dependency is clear.
  - If too many constructor arguments, it can be considered overly responsible.

### Method Injection

If constructor injection is not available, use method injection.

Typically this is for MonoBehaviour.

```csharp
public class SomeBehaviour : MonoBehaviour
{
    float speed;

    [Inject]
    public void Construct(GameSettings settings)
    {
        speed = settings.speed;
    }
}
```

**Recommendation:**
Consider whether injection to MonoBehaviour is necessary.
In a code base where domain logic and presentation are well decoupled, MonoBehaviour should act as a View component.

In my opinion, View components should only be responsible for rendering and should be flexible.

Of course, In order for the View component to work, it needs to pass state at runtime.
But the "state" of an object and its dependency of functionality of other objects are different.

It's enough to pass the state as arguments instead of `[Inject]`.

### Property / Field Injection

If the object has a local default and Inject is optional,
Property/Field Injection can be used.

```csharp
class ClassA
{
    [Inject]
    IServiceA serviceA { get; set; } // Will be overwritten if something is registered.

    public ClassA()
    {
        serviceA = ServiceA.GoodLocalDefault;
    }
}
```

You can also use field.

```csharp
    [Inject]
    IServiceA serviceA;
```

### Implicit Relationship Types

VContainer supports automatically resolving particular types implicitly to support special relationships.

#### `IEnumerable<T>` / `IReadonlyLIst<T>`

Duplicate registered interfaces can be resolved together with IEnumerable<T> or IReadOnlyList<T>.

```csharp
builder.Register<IDisposable, A>(Lifetime.Scoped);
builder.Register<IDisposable, B>(Lifetime.Scoped);
```

```csharp
class ClassA
{
    public ClassA(IEnumerable<IDisposable> disposables) { /* ... */ }
}
```

OR

```csharp
class ClassA
{
    public ClassA(IReadOnlyList<IDisposable> disposables) { /* ... */ }
}
```

Note:
- This is mainly used by internal functions such as `ITickable` marker etc.

## Registering

### Scoping overview

- Singleton : Single instance per container (includes all parents and children).
- Transient : Instance per resolving.
- Scoped    : Instance per `LifetimeScope`.
  - If LifetimeScope is single, similar to Singleton.
  - If you create a LifetimeScope child, the instance will be different for each child.
  - When LifetimeScope is destroyed, release references and calls all the registered `IDisposable`.

See more information: [Controlling Object Lifetime](#controlling-object-lifetime)

### Register method family

There are various ways to use Register.
Let's take the following complex type as an example.

```csharp
class ServiceA : IServiceA, IInputPort, IDisposable { /* ... */ }
```

#### Register Concrete Type

```csharp
builder.Register<ServiceA>(Lifetime.Singleton);
```

It can resolve like this:

```csharp
class ClassA
{
    public ClassA(IServiceA serviceA) { /* ... */ }
}
```

####  Register as Interface

```csharp
builder.Register<IServiceA, ServiceA>();
```

It can resolve like this:

```csharp
class ClassA
{
    public ClassA(IServiceA serviceA) { /* ... */ }
}
```

#### Register as multiple Interface

```csharp
builder.Register<ServiceA>(Lifetime.Singleton)
    .As<IServiceA, IInputPort>();
```

It can resolve like this:

```csharp
class ClassA
{
    public ClassA(IServiceA serviceA) { /* ... */ }
}

class ClassB
{
    public ClassB(IInputPort handlerA) { /* ... */ }
}
```

####  Register all implemented interfaces automatically

```csharp
builder.Register<ServiceA>(Lifetime.Singleton)
    .AsImplementedInterfaces();
```

It can resolve like this:

```csharp
class ClassA
{
    public ClassA(IServiceA serviceA) { /* ... */ }
}

class ClassB
{
    public ClassB(IHandlerB handlerA) { /* ... */ }
}
```

#### Register all implemented interfaces and concrete type

```csharp
builder.Register<ServiceA>(Lifetime.Singleton)
    .AsImplementedInterfaces()
    .AsSelf();
```

It can resolve like this:

```csharp
class ClassA
{
    public ClassA(IServiceA serviceA) { /* ... */ }
}

class ClassB
{
    public ClassB(IHandlerB handlerA) { /* ... */ }
}

class ClassB
{
    public ClassB(ServiceA serviceA) { /* ... */ }
}
```

#### Register lifecycle marker interfaces

```csharp
class GameController : IInitializable, ITickable, IDisposable { /* ... */ }
```

```csharp
builder.RegisterEntryPoint<GameController>(Lifetime.Singleton);
```

Note:
- this is similar to `Register<GameController>(Lifetime.Singleton).AsImplementedInterfaces()`

If you have multiple EntryPoints, you have the option to use the following declaration as grouping.

```csharp
builder.UseEntryPoints(Lifetime.Scoped, entryPoints =>
{
   entryPoints.Add<ScopedEntryPointA>();
   entryPoints.Add<ScopedEntryPointB>();
   entryPoints.Add<ScopedEntryPointC>().AsSelf();
});
```

This is the same as:

```csharp
builder.RegisterEntryPoint<ScopedEntryPointA>(Lifetime.Scoped);
builder.RegisterEntryPoint<ScopedEntryPointB>(Lifetime.Scoped);
builder.RegisterEntryPoint<ScopedEntryPointC>(Lifetime.Scoped).AsSelf();
```

#### Register instance

```csharp
// ...
var obj = new ServiceA();
// ...

builder.RegisterInstance(obj);
```

Note:
- `RegisterIntance` always `Scoped` lifetime. So it has no arguments.

It can resolve like this:

```csharp
class ClassA
{
    public ClassA(ServiceA serviceA) { /* ... */ }
}
```

####  Register instance as interface

Allow `As*` decrelations.

```csharp
builder.RegisterInstance<IInputPort>(serviceA);

builder.RegisterInstance(serviceA)
    .As<IServiceA, IInputPort>();

builder.RegisterInstance()
    .AsImplementedInterfaces();
```

#### Register type-specific parameters

If the types are not unique, but you have a dependency you want to inject at startup, you can use below:

```csharp
builder.Register<SomeService>(lifetime.Singleton)
    .WithParameter<string>("http://example.com");
```

Or, You can parameter name as a key.

```csharp
builder.Register<SomeService>(Lifetime.Singleton)
    .WithParameter("url", "http://example.com");
```


It can resolve like this:


```csharp
class SomeService
{
    public SomeService(string url) { /* ... */ }
}
```

This Register is with only `SomeService`.

```csharp
class OtherClass
{
    // ! Error
    public OtherClass(string hogehoge) { /* ... */ }
}
```

#### Register `MonoBehaviour`

**Register from LifetimeScope's `[SerializeField]`**

```csharp
[SerializeField]
YourBehaviour yourBehaviour;

// ...

builder.RegisterComponent(yourBehaviour);
```

Note:
- `RegisterComponent` simular to `RegisterInstance`. The only difference is that MonoBehaviour registered with `RegisterComponent` will be injected even if not Resolved.

**Register from scene with `LifetimeScope`**

```csharp
builder.RegisterComponentInHierarchy<YourBehaviour>();
```

Note:
- `RegisterComponentInHierarchy` always `.Scoped` lifetime. Because lifetime is equal to the scene.

**Register component that Instantiate from prefab when resolving**

```csharp
[SerializeField]
YourBehaviour prefab;

// ...

builder.RegisterComponentInNewPrefab(prefab, Lifetime.Scoped);
```

**Register component that with new GameObject when resolving**

```csharp
builder.RegisterComponentOnNewGameObject<YourBehaciour>(Lifetime.Scoped, "NewGameObjectName");
```

**Register component as interface**

```csharp
builder.RegisterComponentInHierarchy<YourBehaviour>()
    .AsImplementedInterfaces();
```

**Register component to specific parent Transform**

```csharp
builder.RegisterComponentFromInNewPrefab<YourBehaviour>(Lifetime.Scoped)
    .UnderTransform(parent);

```

Or find at runtime.

```csharp
builder.RegisterComponentFromInNewPrefab<YourBehaviour>(Lifetime.Scoped)
    .UnderTransform(() => {
        // ...
        return parent;
    });

```

### Grouping MonoBehaviour's Registration

```csharp
builder.UseComponents(components =>
{
    components.AddInstance(yourBehaviour);
    components.AddInHierarchy<YourBehaviour>();
    components.AddFromNewPrefab(prefab, Lifetime.Scoped);
    components.AddOnNewGameObject<YourBehaviour>(Lifetime.Scoped, "name");
})
```

This is the same as:

```csharp
builder.RegisterComponent(yourBehaviour);
builder.RegisterComponentInHierarchy<YourBehaviour>();
builder.RegisterComponentFromNewPrefab(prefab, Lifetime.Scoped);
builder.RegisterComponentOnNewGameObject<YourBehaviour>(Lifetime.Scoped, "name");
```

### ScriptableObject Integration

It is useful to register the setting information saved as ScriptableObject Asset as follows.

```csharp
[Serializable]
public class CameraSettings
{
    public float MoveSpeed = 10f;
    public float DefaultDistance = 5f;
    public float ZoomMax = 20f;
    public float ZoomMin = 5f;
}

[Serializable]
public class ActorSettings
{
    public float MoveSpeed = 0.5f;
    public float FlyingTime = 2f;
    public Vector3 FlyingInitialVelocity = Vector3.zero;
}

[CreateAssetMenu(fileName = "GameSettings", menuName = "MyGame/Settings")]
public class GameSettings : ScriptableObject
{
    [SerializeField]
    public FieldCameraSettings cameraSetting

    [SerializeField]
    public ActorSettings actorSettings;
}
```

And
- Create `GameSettings` assets from menu.
- Register the created asset with LifetimeScope.

```csharp
public class SomeLifetimeScope : LifetimeScope
{
    [SerializeField]
    GameSettings settings;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance(settings.cameraSettings);
        builder.RegisterInstance(settings.actorSettings);
    }
}
```

## Controlling Scope and Lifetime

`LifetimeScope` can build parent-child relationship.
it has following behaviours:

- If registered object is not found, `LifetimeScope` will look for a parent `LifetimeScope`.
- For `Lifetime.Singleton`
  - Always returns the same instance.
  - Parent and child cannot register the same type.
- For `LifeTime.Transient`
  - Instance creating for each resolving.
  - If parent and child have the same type, child will prioritize itself.
- For `Lifetime.Scoped`:
  - Instance will be different for each child.
      - If same child, returns same instance.
      - If parent and child have the same type, child will prioritize itself.
  - When a `LifetimeScope` is destroyed, objects with `IDisposable` implemented are called `Dispose()`.

:warning: If scene is alive and only `LifetimeScope` is destroyed, MonoBehaviour registered as `Lifetime.Scoped` is not automatically destroyed.
If you want to destroy with `LifetimeScope`, make it a child transform of `LifetimeScope` or consider implement IDisposable.

### How to make an Additive scene a child

#### How to set the parent when loading a scene

You can parent it by specifying a `LifetimeScope` object before loading the scene.

```csharp
// Find LifetimeScope by type in all loaded scenes
var parent = LifetimeScope.Find<BaseLifetimeScope>();

// The LifetimeScope generated inside this block will have the specified parent
using (LifetimeScope.PushParent(parent))
{
    // If this scene has a LifetimeScope, its parent will be `current`.
    var loading = SceneManager.LoadSceneAsync("...", LoadSceneMode.Additive);
    while (!loading.isDone)
    {
        yield return null;
    }
}

using (LifetimeScope.PushParent(parent))
{
    // UniTask example
    await SceneManager.LoadSceneAsync("...", LoadSceneMode.Additive);
}
```

#### How to pre-set the parent of a scene to scene

`LifetimeScope` can be serialized by specifying the type of parent in other scenes.

In base scene.

![](./docs/screenshot_baselifetimescope.png)

In additional scene.

![](./docs/screenshot_childlifetimescope.gif)

### How to generate child with code first

Child can also be generated from code.
Can be used to get a reference to the `LifetimeScope` if the key is set.

```csharp
var lifetimeScope = LifetimeScope.Find<BaseLifetimeScope>();
```

And below is an example of creating a child.

```csharp
// Create no extra registered child
var childLifetimeScope = lifetimeScope.CreateChild();

// Create a extra registered child
var childLifetimeScope = lifetimeScope.CreateChild(builder =>
{
    builder.Register<ExtraClass>(Lifetime.Scoped);
    builder.RegisterInstance(extraInstance);
    // ...
});

// Create a extra registered child with IInstaller.
class ChildInstaller : IInstaller
{
    public void Install(IContainerBuilder builder)
    {
        // ...
    }
}

var childLifetimeScope = lifetimeScope.CreateChild(childInstaller);

// Create from LifetimeScope prefab
var childLifetimeScope = lifetimeScope.CreateChildFromPrefab(prefab);

// Allow extra registrations
var childLifetimeScope = lifetimeScope.CreateChildFromPrefab(prefab, builder =>
{
    // Extra Registration
});


// Dispose child scope
UnityEngine.Object.Destroy(childLifetimeScope.gameObject);
```

## How to add additional registers to the next scene

Often, you may want to add additional Registers to the loaded scenes.

For example, when context is finalized after assets are loaded asynchronously.

In that case you could use:

```csharp
using (LifetimeScope.Push(builder =>
{
    // Register for the next scene not yet loaded
    builder.RegisterInstance(extraInstance);
    builder.Register<ExtraType>(Lifetime.Scoped);
}))
{
    // Load the scene here.
    var loading = SceneManager.LoadSceneAsync("NextScene");
    while (!loading.isDone)
    {
        yield return null;
    }
}
```

```csharp
// Example with UniTask
using (LifetimeScope.Push(builder =>
{
    // Register for the next scene not yet loaded
    builder.RegisterInstance(extraInstance);
    builder.Register<ExtraType>(Lifetime.Scoped);
}))
{
    // Load the scene here
    await SceneManager.LoadSceneAsync("NextScene");
}

```

```csharp
// Use registration as type
class FooInstaller : IInstaller
{
    public void Install(IContainerBuilder builder)
    {
        builder.Register<ExtraType>(Lifetime.Scoped);
    }
}

using (LifetimeScope.Push(fooInstaller)
{
    // ...
}
```

### Project Root

You can create a parent for all LifetimeScopes in your scene.

To do so, create a `LifetimeScope` subclass prefab in a folder named `Resources` in Assets and name it "ProjectLifetimeScope".

## Dispatching Unity Lifecycle

VContainer has own PlayerLoop sub systems.

If you register a class that implements the marker interface, it will be scheduled in Unity's PlayerLoop cycle.

Since it uses PlayerLoopSystem, it works even if you register at any time (e.g: `IInitilizable` etc)


The following interfaces and timings are available.

- `IInitializable`     : Nearly `Start`
- `IPostInitializable` : After `Start`
- `IFixedTickable`     : Nearly `FixedUpdate`
- `IPostFixedTickable` : After `FixedUpdate`
- `ITickable`          : Nearly `Update`
- `IPostTickable`      : After `Update`
- `ILateTickable`      : Nearly `LateUpdate`
- `IPostLateTickabl`   : After `LateUpdate`

And

- `IDisposable` : With container disposes. (For `Lifetime.Scoped`)

![](./docs/lifecycle_diagram.png)

Note:
- [Unity - Manual: Order of Execution for Event Functions](https://docs.unity3d.com/Manual/ExecutionOrder.html)

## Comparing VContainer to Zenject

Zenject is awesome. but VContainer is:
- Most parts of reflections and assertions are isolated to the Container's build stage.
- Easy to read implementation.
- Code first, transparent API.

 | Zenject                               | VContainer                                |
 |:--------------------------------------|:------------------------------------------|
 | Container.Bind\<Service\>()<br>&nbsp;&nbsp;&nbsp;&nbsp;.AsTransient() | builder.Register\<Service\>(Lifetime.Transient) |
 | Container.Bind\<Service\>()<br>&nbsp;&nbsp;&nbsp;&nbsp;.AsCached() | builder.Register\<Service\>(Lifetime.Scoped) |
 | Container.Bind\<Service\>()<br>&nbsp;&nbsp;&nbsp;&nbsp;.AsSingle() | builder.Register\<Service\>(Lifetime.Singleton) |
 | Container.Bind\<IService\>()<br>&nbsp;&nbsp;&nbsp;&nbsp;.To\<Service\>()<br>&nbsp;&nbsp;&nbsp;&nbsp;.AsCache() | builder.Register\<IService, Service\>(Lifetime.Scoped) |
 | Container.Bind(typeof(IInitializable), typeof(IDisposable))<br>&nbsp;&nbsp;&nbsp;&nbsp;.To\<Service\>()<br>&nbsp;&nbsp;&nbsp;&nbsp;.AsCached(); | builder.Register\<Service\>(Lifetime.Scoped)<br>&nbsp;&nbsp;&nbsp;&nbsp;.As\<IInitializable, IDisposable\>() |
 | Container.BindInterfacesTo\<Service\>()<br>&nbsp;&nbsp;&nbsp;&nbsp;.AsCached() | builder.Register\<Service\>(Lifetime.Scoped)<br>&nbsp;&nbsp;&nbsp;&nbsp;.AsImplementedInterfaces() |
 | Container.BindInterfacesAndSelfTo\<Foo\>()<br>&nbsp;&nbsp;&nbsp;&nbsp;.AsCached()| builder.Register\<Service\>(Lifetime.Scoped)<br>&nbsp;&nbsp;&nbsp;&nbsp;.AsImplementedInterfaces()<br>&nbsp;&nbsp;&nbsp;&nbsp;.AsSelf() |
 | Container.BindInstance(obj) | builder.RegisterInstance(obj) |
 | Container.Bind\<IService\>()<br>&nbsp;&nbsp;&nbsp;&nbsp;.FromInstance(obj) | builder.RegisterInstance\<IService\>(obj) |
 | Container.Bind(typeof(IService1), typeof(IService2))<br>&nbsp;&nbsp;&nbsp;&nbsp;.FromInstance(obj) | builder.RegisterInstance(obj)<br>&nbsp;&nbsp;&nbsp;&nbsp;.As\<IService1, IService2\>() |
 | Container.BindInterfacesTo\<Service\>()<br>&nbsp;&nbsp;&nbsp;&nbsp;.FromInstance(obj) | builder.RegisterInstance(obj)<br>&nbsp;&nbsp;&nbsp;&nbsp;.AsImplementedInterfaces() |
 | Container.BindInterfacesAndSelfTo\<Service\>()<br>&nbsp;&nbsp;&nbsp;&nbsp;.FromInstance(obj) | builder.RegisterInstance(obj)<br>&nbsp;&nbsp;&nbsp;&nbsp;.AsImplementedInterfaces()<br>&nbsp;&nbsp;&nbsp;&nbsp;.AsSelf() |
 | Container.Bind\<Foo\>()<br>&nbsp;&nbsp;&nbsp;&nbsp;.FromComponentInHierarchy()<br>&nbsp;&nbsp;&nbsp;&nbsp;.AsCached(); | builder.RegisterComponentInHierarchy\<Foo\>() |
 | Container.Bind\<Foo\>()<br>&nbsp;&nbsp;&nbsp;&nbsp;.FromComponentInNewPrefab(prefab)<br>&nbsp;&nbsp;&nbsp;&nbsp;.AsCached()<br>&nbsp;&nbsp;&nbsp;&nbsp; | builder.RegisterComponentInNewPrefab(prefab, Lifetime.Scoped)
 | Container.Bind\<Foo\>()<br>&nbsp;&nbsp;&nbsp;&nbsp;.FromNewComponentOnNewGameObject()<br>&nbsp;&nbsp;&nbsp;&nbsp;.AsCached()<br>&nbsp;&nbsp;&nbsp;&nbsp;.WithGameObjectName("Foo1") | builder.RegisterComponentOnNewGameObject\<Foo\>(Lifetime.Scoped, "Foo1") |
 | .UnderTransform(parentTransform) | .UnderTransform(parentTransform) |
 | .UnderTransform(() => parentTransform) | .UnderTransform(() => parentTransform) |
 | Container.Bind\<Foo\>()<br>&nbsp;&nbsp;&nbsp;&nbsp;.FromComponentInNewPrefabResource("Some/Path/Foo") | **Not supported**<br>We should load Resources using  `LoadAsync` family.<br>You can use `RegisterInstance()` etc after loading the Resource. |
 | Container.Bind\<Foo\>()<br>&nbsp;&nbsp;&nbsp;&nbsp;.WithId("foo").AsCached() | **Not supported**<br>Duplicate type Resolve is not recommended.<br>You can instead use type-specific Register<br>builder.Register\<Service\>(Lifetime.Scoped)<br>&nbsp;&nbsp;&nbsp;&nbsp;.WithParameter("foo", foo) |

wip

## Optimization

### Async Contaienr Build

VContainer executes pre-processing such as reflection when building a container.
To reduce main thread blocking time for your game:

1. Set `false` to `LifetimeScope.autoRun`
2. After scene loading, Call `LifetimeScope.Build` manually.

For example:

```csharp
var nextScene = await LoadSceneAsync(...);

var lifetimeScope = LifetimeScope.Find<MyLifetimeScope>(nextScene.Scene);

// Your async solution here.
await UniTask.Run(() => lifetimeScope.Build());
```

:warning: Unity dependent features such as `builder.RegisterComponentInHierarcky()` do not work on background threads.

If this causes an error, use Awake instead.

For example:

```csharp
class GameLifetimeScope : LifetimeScope
{
    Ground groundInScene;

    protected override void Awake()
    {
        // Run main thread.
        groundInScene = FindObjectOfType<Ground>();

        base.Awake();
    }

    protected override void Configure(IContainerBuilder builder)
    {
        // It can run background thread.
        builder.RegisterInstance(groundInScene);
    }
}
```


### Parallel Container Build

If you enable `VCONTAINER_PARALLEL_CONTAINER_BUILD` compilation flag,
VCOntainer will perform container builds in Parallel.

This is the solution when you have a lot of objects to register.

:warning: If there are few objects to register, this will be slower.

## Best Practices and Recommendations

- Use Constructor Injection whenever possible. see [Constructor Injection](#constructor-injection) section.
- Consider whether injection to MonoBehaviour is necessary. see [Method Injection](#method-injection) section.
- wip

## License

 MIT
