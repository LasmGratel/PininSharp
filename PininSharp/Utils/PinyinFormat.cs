using PininSharp.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace PininSharp.Utils
{
    public record PinyinFormat
    {
        // finals with tones on the second character
        private static readonly HashSet<string> Offset = new()
        {
            "ui", "iu", "uan", "uang", "ian", "iang", "ua",
            "ie", "uo", "iong", "iao", "ve", "ia"
        };

        private static readonly Dictionary<char, char> None = new()
        {
            {'a', 'a'}, {'o', 'o'}, {'e', 'e'}, {'i', 'i'}, {'u', 'u'}, {'v', 'ü'}
        };

        private static readonly Dictionary<char, char> First = new()
        {
            {'a', 'ā'}, {'o', 'ō'}, {'e', 'ē'}, {'i', 'ī'}, {'u', 'ū'}, {'v', 'ǖ'}
        };

        private static readonly Dictionary<char, char> Second = new()
        {
            {'a', 'á'}, {'o', 'ó'}, {'e', 'é'}, {'i', 'í'}, {'u', 'ú'}, {'v', 'ǘ'}
        };

        private static readonly Dictionary<char, char> Third = new()
        {
            {'a', 'ǎ'}, {'o', 'ǒ'}, {'e', 'ě'}, {'i', 'ǐ'}, {'u', 'ǔ'}, {'v', 'ǚ'}
        };

        private static readonly Dictionary<char, char> Fourth = new()
        {
            {'a', 'à'}, {'o', 'ò'}, {'e', 'è'}, {'i', 'ì'}, {'u', 'ù'}, {'v', 'ǜ'}
        };

        private static readonly List<Dictionary<char, char>> Tones = new()
        {
            None, First, Second, Third, Fourth
        };

        private static readonly Dictionary<string, string> Symbols = new()
        {
            {"a", "ㄚ"}, {"o", "ㄛ"}, {"e", "ㄜ"}, {"er", "ㄦ"}, {"ai", "ㄞ"},
            {"ei", "ㄟ"}, {"ao", "ㄠ"}, {"ou", "ㄡ"}, {"an", "ㄢ"}, {"en", "ㄣ"},
            {"ang", "ㄤ"}, {"eng", "ㄥ"}, {"ong", "ㄨㄥ"}, {"i", "ㄧ"}, {"ia", "ㄧㄚ"},
            {"iao", "ㄧㄠ"}, {"ie", "ㄧㄝ"}, {"iu", "ㄧㄡ"}, {"ian", "ㄧㄢ"}, {"in", "ㄧㄣ"},
            {"iang", "ㄧㄤ"}, {"ing", "ㄧㄥ"}, {"iong", "ㄩㄥ"}, {"u", "ㄨ"}, {"ua", "ㄨㄚ"},
            {"uo", "ㄨㄛ"}, {"uai", "ㄨㄞ"}, {"ui", "ㄨㄟ"}, {"uan", "ㄨㄢ"}, {"un", "ㄨㄣ"},
            {"uang", "ㄨㄤ"}, {"ueng", "ㄨㄥ"}, {"uen", "ㄩㄣ"}, {"v", "ㄩ"}, {"ve", "ㄩㄝ"},
            {"van", "ㄩㄢ"}, {"vang", "ㄩㄤ"}, {"vn", "ㄩㄣ"}, {"b", "ㄅ"}, {"p", "ㄆ"},
            {"m", "ㄇ"}, {"f", "ㄈ"}, {"d", "ㄉ"}, {"t", "ㄊ"}, {"n", "ㄋ"},
            {"l", "ㄌ"}, {"g", "ㄍ"}, {"k", "ㄎ"}, {"h", "ㄏ"}, {"j", "ㄐ"},
            {"q", "ㄑ"}, {"x", "ㄒ"}, {"zh", "ㄓ"}, {"ch", "ㄔ"}, {"sh", "ㄕ"},
            {"r", "ㄖ"}, {"z", "ㄗ"}, {"c", "ㄘ"}, {"s", "ㄙ"}, {"w", "ㄨ"},
            {"y", "ㄧ"}, {"1", ""}, {"2", "ˊ"}, {"3", "ˇ"}, {"4", "ˋ"},
            {"0", "˙"}, {"", ""}
        };

        private static readonly Dictionary<string, string> Local = new()
        {
            {"yi", "i"}, {"you", "iu"}, {"yin", "in"}, {"ye", "ie"}, {"ying", "ing"},
            {"wu", "u"}, {"wen", "un"}, {"yu", "v"}, {"yue", "ve"}, {"yuan", "van"},
            {"yun", "vn"}, {"ju", "jv"}, {"jue", "jve"}, {"juan", "jvan"}, {"jun", "jvn"},
            {"qu", "qv"}, {"que", "qve"}, {"quan", "qvan"}, {"qun", "qvn"}, {"xu", "xv"},
            {"xue", "xve"}, {"xuan", "xvan"}, {"xun", "xvn"}, {"shi", "sh"}, {"si", "s"},
            {"chi", "ch"}, {"ci", "c"}, {"zhi", "zh"}, {"zi", "z"}, {"ri", "r"}
        };

        public delegate string FormatDelegate(ref Pinyin p);

        public readonly FormatDelegate Format;

        public PinyinFormat(FormatDelegate format)
        {
            Format = format;
        }

        public static readonly PinyinFormat Raw = new(RawFormat);
        public static readonly PinyinFormat Number = new(NumberFormat);
        public static readonly PinyinFormat Phonetic = new(PhoneticFormat);
        public static readonly PinyinFormat Unicode = new(UnicodeFormat);

        private static string RawFormat(ref Pinyin p)
        {
            return p.ToString()[..^1];
        }

        private static string NumberFormat(ref Pinyin p)
        {
            return p.ToString();
        }

        private static string PhoneticFormat(ref Pinyin p)
        {
            var s = p.ToString();

            if (Local.TryGetValue(s[..^1], out var str)) s = str + s[^1];

            var sb = new StringBuilder();

            string[] split;
            if (!Pinyin.HasInitial(s))
            {
                split = new[] { "", s[..^1], s[^1..] };
            }
            else
            {
                var i = s.Length > 2 && s[1] == 'h' ? 2 : 1;
                split = new[] { s[..i], s[i..^1], s[^1..] };
            }

            var weak = split[2] == "0";
            if (weak) sb.Append(Symbols[split[2]]);
            sb.Append(Symbols[split[0]]);
            sb.Append(Symbols[split[1]]);
            if (!weak) sb.Append(Symbols[split[2]]);
            return sb.ToString();
        }

        private static string UnicodeFormat(ref Pinyin p)
        {
            var sb = new StringBuilder();
            var s = p.ToString();
            string finale;

            if (!Pinyin.HasInitial(s))
            {
                finale = s[..^1];
            }
            else
            {
                var i = s.Length > 2 && s[1] == 'h' ? 2 : 1;
                sb.Append(s, 0, i);
                finale = s[i..^1];
            }

            var offset = Offset.Contains(finale) ? 1 : 0;
            if (offset == 1) sb.Append(finale, 0, 1);
            var group = Tones[s[^1] - '0'];
            sb.Append(@group[finale[offset]]);
            if (finale.Length > offset + 1) sb.Append(finale, offset + 1, finale.Length - offset - 1);
            return sb.ToString();
        }
    }
}
