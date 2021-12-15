using System;
using PininSharp.Utils;
using System.Collections.Generic;
using System.Linq;

namespace PininSharp.Searchers
{
    public class SimpleSearcher<T> : ISearcher<T>
    {
        protected readonly List<T> Objs = new();
        protected readonly Accelerator Acc;
        protected readonly Compressor Strs = new();

        public PinIn Context { get; internal set; }

        public SearcherLogic Logic { get; internal set; }

        public SimpleSearcher(SearcherLogic logic, PinIn context)
        {
            Context = context;
            Logic = logic;
            Acc = new Accelerator(context);
            Acc.SetProvider(Strs);
        }

        public virtual void Put(string name, T identifier)
        {
            Strs.Put(name);
            foreach (var t in name)
                Context.GetCharacter(t);

            Objs.Add(identifier);
        }

        public virtual List<T> Search(string name)
        {
            var ret = new List<T>();
            Acc.Search(name);
            var offsets = Strs.Offsets();
            for (var i = 0; i < offsets.Count; i++)
            {
                var s = offsets[i];
                if (Logic.Test(Acc, 0, s)) ret.Add(Objs[i]);
            }
            return ret;
        }
        public virtual void Reset()
        {
            Acc.Reset();
        }

        public override string ToString()
        {
            return $"SimpleSearcher_{Logic}";
        }
    }
}
