using System;
using Reflex.Scripts.Utilities;

namespace Reflex
{
	internal static class PlatformReflector
	{
		internal static Reflector Current = null;

		static PlatformReflector()
		{
			switch (RuntimeScriptingBackend.Current)
			{
				case RuntimeScriptingBackend.Backend.Undefined: throw new Exception($"Scripting backend could not be defined.");
				case RuntimeScriptingBackend.Backend.Mono: Current = new MonoReflector(); break;
				case RuntimeScriptingBackend.Backend.IL2CPP: Current = new IL2CPPReflector(); break;
				default: throw new ArgumentOutOfRangeException($"{RuntimeScriptingBackend.Current} scripting backend not handled.");
			}
		}
	}
}