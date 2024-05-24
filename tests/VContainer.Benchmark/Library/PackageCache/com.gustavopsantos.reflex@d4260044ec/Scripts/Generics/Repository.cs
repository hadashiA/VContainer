using System;
using System.Collections.Generic;

namespace Reflex
{
	internal class Repository<TKey, TValue>
	{
		private readonly Func<TKey, TValue> _cache;
		private readonly Dictionary<TKey, TValue> _registry = new Dictionary<TKey, TValue>();

		public Repository(Func<TKey, TValue> cache)
		{
			_cache = cache;
		}
		
		internal TValue Fetch(TKey key)
		{
			if (!_registry.ContainsKey(key))
			{
				_registry.Add(key, _cache(key));
			}

			return _registry[key];
		}
	}
}