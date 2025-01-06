using UnityEditor;
using VContainer.Unity.Extensions;

namespace VContainer.Editor
{
    [CustomEditor(typeof(NestedSingletonLifetimeScope<>), true)]
    public sealed class NestedSingletonLifetimeScopeEditor : ReadOnlyParentReferenceEditor{}
}