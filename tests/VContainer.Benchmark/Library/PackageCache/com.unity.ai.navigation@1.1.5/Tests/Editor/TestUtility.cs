using UnityEditor;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.AI.Navigation.Editor.Tests
{
    public class TestUtility
    {
        [return: NotNull]
        public static GameObject InstantiatePrefab(GameObject prefab, string name)
        {
            GameObject result;

            if (EditorApplication.isPlaying)
                result = Object.Instantiate(prefab);
            else
                result = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

            Assume.That(result, Is.Not.Null);

            result!.name = name;
            return result!;
        }

        public static IEnumerator BakeNavMeshAsync(NavMeshSurface surface, int defaultArea)
        {
            surface.defaultArea = defaultArea;
            NavMeshAssetManager.instance.StartBakingSurfaces(new Object[] { surface });
            yield return new WaitWhile(() => NavMeshAssetManager.instance.IsSurfaceBaking(surface));
        }

        public static void EliminateFromScene(ref GameObject go, bool keepDeactivated = false)
        {
            if (go == null)
                return;

            if (keepDeactivated)
                go.SetActive(false);
            else
                Object.DestroyImmediate(go);
        }
    }

    enum RunMode
    {
        EditMode,
        PlayMode
    }
}
