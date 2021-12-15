using System;
using System.Collections.Generic;

namespace PininSharp.Utils
{
    public class Cache<TK, TV>
    {
        public Dictionary<TK, TV> Data = new();

        public Func<TK, TV> Generator;

        public Cache(Func<TK, TV> generator)
        {
            Generator = generator;
        }

        public TV Get(TK key)
        {
            if (Data.TryGetValue(key, out var ret)) return ret;
            ret = Generator(key);
            if (ret != null) Data[key] = ret;
            return ret;
        }

        public void ForEach(Action<KeyValuePair<TK, TV>> c)
        {
            foreach (var pair in Data)
            {
                c(pair);
            }
        }

        public void Clear()
        {
            Data.Clear();
        }
    }
}