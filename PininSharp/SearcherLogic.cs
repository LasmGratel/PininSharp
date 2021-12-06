using PininSharp.Utils;
using System;

namespace PininSharp
{
    public sealed class SearcherLogic
    {
        public static readonly SearcherLogic Begin = new SearcherLogic(
            (a, offset, start) => a.Begins(offset, start),
            (p, s1, s2) => p.Begins(s1, s2),
            (s1, s2) => s1.StartsWith(s2),
            "Begin"
        );

        public static readonly SearcherLogic Contain = new SearcherLogic(
            (a, offset, start) => a.Contains(offset, start),
            (p, s1, s2) => p.Contains(s1, s2),
            (s1, s2) => s1.Contains(s2),
            "Contain"
        );

        public static readonly SearcherLogic Equal = new SearcherLogic(
            (a, offset, start) => a.Matches(offset, start),
            (p, s1, s2) => p.Matches(s1, s2),
            (s1, s2) => s1 == s2,
            "Equal"
        );

        public static readonly SearcherLogic[] Values = { Begin, Contain, Equal };

        private readonly Func<Accelerator, int, int, bool> _test1;
        private readonly Func<PinIn, string, string, bool> _test2;
        private readonly Func<string, string, bool> _raw;
        private readonly string _name;

        public SearcherLogic(Func<Accelerator, int, int, bool> test1, Func<PinIn, string, string, bool> test2, Func<string, string, bool> raw, string name)
        {
            _test1 = test1;
            _test2 = test2;
            _raw = raw;
            _name = name;
        }

        public bool Test(Accelerator a, int offset, int start)
        {
            return _test1(a, offset, start);
        }

        public bool Test(PinIn p, string s1, string s2)
        {
            return _test2(p, s1, s2);
        }

        public bool Raw(string s1, string s2)
        {
            return _raw(s1, s2);
        }

        public override string ToString()
        {
            return _name;
        }
    }
}
