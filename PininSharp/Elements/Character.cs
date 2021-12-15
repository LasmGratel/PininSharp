using PininSharp.Utils;
using System;
using System.Linq;

namespace PininSharp.Elements
{
    public readonly struct Character : IElement
    {
        public readonly char Ch;
        public static readonly Pinyin[] None = Array.Empty<Pinyin>();

        private readonly Pinyin[] _pinyin;

        public Character(char ch, in Pinyin[] pinyin)
        {
            Ch = ch;
            _pinyin = pinyin;
        }

        public IndexSet Match(string str, int start, bool partial)
        {
            var ret = (str[start] == Ch ? IndexSet.One : IndexSet.None).Copy();
            foreach (var p in _pinyin) ret = ret.Merge(p.Match(str, start, partial));
            return ret;
        }

        public char Get()
        {
            return Ch;
        }

        public Pinyin[] Pinyins()
        {
            return _pinyin;
        }
    }
}
