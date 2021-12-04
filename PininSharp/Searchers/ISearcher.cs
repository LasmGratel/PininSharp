using System.Collections.Generic;

namespace PininSharp
{
    public interface ISearcher<T>
    {
        void Put(string name, T identifier);

        List<T> Search(string name);

        PinIn Context { get; }
    }
}
