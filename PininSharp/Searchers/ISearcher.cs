using System.Collections.Generic;

namespace PininSharp
{
    public interface ISearcher<T>
    {
        SearcherLogic Logic { get; }

        void Put(string name, T identifier);

        List<T> Search(string name);

        PinIn Context { get; }
    }
}
