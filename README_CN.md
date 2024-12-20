# VContainer

![](https://github.com/hadashiA/VContainer/workflows/Test/badge.svg)
![](https://img.shields.io/badge/unity-2018.4+-000.svg)
[![Releases](https://img.shields.io/github/release/hadashiA/VContainer.svg)](https://github.com/hadashiA/VContainer/releases)
[![openupm](https://img.shields.io/npm/v/jp.hadashikick.vcontainer?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/jp.hadashikick.vcontainer/)

运行在 Unity 游戏引擎上的超快 DI（依赖注入）库。

"V" 意味着让 Unity 初始的 "U" 更轻量更稳固 ... !

- **快速解析:** 基本比 Zenject 快5-10倍。
- **最小GC分配:** 解析过程中，在没有生成实例的情况下实现了**零堆内存分配**。
- **小代码量:** 只有很少的内部类型和 .callvirt。
- **支持正确的 DI 方式:** 提供简单透明的 API，并仔细选择功能，防止 DI 声明变得过于复杂。
- **不可变容器:** 线程安全和健壮性。

## 特征

- 构造函数注入 / 方法注入 / 属性和字段注入
- 调度自己的 PlayerLoopSystem
- 灵活的作用域
  - 应用程序可自由创建嵌套的生命周期作用域 (Lifetime Scope)，可根据自己的喜好使用任何异步。
- 使用 SourceGenerator 的加速模式（可选）
- 为 Unity 提供编辑器中的诊断 (Diagnositcs) 窗口
- 集成 UniTask
- 集成 ECS *beta*

## 文档

访问 [vcontainer.hadashikick.jp](https://vcontainer.hadashikick.jp) 阅读完整文档

## 性能

![](./website/static/img/benchmark_result.png)

### GC 分配结果示例

![](./website/static/img/gc_alloc_profiler_result.png)

![](./website/static/img/screenshot_profiler_vcontainer.png)

![](./website/static/img/screenshot_profiler_zenject.png)

## 安装

*要求 Unity 2018.4+*

### 通过 UPM 安装 (使用 Git 链接)

1. 导航到项目的 Packages 文件夹并打开 manifest.json 文件。
2. 在 "dependencies": { 行下添加以下内容：
    - ```json title="Packages/manifest.json"
      "jp.hadashikick.vcontainer": "https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer#1.16.6",
      ```
3. UPM 现在应该会安装此包。

### 通过 OpenUPM 安装


1. [openupm registry](https://openupm.com) 上支持此包。 推荐通过 [openupm-cli](https://github.com/openupm/openupm-cli) 进行安装。
2. 执行以下 openum 命令：.
    - ```
      openupm add jp.hadashikick.vcontainer
      ```

### 手动安装（使用.unitypackage）

1. 从 [releases](https://github.com/hadashiA/VContainer/releases) 页面下载 .unitypackage 。
2. 打开 VContainer.x.x.x.unitypackage

## 基础用法

首先，创建一个作用域。这里注册的类型会被自动解析引用。

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

类的定义如下：

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

以及：

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
        // 在 VContainer 自己的 PlayerLoopSystem 上调度 Start()
    }
}
```


- 在这个例子中，当解析 CharacterService 时，CharacterService 的 routeSearch 会被自动设置为 AStarRouteSearch 的实例。
- 此外，VContainer 可以将纯 C# 类作为入口点（可以指定各种事件函数，如 Start、Update 等）。这有助于实现“逻辑领域和表现层的分离”。

### 使用异步 (async) 的灵活作用域

生命周期作用域可以动态创建子作用域。这允许你处理游戏中经常发生的异步资源加载。

```csharp
public void LoadLevel()
{
    // ... 加载一些资源

    // 创建一个子作用域
    instantScope = currentScope.CreateChild();

    // 使用 LifetimeScope 预制件创建子作用域
    instantScope = currentScope.CreateChildFromPrefab(lifetimeScopePrefab);

    // 额外注册创建子作用域
    instantScope = currentScope.CreateChildFromPrefab(
        lifetimeScopePrefab,
        builder =>
        {
            // 额外注册...
        });

    instantScope = currentScope.CreateChild(builder =>
    {
        // 额外注册...
    });

    instantScope = currentScope.CreateChild(extraInstaller);
}

public void UnloadLevel()
{
    instantScope.Dispose();
}
```

此外，你可以在附加场景中使用 LifetimeScope 创建父子关系。

```csharp
class SceneLoader
{
    readonly LifetimeScope currentScope;

    public SceneLoader(LifetimeScope currentScope)
    {
        this.currentScope = currentScope; // 注入此类所属的 LifetimeScope
    }

    IEnumerator LoadSceneAsync()
    {
        // 在此区块中生成的 LifetimeScope 将设置`this.lifetimeScope`为父级
        using (LifetimeScope.EnqueueParent(currentScope))
        {
            // 如果此场景有一个 LifetimeScope，它的父级将是 `parent`
            var loading = SceneManager.LoadSceneAsync("...", LoadSceneMode.Additive);
            while (!loading.isDone)
            {
                yield return null;
            }
        }
    }

    // UniTask 示例
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
// 在此区块中生成的 LifetimeScope 将被额外注册。
using (LifetimeScope.Enqueue(builder =>
{
    // 为尚未加载的下一个场景注册
    builder.RegisterInstance(extraInstance);
}))
{
    // 加载场景...
}
```

查看 [scoping](https://vcontainer.hadashikick.jp/scoping/lifetime-overview) 获得更多信息。

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

查看 [integrations](https://vcontainer.hadashikick.jp/integrations/unitask) 获取更多信息。

## Diagnositcs 窗口

![](./website/static/img/screenshot_diagnostics_window.png)

查看 [diagnostics](https://vcontainer.hadashikick.jp/diagnostics/diagnostics-window) 获得更多信息。

## Credits

VContainer 灵感来源:

- [Zenject](https://github.com/modesttree/Zenject) / [Extenject](https://github.com/svermeulen/Extenject).
- [Autofac](http://autofac.org) - [Autofac Project](https://github.com/autofac/Autofac).
- [MicroResolver](https://github.com/neuecc/MicroResolver)

## 作者

[@hadashiA](https://twitter.com/hadashiA)

## License

MIT
