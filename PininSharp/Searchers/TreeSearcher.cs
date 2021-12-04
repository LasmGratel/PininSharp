using PininSharp.Elements;
using PininSharp.Utils;
using System.Collections.Generic;
using System.Linq;

namespace PininSharp.Searchers
{
    public class TreeSearcher<T> : ISearcher<T>
    {
        public INode<T> Root = new NDense<T>();

        public readonly List<T> Objects = new List<T>();
        public readonly List<NAcc<T>> Naccs = new List<NAcc<T>>();
        public readonly Accelerator Acc;
        public readonly Compressor Strs = new Compressor();
        public PinIn Context { get; internal set; }
        public readonly SearcherLogic Logic;
        public readonly PinIn.Ticket Ticket;
        public static readonly int Threshold = 128;

        public TreeSearcher(SearcherLogic logic, PinIn context)
        {
            Logic = logic;
            Context = context;
            Acc = new Accelerator(context);
            Acc.SetProvider(Strs);
            Ticket = context.CreateTicket(() =>
            {
                Naccs.ForEach(i => i.Reload(this));
                Acc.Reset();
            });
        }

        public void Put(string name, T identifier)
        {
            Ticket.Renew(Context.Modification);
            var pos = Strs.Put(name);
            var end = Logic == SearcherLogic.Contain ? name.Length : 1;
            for (var i = 0; i < end; i++)
                Root = Root.Put(this, pos + i, Objects.Count);
            Objects.Add(identifier);
        }

        public List<T> Search(string s)
        {
            Ticket.Renew(Context.Modification);
            Acc.Search(s);
            var ret = new SortedSet<int>();
            Root.Get(this, ret, 0);
            return ret.Select(i => Objects[i]).ToList();
        }

        public void Refresh()
        {
            Ticket.Renew(Context.Modification);
        }
    }



    public interface INode<T>
    {
        void Get(TreeSearcher<T> p, ISet<int> ret, int offset);

        void Get(TreeSearcher<T> p, ISet<int> ret);

        INode<T> Put(TreeSearcher<T> p, int name, int identifier);

        Accelerator Acc { get; }
    }

    public class NSlice<T> : INode<T>
    {
        private INode<T> _exit = new NMap<T>();
        private readonly int _start;
        private int _end;
        public Accelerator Acc { get; internal set; }

        public NSlice(int start, int end)
        {
            _start = start;
            _end = end;
        }


        public void Get(TreeSearcher<T> p, ISet<int> ret, int offset)
        {
            Get(p, ret, offset, 0);
        }


        public void Get(TreeSearcher<T> p, ISet<int> ret)
        {
            _exit.Get(p, ret);
        }


        public INode<T> Put(TreeSearcher<T> p, int name, int identifier)
        {
            var length = _end - _start;
            var match = p.Acc.Common(_start, name, length);
            if (match >= length) _exit = _exit.Put(p, name + length, identifier);
            else
            {
                Cut(p, _start + match);
                _exit = _exit.Put(p, name + match, identifier);
            }

            return _start == _end ? _exit : this;
        }

        private void Cut(TreeSearcher<T> p, int offset)
        {
            var insert = new NMap<T>();
            if (offset + 1 == _end) insert.Put(p.Strs.Get(offset), _exit);
            else
            {
                var half = new NSlice<T>(offset + 1, _end)
                {
                    _exit = _exit
                };
                insert.Put(p.Strs.Get(offset), half);
            }

            _exit = insert;
            _end = offset;
        }

        private void Get(TreeSearcher<T> p, ISet<int> ret, int offset, int start)
        {
            if (_start + start == _end)
                _exit.Get(p, ret, offset);
            else if (offset == p.Acc.Search().Length)
            {
                if (p.Logic != SearcherLogic.Equal) _exit.Get(p, ret);
            }
            else
            {
                var ch = p.Strs.Get(_start + start);
                p.Acc.Get(ch, offset).ForEach(i =>
                    Get(p, ret, offset + i, start + 1));
            }
        }
    }

    public class NDense<T> : INode<T>
    {
        public Accelerator Acc { get; internal set; }

        // offset, object, offset, object
        private readonly List<int> _data = new List<int>();

        public void Get(TreeSearcher<T> p, ISet<int> ret, int offset)
        {
            var full = p.Logic == SearcherLogic.Equal;
            if (!full && p.Acc.Search().Length == offset) Get(p, ret);
            else
            {
                for (var i = 0; i < _data.Count / 2; i++)
                {
                    var ch = _data[i * 2];
                    if (full ? p.Acc.Matches(offset, ch) : p.Acc.Begins(offset, ch))
                        ret.Add(_data[i * 2 + 1]);
                }
            }
        }

        public void Get(TreeSearcher<T> p, ISet<int> ret)
        {
            for (var i = 0; i < _data.Count / 2; i++)
                ret.Add(_data[i * 2 + 1]);
        }

