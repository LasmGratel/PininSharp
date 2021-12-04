using PininSharp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PininSharp.Elements
{
    public class Phoneme : IElement
    {
        private string[] _strs;

        public override string ToString()
        {
            return _strs[0];
        }

        public Phoneme(string str, PinIn p)
        {
            Reload(str, p);
        }

        public IndexSet Match(string source, IndexSet idx, int start, bool partial)
        {
            if (_strs.Length == 1 && _strs[0].Trim() == "") return new IndexSet(idx);
            var ret = new IndexSet();
            idx.ForEach(i =>
            {
                var set = Match(source, start + i, partial);
                set.Offset(i);
                ret.Merge(set);
            });
            return ret;
        }

        public bool IsEmpty()
        {
            return _strs.Length == 1 && _strs[0].Trim() == "";
        }

        private static int StrCmp(string a, string b, int aStart)
        {
            var len = Math.Min(a.Length - aStart, b.Length);
            for (var i = 0; i < len; i++)
                if (a[i + aStart] != b[i]) return i;
            return len;
        }

        public IndexSet Match(string source, int start, bool partial)
        {
            var ret = new IndexSet();
            if (_strs.Length == 1 && _strs[0].Trim() == "") return ret;
            foreach (var str in _strs)
            {
                var size = StrCmp(source, str, start);
                if (partial && start + size == source.Length) ret.Set(size);  // ending match
                else if (size == str.Length) ret.Set(size); // full match
            }
            return ret;
        }

        public void Reload(string str, PinIn p)
        {
            var ret = new HashSet<string>
            {
                str
            };

            if (p.fCh2C && str.StartsWith("c")) ret.AddAll(new[] { "c", "ch" });
            if (p.fSh2S && str.StartsWith("s")) ret.AddAll(new[] { "s", "sh" });
            if (p.fZh2Z && str.StartsWith("z")) ret.AddAll(new[] { "z", "zh" });
            if (p.fU2V && str.StartsWith("v"))
                ret.Add("u" + str.Substring(1));
            if ((p.fAng2An && str.EndsWith("ang"))
                    || (p.fEng2En && str.EndsWith("eng"))
                    || (p.fIng2In && str.EndsWith("ing")))
                ret.Add(str.SubstringInRange(0, str.Length - 1));
            if ((p.fAng2An && str.EndsWith("an"))
                    || (p.fEng2En && str.EndsWith("en"))
                    || (p.fIng2In && str.EndsWith("in")))
                ret.Add(str + 'g');
            _strs = ret.Select(p.Keyboard.Keys).ToArray();
        }
    }
}
