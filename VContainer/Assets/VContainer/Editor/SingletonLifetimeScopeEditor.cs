using UnityEditor;
using VContainer.Unity.Extensions;

namespace VContainer.Editor
{
    [CustomEditor(typeof(SingletonLifetimeScope<>), true)]
    public sealed class SingletonLifetimeScopeEditor : ReadOnlyParentReferenceEditor{}
}