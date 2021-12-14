using System.Collections.Generic;
using System.Linq;

namespace PininSharp.Utils
{
    public class Compressor : Accelerator.IProvider
    {
        private readonly List<char> _chars = new List<char>();
        private readonly List<int> _strs = new List<int>();

        public List<int> Offsets()
        {
            return _strs;
        }

        public int Put(string s)
        {
            _strs.Add(_chars.Count);
            _chars.AddRange(s);

            _chars.Add('\0');
            return _strs[^1];
        }

        public bool End(int i)
        {
            return _chars[i] == '\0';
        }

        public char Get(int i)
        {
            return _chars[i];
        }
    }
}