        public INode<T> Put(TreeSearcher<T> p, int name, int identifier)
        {
            if (_data.Count >= TreeSearcher<T>.Threshold)
            {
                var pattern = _data[0];
                INode<T> ret = new NSlice<T>(pattern, pattern + Match(p));
                for (var j = 0; j < _data.Count / 2; j++)
                    ret.Put(p, _data[j * 2], _data[j * 2 + 1]);
                ret.Put(p, name, identifier);
                return ret;
            }

            _data.Add(name);
            _data.Add(identifier);
            return this;
        }

        private int Match(TreeSearcher<T> p)
        {
            for (var i = 0; ; i++)
            {
                var a = p.Strs.Get(_data[0] + i);
                for (var j = 1; j < _data.Count / 2; j++)
                {
                    var b = p.Strs.Get(_data[j * 2] + i);
                    if (a != b || a == '\0') return i;
                }
            }
        }
    }

    public class NMap<T> : INode<T>
    {
        public Accelerator Acc { get; internal set; }
        public Dictionary<char, INode<T>> Children;
        public ISet<int> Leaves = new SortedSet<int>();


        public virtual void Get(TreeSearcher<T> p, ISet<int> ret, int offset)
        {
            if (p.Acc.Search().Length == offset)
            {
                if (p.Logic == SearcherLogic.Equal)
                    foreach (var leaf in Leaves)
                    {
                        ret.Add(leaf);
                    }

                else Get(p, ret);
            }
            else if (Children != null)
            {
                foreach (var pair in Children)
                {
                    pair.Value.Acc.Get(pair.Key, offset)
                        .ForEach(i => pair.Value.Get(p, ret, offset + i));
                }
            }
        }


        public virtual void Get(TreeSearcher<T> p, ISet<int> ret)
        {
            foreach (var leaf in Leaves)
            {
                ret.Add(leaf);
            }

            if (Children == null) return;
            foreach (var value in Children.Values)
            {
                value.Get(p, ret);
            }
        }


        public virtual INode<T> Put(TreeSearcher<T> p, int name, int identifier)
        {
            if (p.Strs.Get(name) == '\0')
            {
                if (Leaves.Count >= TreeSearcher<T>.Threshold && Leaves != null)
                    Leaves = new SortedSet<int>(Leaves)
                        {
                            identifier
                        };
            }
            else
            {
                Init();
                var ch = p.Strs.Get(name);
                var sub = Children[ch];
                if (sub == null) Put(ch, sub = new NDense<T>());
                sub = sub.Put(p, name + 1, identifier);
                Children[ch] = sub;
            }

            return !(this is NAcc<T>) && Children != null && Children.Count > 32 ? new NAcc<T>(p, this) : this;
        }

        public void Put(char ch, INode<T> n)
        {
            Init();
            if (Children.Count >= TreeSearcher<T>.Threshold && Children != null)
                Children = new Dictionary<char, INode<T>>(Children)
                {
                    { ch, n }
                };
        }

        private void Init()
        {
            if (Children == null) Children = new Dictionary<char, INode<T>>();
        }
    }

    public class NAcc<T> : NMap<T>
    {
        private readonly Dictionary<Phoneme, ISet<char>> _index = new Dictionary<Phoneme, ISet<char>>();

        public NAcc(TreeSearcher<T> p, NMap<T> n)
        {
            Children = n.Children;
            Leaves = n.Leaves;
            Reload(p);
            p.Naccs.Add(this);
        }


        public override void Get(TreeSearcher<T> p, ISet<int> ret, int offset)
        {
            if (p.Acc.Search().Length == offset)
            {
                if (p.Logic == SearcherLogic.Equal)
                    foreach (var leaf in Leaves)
                    {
                        ret.Add(leaf);
                    }

                else Get(p, ret);
            }
            else
            {
                if (Children.TryGetValue(p.Acc.Search()[offset], out var n)) n.Get(p, ret, offset + 1);
                foreach (var c in _index.Where(pair => !pair.Key.Match(p.Acc.Search(), offset, true).IsEmpty()).SelectMany(pair => pair.Value))
                {
                    p.Acc.Get(c, offset).ForEach(j => Children[c].Get(p, ret, offset + j));
                }
            }
        }


        public override INode<T> Put(TreeSearcher<T> p, int name, int identifier)
        {
            base.Put(p, name, identifier);
            Index(p, p.Strs.Get(name));
            return this;
        }

        public void Reload(TreeSearcher<T> p)
        {
            _index.Clear();
            foreach (var i in Children.Keys)
            {
                Index(p, i);
            }
        }

        private void Index(ISearcher<T> p, char c)
        {
            var ch = p.Context.GetCharacter(c);
            foreach (var py in
                ch.Pinyins())
            {
                var key = py.Phonemes[0];
                if (_index.TryGetValue(py.Phonemes[0], out var value))
                {
                    if (value.Count >= TreeSearcher<T>.Threshold && !value.Contains(c))
                    {
                        _index[py.Phonemes[0]] = new SortedSet<char>(value); // Should be CharOpenHashSet
                    }
                }
                else
                {
                    _index[py.Phonemes[0]] = new HashSet<char>(); // Should be CharArraySet
                }

                _index[py.Phonemes[0]].Add(c);
            }
        }
    }
}