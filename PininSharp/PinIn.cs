using PininSharp.Elements;
using PininSharp.Utils;
using System;
using System.Linq;
using System.Threading;

namespace PininSharp
{
    public class PinIn
    {
        private int _total;

        private readonly Cache<string, Phoneme> _phonemes;
        private readonly Cache<string, Pinyin> _pinyins;
        private readonly Character?[] _chars = new Character[char.MaxValue];
        private readonly Character.Dummy _temp = new Character.Dummy();
        private readonly ThreadLocal<Accelerator> _acc;

        public Keyboard Keyboard { get; set; } = Keyboard.Quanpin;
        public int Modification { get; set; }
        public bool fZh2Z { get; set; } = false;
        public bool fSh2S { get; set; } = false;
        public bool fCh2C { get; set; } = false;
        public bool fAng2An { get; set; } = false;
        public bool fIng2In { get; set; } = false;
        public bool fEng2En { get; set; } = false;
        public bool fU2V { get; set; } = false;
        public bool Accelerate { get; set; } = false;
        public PinyinFormat format = PinyinFormat.Number;
        public IDictLoader Loader;

        /**
         * Use PinIn object to manage the context
         * To configure it, use {@link #config()}
         */
        public PinIn() : this(new DefaultDictLoader())
        {
        }

        public PinIn(IDictLoader loader)
        {
            _phonemes = new Cache<string, Phoneme>(s => new Phoneme(s, this));
            _pinyins = new Cache<string, Pinyin>(s => new Pinyin(s, this, _total++));

            _acc = new ThreadLocal<Accelerator>(() => new Accelerator(this));

            Loader = loader;
        }

        public void Load()
        {
            Loader.Load((c, ss) =>
            {
                if (ss == null)
                {
                    _chars[c] = null;
                }
                else
                {
                    var pinyins = new Pinyin?[ss.Length];
                    for (var i = 0; i < ss.Length; i++)
                    {
                        pinyins[i] = GetPinyin(ss[i]);
                        if (pinyins[i] == null) Console.WriteLine("Cannot get pinyin for {0}", ss[i]);
                    }
                    _chars[c] = new Character(c, pinyins.Where(x => x != null).Select(x => x!.Value).ToArray());
                }
            });
        }

        public static PinIn CreateDefault()
        {
            var p = new PinIn();
            p.Load();
            return p;
        }

        public bool Contains(string s1, string s2)
        {
            if (!Accelerate) return Matcher.Contains(s1, s2, this);
            var a = _acc.Value;
            a.SetProvider(s1);
            a.Search(s2);
            return a.Contains(0, 0);

        }

        public bool Begins(string s1, string s2)
        {
            if (!Accelerate) return Matcher.Begins(s1, s2, this);
            var a = _acc.Value;
            a.SetProvider(s1);
            a.Search(s2);
            return a.Begins(0, 0);

        }

        public bool Matches(string s1, string s2)
        {
            if (Accelerate)
            {
                var a = _acc.Value;
                a.SetProvider(s1);
                a.Search(s2);
                return a.Matches(0, 0);
            }

            return Matcher.Matches(s1, s2, this);
        }

        public Phoneme? GetPhoneme(string s)
        {
            return _phonemes.Get(s);
        }

        public Pinyin? GetPinyin(string s)
        {
            return _pinyins.Get(s);
        }

        public Character GetCharacter(char c)
        {
            var ret = _chars[c];
            if (ret != null)
            {
                return ret;
            }

            _temp.Set(c);
            return _temp;
        }

        public string Format(Pinyin p)
        {
            return format.Format(p);
        }


        public Ticket CreateTicket(Action r)
        {
            return new Ticket(r, Modification);
        }

        public void CommitChanges()
        {
            _phonemes.ForEach(pair => pair.Value?.Reload(pair.Key, this));
            Modification++;
        }

        public static class Matcher
        {
            public static bool Begins(string s1, string s2, PinIn p)
            {
                if (s1.Trim() == "") return s1.StartsWith(s2, StringComparison.Ordinal);
                return Check(s1, 0, s2, 0, p, true);
            }

            public static bool Contains(string s1, string s2, PinIn p)
            {
                if (s1.Trim() == "") return s1.Contains(s2, StringComparison.Ordinal);
                return s1.Where((t, i) => Check(s1, i, s2, 0, p, true)).Any();
            }

            public static bool Matches(string s1, string s2, PinIn p)
            {
                if (s1.Trim() == "") return s1 == s2;
                return Check(s1, 0, s2, 0, p, false);
            }

            private static bool Check(string s1, int start1, string s2, int start2, PinIn p, bool partial)
            {
                if (start2 == s2.Length) return partial || start1 == s1.Length;

                IElement r = p.GetCharacter(s1[start1]);
                var s = r.Match(s2, start2, partial);

                if (start1 == s1.Length - 1)
                {
                    var i = s2.Length - start2;
                    return s.Get(i);
                }

                return !s.Traverse(i => !Check(s1, start1 + 1, s2, start2 + i, p, partial));
            }
        }

        public class Ticket
        {
            private int _modification;
            private readonly Action _action;

            public Ticket(Action r, int modification)
            {
                _action = r;
                _modification = modification;
            }

            public void Renew(int modification)
            {
                if (_modification == modification) return;
                _modification = modification;
                _action();
            }
        }
    }
}
