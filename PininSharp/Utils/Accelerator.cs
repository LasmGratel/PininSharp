using PininSharp.Elements;
using System.Collections.Generic;

namespace PininSharp.Utils
{
    public class Accelerator
    {
        private readonly PinIn _context;
        private List<IndexSet.Storage> _cache;
        private char[] _searchChars;
        private string _searchStr;
        public IProvider Provider { get; private set; }


        private Str _str = new Str();
        private bool _partial;

        public Accelerator(PinIn context)
        {
            _context = context;
        }

        public void Search(string s)
        {
            if (s != _searchStr)
            {
                // here we store both search token as string and char array
                // it seems stupid, but saves over 10% of accelerator overhead
                _searchStr = s;
                _searchChars = s.ToCharArray();
                Reset();
            }
        }

        public IndexSet Get(char ch, int offset)
        {
            var c = _context.GetCharacter(ch);
            var ret = (_searchChars[offset] == c.Get() ? IndexSet.One : IndexSet.None).Copy();
            foreach (var p in c.Pinyins()) ret.Merge(Get(p, offset));
            return ret;
        }

        public IndexSet Get(Pinyin p, int offset)
        {
            for (var i = _cache.Count; i <= offset; i++)
                _cache.Add(new IndexSet.Storage());
            var data = _cache[offset];
            var ret = data.Get(p.Id);
            if (ret == null)
            {
                ret = p.Match(_searchStr, offset, _partial);
                data.Set(ret, p.Id);
            }
            return ret;
        }

        public void SetProvider(IProvider p)
        {
            Provider = p;
        }

        public void SetProvider(string s)
        {
            _str.S = s;
            Provider = _str;
        }

        public void Reset()
        {
            _cache = new List<IndexSet.Storage>();
        }

        // offset - offset in search string
        // start - start point in raw text
        public bool Check(int offset, int start)
        {
            if (offset == _searchStr.Length) return _partial || Provider.End(start);
            if (Provider.End(start)) return false;

            var s = Get(Provider.Get(start), offset);

            if (Provider.End(start + 1))
            {
                var i = _searchStr.Length - offset;
                return s.Get(i);
            }

            return !s.Traverse(i => !Check(offset + i, start + 1));
        }

        public bool Matches(int offset, int start)
        {
            if (_partial)
            {
                _partial = false;
                Reset();
            }
            return Check(offset, start);
        }

        public bool Begins(int offset, int start)
        {
            if (!_partial)
            {
                _partial = true;
                Reset();
            }
            return Check(offset, start);
        }

        public bool Contains(int offset, int start)
        {
            if (!_partial)
            {
                _partial = true;
                Reset();
            }
            for (var i = start; !Provider.End(i); i++)
            {
                if (Check(offset, i)) return true;
            }
            return false;
        }

        public string Search()
        {
            return _searchStr;
        }

        public int Common(int s1, int s2, int max)
        {
            for (var i = 0; ; i++)
            {
                if (i >= max) return max;
                var a = Provider.Get(s1 + i);
                var b = Provider.Get(s2 + i);
                if (a != b || a == '\0') return i;
            }
        }


        public interface IProvider
        {
            bool End(int i);

            char Get(int i);
        }

        public class Str : IProvider
        {
            public string S;

            public bool End(int i)
            {
                return i >= S.Length;
            }

            public char Get(int i)
            {
                return S[i];
            }
        }

    }
}
