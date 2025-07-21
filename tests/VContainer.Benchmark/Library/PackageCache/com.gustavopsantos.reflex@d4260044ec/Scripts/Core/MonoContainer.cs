using System;
using UnityEngine;

namespace Reflex.Scripts
{
    public class MonoContainer : MonoBehaviour
    {
        internal readonly Container Container = new Container();

        public void Clear()
        {
            Container.Clear();
        }

        public BindingContractDefinition<TContract> Bind<TContract>()
        {
            return Container.Bind<TContract>();
        }

        public void BindSingleton<TContract>(TContract instance)
        {
            Container.BindSingleton<TContract>(instance);
        }

        public void BindSingleton<T>(Type contract, T instance)
        {
            Container.BindSingleton(contract, instance);
        }

        public BindingGenericContractDefinition BindGenericContract(Type genericContract)
        {
            return Container.BindGenericContract(genericContract);
        }

        public TContract Resolve<TContract>()
        {
            return Container.Resolve<TContract>();
        }

        public object Resolve(Type contract)
        {
            return Container.Resolve(contract);
        }

        public TCast ResolveGenericContract<TCast>(Type genericContract, params Type[] genericConcrete)
        {
            return Container.ResolveGenericContract<TCast>(genericContract, genericConcrete);
        }
    }
}