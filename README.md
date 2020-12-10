# VContainer

![](https://github.com/hadashiA/VContainer/workflows/Test/badge.svg)
![](https://img.shields.io/badge/unity-2018.4+-000.svg)
[![Releases](https://img.shields.io/github/release/hadashiA/VContainer.svg)](https://github.com/hadashiA/VContainer/releases)
[![openupm](https://img.shields.io/npm/v/jp.hadashikick.vcontainer?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/jp.hadashikick.vcontainer/)

**VContainer** is an fatest DI (Dependency Injection) library running on Unity (Game Engine).  

![](website/docs/about/assets/vcontainer@2x.png)

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
- Pre IL Code generation optimization mode
- ECS Integration *beta*

## Getting Started

Visit [vcontainer.hadashikick.jp/getting-started](https://vcontainer.hadashikick.jp/getting-started/installation) to get started with Next.js.

## Documentation

Visit [vcontainer.hadashikick.jp](https://vcontainer.hadashikick.jp) to view the full documentation.

## License

MIT
