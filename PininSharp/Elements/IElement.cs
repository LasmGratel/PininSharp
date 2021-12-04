using PininSharp.Utils;

namespace PininSharp.Elements
{
    public interface IElement
    {
        IndexSet Match(string str, int start, bool partial);
    }
}
