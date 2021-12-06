using PininSharp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PininSharp.Searchers
{
    public class CachedSearcher<T> : SimpleSearcher<T>
    {
        private readonly List<int> _all = new List<int>();
        private readonly float _scale;
        private int _lenCached;  // longest string with cached result
        private int _maxCached;  // maximum amount of cached results
        private int _total;  // total characters of all strings
        private readonly Stats<string> _stats = new Stats<string>();

        private readonly Dictionary<string, List<int>> _cache = new Dictionary<string, List<int>>();

        public CachedSearcher(SearcherLogic logic, PinIn context) : this(logic, context, 1)
        {
        }

        public CachedSearcher(SearcherLogic logic, PinIn context, float scale) : base(logic, context)
        {
            this._scale = scale;
        }

        public override void Put(string name, T identifier)
        {
            Reset();
            foreach (var c in name)
                Context.GetCharacter(c);

            _total += name.Length;
            _all.Add(_all.Count);
            _lenCached = 0;
            _maxCached = 0;
            base.Put(name, identifier);
        }

        public override List<T> Search(string name)
        {
            Ticket.Renew(Context.Modification);
            if (_all.Count == 0) return new List<T>();

            if (_maxCached == 0)
            {
                float totalSearch = Logic == SearcherLogic.Contain ? _total : _all.Count;
                _maxCached = (int)(_scale * Math.Ceiling(2 * Math.Log(totalSearch) / Math.Log(2) + 16));
            }
            if (_lenCached == 0) _lenCached = (int)Math.Ceiling(Math.Log(_maxCached) / Math.Log(8));
            return Test(name).Select(i => Objs[i]).ToList();
        }

        public override void Reset()
        {
            base.Reset();
            _stats.Reset();
            _lenCached = 0;
            _maxCached = 0;
        }

        private List<int> Filter(string name)
        {
            if (name.Trim() == "") return _all;

            _stats.Count(name);

            if (_cache.TryGetValue(name, out var ret)) return ret;
            var baseList = this.Filter(name.SubstringInRange(0, name.Length - 1));
            if (_cache.Count >= _maxCached)
            {
                var least = _stats.Least(_cache.Keys, name);
                if (!least.Equals(name)) _cache.Remove(least);
                else return baseList;
            }

            Acc.Search(name);
            var filter = Logic == SearcherLogic.Equal ? SearcherLogic.Begin : Logic;
            var tmp = baseList.Where(i => filter.Test(Acc, 0, Strs.Offsets()[i])).ToList();

            if (tmp.Count == baseList.Count)
            {
                ret = baseList;
            }
            else
            {
                tmp.TrimExcess();
                ret = tmp;
            }

            _cache[name] = ret;

            return ret;
        }

        private List<int> Test(string name)
        {
            var list = Filter(name.SubstringInRange(0, Math.Min(name.Length, _lenCached)));
            if (Logic == SearcherLogic.Equal || name.Length > _lenCached)
            {
                Acc.Search(name);
                return list.Where(i => Logic.Test(Acc, 0, Strs.Offsets()[i])).ToList();
            }

            return list;
        }

        public override string ToString()
        {
            return $"CachedSearcher_{Logic}";
        }


    }

    public sealed class Stats<T>
    {
        private readonly Dictionary<T, int> _data = new Dictionary<T, int>();

        public void Count(T key)
        {
            var cnt = _data.TryGetValue(key, out var x) ? x + 1 : 1;
            _data[key] = cnt;
            if (cnt == int.MaxValue)
            {
                foreach (var pair in _data)
                {
                    _data[pair.Key] = pair.Value / 2;
                }
            }
        }

        public T Least(ICollection<T> keys, T extra)
        {
            var ret = extra;
            var cnt = _data[extra];
            foreach (var i in keys)
            {
                var value = _data[i];
                if (value < cnt)
                {
                    ret = i;
                    cnt = value;
                }
            }
            return ret;
        }

        public void Reset()
        {
            _data.Clear();
        }
    }
}
