using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace VContainer.Tests.Unity
{
    public class SampleMonoBehaviour3 : MonoBehaviour
    {
        public GameObject Prefab;
        [Inject]
        public void Construct(IObjectResolver container)
        {
            var go = Object.Instantiate(Prefab);
            container.InjectGameObject(go);
        }
    }
}
