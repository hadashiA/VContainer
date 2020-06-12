**work in progress**

# VContainer

VContainer is an implementation of lightweight DI (Dependency Injection) Container for Unity (Game Engine).

"V" means making Unity's initial "U" more thinner and solid ... !

- Fast resolve. Minimum GC allocation. (For using reflection. Because it targets Unity IL2CPP.)
- Extra small code size. Few internal types. 
- Application can freely create nested Lifetime Scope
- Immutable Container.
  - Resolve is thread safe
  - Container build phasae can be running on background thread.

## Installation

## Getting Started

```csharp
// Create a container registry
var builder = new ContainerBuilder();

// Register interface -> type mapping
// vcontainer requires lifetime argument explicitly. 
builder.Register<IUserRepository, SqlUserRepository>(Lifetime.Transient);
builder.Register<ILogger, Logger>(Lifetime.Singleton);
builder.Register<ILogger, Logger>(Lifetime.Scoped);

// Build container. Type information collection by reflection is performed here.
var container = builder.Build();

// Get instance from container
var userRepository = resolver.Resolve<IUserRepository>();
var logger = resolver.Resolve<ILogger>();

// Scoped objects will be disposing with Container
container.Dispose(); 
```

Containers can have nested scopes.

```csharp
// TODO:
```

```csharp
public class MyType : IMyType
{
    // field injection

    [Inject]
    public IInjectTarget PublicField;

    [Inject]
    IInjectTarget PrivateField;

    // property injection

    [Inject]
    public IInjectTarget PublicProperty { get; set; }

    [Inject]
    IInjectTarget PrivateProperty { get; set; }

    // constructor injection
    // if not marked [Inject], the constructor with the most parameters is used.
    [Inject]
    public MyType(IInjectTarget x, IInjectTarget y, IInjectTarget z)
    {

    }

    // method injection

    [Inject]
    public void Initialize1()
    {
    }

    [Inject]
    public void Initialize2()
    {
    }
}

// and resolve it
var v = resolver.Resolve<IMyType>();
```

## Using with Unity

wip

### Scene lifecycle

wip

### Instantiate GameObject

wip

## Thread safety

wip

You can building container can running on background thread.

## Controlling Scope and Lifetime

wip

## Comarison with Zenject ?

wip

Zenject is awesome. but...

- Most parts of reflections and assertions are isolated to the Container's build stage.
- Easy to read implementation
- Code first, transparent API !
