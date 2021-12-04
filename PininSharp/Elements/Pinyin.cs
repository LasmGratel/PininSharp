using PininSharp.Utils;
using System.Linq;

namespace PininSharp.Elements
{
    public class Pinyin : IElement
    {
        private bool _duo;
        public readonly int Id;
        private string _raw;

        public Phoneme[] Phonemes { get; internal set; }

        public Pinyin(string str, PinIn p, int id)
        {
            _raw = str;
            Id = id;
            Reload(str, p);
        }

        public IndexSet Match(string str, int start, bool partial)
        {
            if (_duo)
            {
                // in shuangpin we require initial and final both present,
                // the phoneme, which is tone here, is optional
                var ret = IndexSet.Zero;
                ret = Phonemes[0].Match(str, ret, start, partial);
                ret = Phonemes[1].Match(str, ret, start, partial);
                if (Phonemes.Length == 3)
                    ret.Merge(Phonemes[2].Match(str, ret, start, partial));
                return ret;
            }
            else
            {
                // in other keyboards, match of precedent phoneme
                // is compulsory to match subsequent phonemes
                // for example, zhong1, z+h+ong+1 cannot match zong or zh1
                var active = IndexSet.Zero;
                var ret = new IndexSet();
                foreach (var phoneme in Phonemes)
                {
                    active = phoneme.Match(str, active, start, partial);
                    if (active.IsEmpty()) break;
                    ret.Merge(active);
                }
                return ret;
            }
        }

        public override string ToString()
        {
            return _raw;
        }

        public void Reload(string str, PinIn p)
        {
            var split = p.Keyboard.Split(str);
            var l = split.Select(p.GetPhoneme).ToList();
            if (str[1] == 'h' && p.Keyboard == Keyboard.Quanpin)
            {
                // here we implement sequence matching in quanpin, with a dirty trick
                // if initial is one of 'zh' 'sh' 'ch', and fuzzy is not on, we slice it
                // the first is one if 'z' 's' 'c', and the second is 'h'
                bool slice;
                var sequence = str[0];
                switch (sequence)
                {
                    case 'z':
                        slice = !p.fZh2Z;
                        break;
                    case 'c':
                        slice = !p.fCh2C;
                        break;
                    case 's':
                        slice = !p.fSh2S;
                        break;
                    default:
                        slice = false;
                        break;
                }
                if (slice)
                {
                    l[0] = p.GetPhoneme(sequence.ToString());
                    l.Insert(1, p.GetPhoneme("h"));
                }
            }
            Phonemes = l.ToArray();

            _duo = p.Keyboard.Duo;
        }

        public static bool HasInitial(string s)
        {
            return new[] { 'a', 'e', 'i', 'o', 'u', 'v' }.All(i => s[0] != i);
        }
    }
}
