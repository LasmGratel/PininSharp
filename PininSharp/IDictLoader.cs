using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace PininSharp
{
    public interface IDictLoader
    {
        void Load(Action<char, string[]> feed);
    }

    public class DefaultDictLoader : IDictLoader
    {
        public void Load(Action<char, string[]> feed)
        {
            var assembly = typeof(PinIn).GetTypeInfo().Assembly;
            var resourceName = "PininSharp.data.gz";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
                if (stream != null)
                    using (var reader = new StreamReader(new GZipStream(stream, CompressionMode.Decompress)))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            var ch = line[0];
                            var records = line.Substring(3).Split(new[] { ", " }, StringSplitOptions.None);
                            feed(ch, records);
                        }
                    }
        }
    }
}
