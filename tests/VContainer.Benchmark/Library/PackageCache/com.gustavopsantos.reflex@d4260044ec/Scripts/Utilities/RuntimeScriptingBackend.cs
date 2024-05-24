namespace Reflex.Scripts.Utilities
{
	internal static class RuntimeScriptingBackend
	{
		internal enum Backend
		{
			Undefined,
			Mono,
			IL2CPP
		}

		internal static Backend Current
		{
			get
			{
#if ENABLE_MONO
				return Backend.Mono;
#elif ENABLE_IL2CPP
				return Backend.IL2CPP;
#else
				return Backend.Undefined;
#endif
			}
		}
	}
}