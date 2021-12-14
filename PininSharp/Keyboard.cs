using PininSharp.Elements;
using PininSharp.Utils;
using System;
using System.Collections.Generic;

namespace PininSharp
{
    public class Keyboard
    {
        private static readonly Dictionary<string, string> DaqianKeys = new Dictionary<string, string>
        {
            {"", ""}, {"0", ""}, {"1", " "}, {"2", "6"}, {"3", "3"},
            {"4", "4"}, {"a", "8"}, {"ai", "9"}, {"an", "0"}, {"ang", ";"},
            {"ao", "l"}, {"b", "1"}, {"c", "h"}, {"ch", "t"}, {"d", "2"},
            {"e", "k"}, {"ei", "o"}, {"en", "p"}, {"eng", "/"}, {"er", "-"},
            {"f", "z"}, {"g", "e"}, {"h", "c"}, {"i", "u"}, {"ia", "u8"},
            {"ian", "u0"}, {"iang", "u;"}, {"iao", "ul"}, {"ie", "u,"}, {"in", "up"},
            {"ing", "u/"}, {"iong", "m/"}, {"iu", "u."}, {"j", "r"}, {"k", "d"},
            {"l", "x"}, {"m", "a"}, {"n", "s"}, {"o", "i"}, {"ong", "j/"},
            {"ou", "."}, {"p", "q"}, {"q", "f"}, {"r", "b"}, {"s", "n"},
            {"sh", "g"}, {"t", "w"}, {"u", "j"}, {"ua", "j8"}, {"uai", "j9"},
            {"uan", "j0"}, {"uang", "j;"}, {"uen", "mp"}, {"ueng", "j/"}, {"ui", "jo"},
            {"un", "jp"}, {"uo", "ji"}, {"v", "m"}, {"van", "m0"}, {"vang", "m;"},
            {"ve", "m,"}, {"vn", "mp"}, {"w", "j"}, {"x", "v"}, {"y", "u"},
        };

        private static readonly Dictionary<string, string> XiaoheKeys = new Dictionary<string, string>
        {
            {"ai", "d"}, {"an", "j"}, {"ang", "h"}, {"ao", "c"}, {"ch", "i"},
            {"ei", "w"}, {"en", "f"}, {"eng", "g"}, {"ia", "x"}, {"ian", "m"},
            {"iang", "l"}, {"iao", "n"}, {"ie", "p"}, {"in", "b"}, {"ing", "k"},
            {"iong", "s"}, {"iu", "q"}, {"ong", "s"}, {"ou", "z"}, {"sh", "u"},
            {"ua", "x"}, {"uai", "k"}, {"uan", "r"}, {"uang", "l"}, {"ui", "v"},
            {"un", "y"}, {"uo", "o"}, {"ve", "t"}, {"ue", "t"}, {"vn", "y"},
        };

        private static readonly Dictionary<string, string> ZiranmaKeys = new Dictionary<string, string>
        {
            {"ai", "l"}, {"an", "j"}, {"ang", "h"}, {"ao", "k"}, {"ch", "i"},
            {"ei", "z"}, {"en", "f"}, {"eng", "g"}, {"ia", "w"}, {"ian", "m"},
            {"iang", "d"}, {"iao", "c"}, {"ie", "x"}, {"in", "n"}, {"ing", "y"},
            {"iong", "s"}, {"iu", "q"}, {"ong", "s"}, {"ou", "b"}, {"sh", "u"},
            {"ua", "w"}, {"uai", "y"}, {"uan", "r"}, {"uang", "d"}, {"ui", "v"},
            {"un", "p"}, {"uo", "o"}, {"ve", "t"}, {"ue", "t"}, {"vn", "p"},
            {"zh", "v"},
        };

        private static readonly Dictionary<string, string> PhoneticLocal = new Dictionary<string, string>
        {
            {"yi", "i"}, {"you", "iu"}, {"yin", "in"}, {"ye", "ie"}, {"ying", "ing"},
            {"wu", "u"}, {"wen", "un"}, {"yu", "v"}, {"yue", "ve"}, {"yuan", "van"},
            {"yun", "vn"}, {"ju", "jv"}, {"jue", "jve"}, {"juan", "jvan"}, {"jun", "jvn"},
            {"qu", "qv"}, {"que", "qve"}, {"quan", "qvan"}, {"qun", "qvn"}, {"xu", "xv"},
            {"xue", "xve"}, {"xuan", "xvan"}, {"xun", "xvn"}, {"shi", "sh"}, {"si", "s"},
            {"chi", "ch"}, {"ci", "c"}, {"zhi", "zh"}, {"zi", "z"}, {"ri", "r"},
        };

        public static Keyboard Quanpin = new Keyboard(null, null, StandardCutter, false);
        public static Keyboard Daqian = new Keyboard(PhoneticLocal, DaqianKeys, StandardCutter, false);
        public static Keyboard Xiaohe = new Keyboard(null, XiaoheKeys, ZeroCutter, true);
        public static Keyboard Ziranma = new Keyboard(null, ZiranmaKeys, ZeroCutter, true);

        private readonly Dictionary<string, string>? _local;
        private readonly Dictionary<string, string>? _keys;
        private readonly Func<string, ICollection<string>> _cutter;
        public readonly bool Duo;

        public Keyboard(Dictionary<string, string>? local, Dictionary<string, string>? keys,
        Func<string, ICollection<string>> cutter, bool duo)
        {
            _local = local;
            _keys = keys;
            _cutter = cutter;
            Duo = duo;
        }

        public static List<string> StandardCutter(string s)
        {
            var ret = new List<string>();
            var cursor = 0;

            // initial
            if (Pinyin.HasInitial(s))
            {
                cursor = s.Length > 2 && s[1] == 'h' ? 2 : 1;
                ret.Add(s[..cursor]);
            }

            // final
            if (s.Length != cursor + 1)
            {
                ret.Add(s[cursor..^1]);
            }

            // tone
            ret.Add(s[^1..]);

            return ret;
        }

        public string Keys(string s)
        {
            if (_keys == null)
                return s;
            return _keys.TryGetValue(s, out var ret) ? ret : s;
        }

        public static List<string> ZeroCutter(string s)
        {
            var ss = StandardCutter(s);
            if (ss.Count != 2) return ss;

            var finale = ss[0];
            ss[0] = finale[0].ToString();
            ss[1] = finale.Length == 2 ? finale[1].ToString() : finale;

            return ss;
        }

        public ICollection<string> Split(string s)
        {
            if (_local == null) return _cutter(s);

            var cut = s[..^1];
            if (_local.TryGetValue(cut, out var alt)) s = alt + s[^1];

            return _cutter(s);
        }
    }
}
