# About AI Navigation

The AI Navigation package makes available the following tools for working with navigation in Unity:
* The __Navigation__ window
* The __NavMesh Updater__ window
* The __AI Naigation__ overlay in the scene view
* The visualization in the scene view of the NavMeshes, links, obstacles, agents and their gizmos used for editing
* The authoring part of the components that exist in the `com.unity.modules.ai` core module (i.e. __NavMeshAgent__, __NavMeshObstacle__, __OffMeshLink__)
* Four components that comprise the high level controls for creating data for the navigation system:
  * __NavMeshSurface__ – for building and enabling a NavMesh surface for one agent type.
  * __NavMeshModifier__ – affects the NavMesh generation of NavMesh area types, based on the transform hierarchy.
  * __NavMeshModifierVolume__ – affects the NavMesh generation of NavMesh area types, based on volume.
  * __NavMeshLink__ – connects same or different NavMesh surfaces for one agent type.

# Using AI Navigation
For detailed information on how to use the package, see the [User manual](Documentation~/index.md).\
You can add it to your project by [installing it from the list](https://docs.unity3d.com/Manual/upm-ui-install.html) in the __Package Manager__ window or by [specifying its name](https://docs.unity3d.com/Manual/upm-ui-quick.html) `com.unity.ai.navigation`. Alternatively, you can add an entry with the package name directly in the [project manifest](https://docs.unity3d.com/Manual/upm-manifestPrj.html).\
The package is available as of Unity 2022.2.

# Notice on the Change of License

With effect from 8th December 2020 this package is [licensed](LICENSE.md) under the [Unity Companion License](https://unity3d.com/legal/licenses/unity_companion_license) for Unity-dependent projects. Prior to the 8th December 2020 this package was licensed under MIT and all use of the package content prior to 8th December 2020 is licensed under MIT.
