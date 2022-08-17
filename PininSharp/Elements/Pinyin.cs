using System;
using PininSharp.Utils;
using System.Linq;

namespace PininSharp.Elements
{
    public readonly struct Pinyin : IElement
    {
        private readonly bool _duo;
        private readonly bool _sequence;
        public readonly int Id;
        private readonly string _raw;

        public readonly Phoneme[] Phonemes;

        public Pinyin(string str, PinIn p, int id)
        {
            _raw = str;
            Id = id;
            var split = p.Keyboard.Split(str);
            var l = split.Select(p.GetPhoneme).ToList();

            Phonemes = l.Where(x => x != null).Select(x => x!.Value).ToArray();

            _duo = p.Keyboard.Duo;
            _sequence = p.Keyboard.Sequence;
        }

        public IndexSet Match(string str, int start, bool partial)
        {
            IndexSet ret;
            if (_duo)
            {
                // in shuangpin we require initial and final both present,
                // the phoneme, which is tone here, is optional
                ret = IndexSet.Zero;
                ret = Phonemes[0].Match(str, ret, start, partial);
                ret = Phonemes[1].Match(str, ret, start, partial);
                if (Phonemes.Length == 3)
                    ret = ret.Merge(Phonemes[2].Match(str, ret, start, partial));
            }
            else
            {
                // in other keyboards, match of precedent phoneme
                // is compulsory to match subsequent phonemes
                // for example, zhong1, z+h+ong+1 cannot match zong or zh1
                var active = IndexSet.Zero;
                ret = new IndexSet(0);
                foreach (var phoneme in Phonemes)
                {
                    active = phoneme.Match(str, active, start, partial);
                    if (active.IsEmpty()) break;
                    ret = ret.Merge(active);
                }
                
            }

            if (_sequence && Phonemes[0].MatchSequence(str[start]))
            {
                ret = ret.Set(1);
            }

            return ret;
        }

        public override string ToString()
        {
            return _raw;
        }

        public static readonly char[] VowelChars = { 'a', 'e', 'i', 'o', 'u', 'v' };

        public static bool HasInitial(string s)
        {
            return VowelChars.All(i => s[0] != i);
        }
    }
}
