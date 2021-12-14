using System;
using System.Collections.Generic;

namespace PininSharp.Utils
{
    public class Cache<TK, TV> where TV : class
    {
        public Dictionary<TK, WeakReference<TV?>> Data = new Dictionary<TK, WeakReference<TV?>>();

        public Func<TK, TV> Generator;

        public Cache(Func<TK, TV> generator)
        {
            Generator = generator;
        }

        public TV? Get(TK key)
        {
            if (!Data.ContainsKey(key)) Data[key] = new WeakReference<TV?>(null);
            var weak = Data[key];
            if (weak.TryGetTarget(out var ret))
                return ret;

            ret = Generator(key);
            weak.SetTarget(ret);
            return ret;
        }

        public void ForEach(Action<KeyValuePair<TK, TV?>> c)
        {
            foreach (var (key, value) in Data)
            {
                if (value.TryGetTarget(out var ret))
                    c(new KeyValuePair<TK, TV?>(key, ret));
            }
        }

        public void Clear()
        {
            Data.Clear();
        }
    }
}
