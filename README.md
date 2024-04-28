# VContainer

![](https://github.com/hadashiA/VContainer/workflows/Test/badge.svg)
![](https://img.shields.io/badge/unity-2018.4+-000.svg)
[![Releases](https://img.shields.io/github/release/hadashiA/VContainer.svg)](https://github.com/hadashiA/VContainer/releases)
[![openupm](https://img.shields.io/npm/v/jp.hadashikick.vcontainer?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/jp.hadashikick.vcontainer/)

The extra fast DI (Dependency Injection) library running on Unity Game Engine.

"V" means making Unity's initial "U" more thinner and solid ... !

- **Fast Resolve:** Basically 5-10x faster than Zenject.
- **Minimum GC Allocation:** In Resolve, we have **zero allocation** without spawned instances.
- **Small code size:** Few internal types and few .callvirt.
- **Assisting correct DI way:** Provides simple and transparent API, and carefully select features. This prevents the DI declaration from becoming overly complex.
- **Immutable Container:** Thread safety and robustness.

## Features

- Constructor Injection / Method Injection / Property & Field Injection
- Dispatch own PlayerLoopSystem
- Flexible scoping
  - Application can freely create nested Lifetime Scope with any async way for you like.
- Accelerated mode with SourceGenerator (Optional)
- Diagnositcs window on unity editor
- UniTask Integration
- ECS Integration *beta*

## Documentation

Visit [vcontainer.hadashikick.jp](https://vcontainer.hadashikick.jp) to view the full documentation.

## Performance

![](./website/static/img/benchmark_result.png)

### GC Alloc Result Example

![](./website/static/img/gc_alloc_profiler_result.png)

![](./website/static/img/screenshot_profiler_vcontainer.png)

![](./website/static/img/screenshot_profiler_zenject.png)


## Installation

*Requires Unity 2018.4+*

### Install via UPM (using Git URL)

1. Navigate to your project's Packages folder and open the manifest.json file.
2. Add this line below the "dependencies": { line
    - ```json title="Packages/manifest.json"
      "jp.hadashikick.vcontainer": "https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer#1.15.4",
      ```
3. UPM should now install the package.

### Install via OpenUPM


1. The package is available on the [openupm registry](https://openupm.com). It's recommended to install it via [openupm-cli](https://github.com/openupm/openupm-cli).
2. Execute the openum command.
    - ```
      openupm add jp.hadashikick.vcontainer
      ```

### Install manually (using .unitypackage)

1. Download the .unitypackage from [releases](https://github.com/hadashiA/VContainer/releases) page.
2. Open VContainer.x.x.x.unitypackage

## Basic Usage

First, create a scope. References are automatically resolved for types registered here.

```csharp
public class GameLifetimeScope : LifetimeScope
{
    public override void Configure(IContainerBuilder builder)
    {
        builder.RegisterEntryPoint<ActorPresenter>();

        builder.Register<CharacterService>(Lifetime.Scoped);
        builder.Register<IRouteSearch, AStarRouteSearch>(Lifetime.Singleton);

        builder.RegisterComponentInHierarchy<ActorsView>();
    }
}
```

Where definitions of classes are

```csharp
public interface IRouteSearch
{
}

public class AStarRouteSearch : IRouteSearch
{
}

public class CharacterService
{
    readonly IRouteSearch routeSearch;

    public CharacterService(IRouteSearch routeSearch)
    {
        this.routeSearch = routeSearch;
    }
}
```

```csharp
public class ActorsView : MonoBehaviour
{
}
```

and

```csharp
public class ActorPresenter : IStartable
{
    readonly CharacterService service;
    readonly ActorsView actorsView;

    public ActorPresenter(
        CharacterService service,
        ActorsView actorsView)
    {
        this.service = service;
        this.actorsView = actorsView;
    }

    void IStartable.Start()
    {
        // Scheduled at Start () on VContainer's own PlayerLoopSystem.
    }
}
```


- In this example, the routeSearch of CharacterService is automatically set as the instance of AStarRouteSearch when CharacterService is resolved.
- Further, VContainer can have a Pure C# class as an entry point. (Various timings such as Start, Update, etc. can be specified.) This facilitates "separation of domain logic and presentation".

### Flexible Scoping with async

LifetimeScope can dynamically create children. This allows you to deal with the asynchronous resource loading that often occurs in games.

```csharp
public void LoadLevel()
{
    // ... Loading some assets

    // Create a child scope
    instantScope = currentScope.CreateChild();

    // Create a child scope with LifetimeScope prefab
    instantScope = currentScope.CreateChildFromPrefab(lifetimeScopePrefab);

    // Create a child with additional registration
    instantScope = currentScope.CreateChildFromPrefab(
        lifetimeScopePrefab,
        builder =>
        {
            // Extra Registrations ...
        });

    instantScope = currentScope.CreateChild(builder =>
    {
        // ExtraRegistrations ...
    });

    instantScope = currentScope.CreateChild(extraInstaller);
}

public void UnloadLevel()
{
    instantScope.Dispose();
}
```

In addition, you can create a parent-child relationship with LifetimeScope in an Additive scene.

```csharp
class SceneLoader
{
    readonly LifetimeScope currentScope;

    public SceneLoader(LifetimeScope currentScope)
    {
        this.currentScope = currentScope; // Inject the LifetimeScope to which this class belongs
    }

    IEnumerator LoadSceneAsync()
    {
        // LifetimeScope generated in this block will be parented by `this.lifetimeScope`
        using (LifetimeScope.EnqueueParent(currentScope))
        {
            // If this scene has a LifetimeScope, its parent will be `parent`.
            var loading = SceneManager.LoadSceneAsync("...", LoadSceneMode.Additive);
            while (!loading.isDone)
            {
                yield return null;
            }
        }
    }

    // UniTask example
    async UniTask LoadSceneAsync()
    {
        using (LifetimeScope.EnqueueParent(parent))
        {
            await SceneManager.LoadSceneAsync("...", LoadSceneMode.Additive);
        }
    }
}
```

```csharp
// LifetimeScopes generated during this block will be additionally Registered.
using (LifetimeScope.Enqueue(builder =>
{
    // Register for the next scene not yet loaded
    builder.RegisterInstance(extraInstance);
}))
{
    // Loading the scene..
}
```

See [scoping](https://vcontainer.hadashikick.jp/scoping/lifetime-overview) for more information.

## UniTask

```csharp
public class FooController : IAsyncStartable
{
    public async UniTask StartAsync(CancellationToken cancellation)
    {
        await LoadSomethingAsync(cancellation);
        await ...
        ...
    }
}
```

```csharp
builder.RegisterEntryPoint<FooController>();
```

See [integrations](https://vcontainer.hadashikick.jp/integrations/unitask) for more information.


## Diagnostics Window

![](./website/static/img/screenshot_diagnostics_window.png)

See [diagnostics](https://vcontainer.hadashikick.jp/diagnostics/diagnostics-window) for more information.

## Credits

VContainer is inspired by:

- [Zenject](https://github.com/modesttree/Zenject) / [Extenject](https://github.com/svermeulen/Extenject).
- [Autofac](http://autofac.org) - [Autofac Project](https://github.com/autofac/Autofac).
- [MicroResolver](https://github.com/neuecc/MicroResolver)

## Author

[@hadashiA](https://twitter.com/hadashiA)

## License

MIT
