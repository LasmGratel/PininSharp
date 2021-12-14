using PininSharp.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PininSharp.Elements
{
    public class Phoneme : IElement
    {
        private string[] _strings;

        public override string ToString()
        {
            return _strings[0];
        }

        public Phoneme(string str, PinIn p)
        {
            _strings = Array.Empty<string>();
            Reload(str, p);
        }

        public IndexSet Match(ref string source, IndexSet idx, int start, bool partial)
        {
            if (_strings.Length == 1 && _strings[0].Trim() == "") return new IndexSet(idx);
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
            return _strings.Length == 1 && _strings[0].Trim() == "";
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
            if (_strings.Length == 1 && _strings[0].Trim() == "") return ret;
            foreach (var str in _strings)
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
                ret.Add("u" + str[1..]);
            if ((p.fAng2An && str.EndsWith("ang"))
                    || (p.fEng2En && str.EndsWith("eng"))
                    || (p.fIng2In && str.EndsWith("ing")))
                ret.Add(str[..^1]);
            if ((p.fAng2An && str.EndsWith("an"))
                    || (p.fEng2En && str.EndsWith("en"))
                    || (p.fIng2In && str.EndsWith("in")))
                ret.Add(str + 'g');
            _strings = ret.Select(p.Keyboard.Keys).ToArray();
        }

        public override int GetHashCode()
        {
            return _strings.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(_strings, ((Phoneme) obj)._strings);
        }
    }
}
