using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PininSharp.Utils
{
    public readonly struct IndexSet : IEnumerable<int>
    {
        public static readonly IndexSet Zero = new IndexSet(0x1);
        public static readonly IndexSet One = new IndexSet(0x2);
        public static readonly IndexSet None = new IndexSet(0x0);

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


        public IEnumerator<int> GetEnumerator()
        {
            var v = _value;
            for (var i = 0; i < 7; i++)
            {
                if ((v & 0x1) == 0x1) yield return i;
                else if (v == 0) yield break;
                v >>= 1;
            }
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
            if (builder.Length != 0)
            {
                builder.Remove(builder.Length - 2, builder.Length);
                return builder.ToString();
            }

            return "0";
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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

            public IndexSet? Get(int index)
            {
                if (index >= _data.Length) return null;
                var ret = _data[index];
                return ret == 0 ? null : new IndexSet(ret - 1);
            }
        }
    }
}
