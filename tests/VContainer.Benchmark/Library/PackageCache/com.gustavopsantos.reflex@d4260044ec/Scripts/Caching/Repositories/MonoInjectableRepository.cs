using System;
using UnityEngine;

namespace Reflex
{
	internal static class MonoInjectableRepository
	{
		internal static readonly Repository<MonoBehaviour, MonoInjectableInfo> Repository = new Repository<MonoBehaviour, MonoInjectableInfo>(BakeMonoInjectableInfo.Bake);
	}
}