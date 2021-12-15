using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PininSharp.Utils
{
    public readonly ref struct IndexSet
    {
        public static IndexSet Zero => new(0x1);
        public static IndexSet One => new(0x2);
        public static IndexSet None => new(0x0);
        public static IndexSet Null => new(-1);

        private readonly int _value;

        public IndexSet(in IndexSet set)
        {
            _value = set._value;
        }

        public IndexSet(int value)
        {
            _value = value;
        }

        public IndexSet Set(int index)
        {
            var i = 0x1 << index;
            return new IndexSet(_value | i);
        }

        public bool Get(int index)
        {
            var i = 0x1 << index;
            return (_value & i) != 0;
        }

        public IndexSet Merge(in IndexSet s)
        {
            var value = _value;
            if (value == 0x1)
                value = s._value;
            else
                value = value |= s._value;
            return new IndexSet(value);
        }

        public bool Traverse(Func<int, bool> p)
        {
            var v = _value;
            for (var i = 0; i < 7; i++)
            {
                if ((v & 0x1) == 0x1 && !p(i)) return false;
                if (v == 0) return true;
                v >>= 1;
            }
            return true;
        }

        public void ForEach(Action<int> c)
        {
            var v = _value;
            for (var i = 0; i < 7; i++)
            {
                if ((v & 0x1) == 0x1) c(i);
                else if (v == 0) return;
                v >>= 1;
            }
        }

        public IndexSet Offset(int i)
        {
            return new IndexSet(_value << i);
        }

        public bool IsNull()
        {
            return _value == -1;
        }

        public IEnumerable<int> GetEnumerator()
        {
            var list = new List<int>(7);
            var v = _value;
            for (var i = 0; i < 7; i++)
            {
                if ((v & 0x1) == 0x1) list.Add(i);
                else if (v == 0) return list.AsEnumerable();
                v >>= 1;
            }

            return list.AsEnumerable();
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            Traverse(i =>
            {
                builder.Append(i);
                builder.Append(", ");
                return true;
            });
            if (builder.Length == 0) return "0";
            builder.Remove(builder.Length - 2, builder.Length);
            return builder.ToString();

        }

        public bool IsEmpty()
        {
            return _value == 0x0;
        }

        public IndexSet Copy()
        {
            return new IndexSet(_value);
        }

        public sealed class Storage
        {
            private int[] _data = new int[16];

            public void Set(IndexSet set, int index)
            {
                if (index >= _data.Length)
                {
                    var size = index;
                    size |= size >> 1;
                    size |= size >> 2;
                    size |= size >> 4;
                    size |= size >> 8;
                    size |= size >> 16;
                    var replace = new int[size + 1];
                    Array.Copy(_data, 0, replace, 0, _data.Length);
                    _data = replace;
                }
                _data[index] = set._value + 1;
            }

            public IndexSet Get(int index)
            {
                if (index >= _data.Length) return Null;
                var ret = _data[index];
                return ret == 0 ? Null : new IndexSet(ret - 1);
            }
        }
    }
}
