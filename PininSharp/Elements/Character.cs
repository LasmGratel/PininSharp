﻿using PininSharp.Utils;
using System;

namespace PininSharp.Elements
{
    public class Character : IElement
    {
        protected char Ch;
        public static readonly Pinyin[] None = Array.Empty<Pinyin>();

        private Pinyin[] _pinyin;

        public Character(char ch, Pinyin[] pinyin)
        {
            Ch = ch;
            _pinyin = pinyin;
        }

        public IndexSet Match(string str, int start, bool partial)
        {
            var ret = (str[start] == Ch ? IndexSet.One : IndexSet.None).Copy();
            foreach (var p in _pinyin) ret.Merge(p.Match(str, start, partial));
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

        public sealed class Dummy : Character
        {
            public Dummy() : base('\0', None)
            {
            }

            public void Set(char ch)
            {
                Ch = ch;
            }
        }
    }
}
