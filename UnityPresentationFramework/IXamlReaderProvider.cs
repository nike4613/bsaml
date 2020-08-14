using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xaml;

namespace UnityPresentationFramework
{
    public interface IXamlReaderProvider
    {
        XamlReader FromStream(Stream stream);
        XamlReader FromTextReader(TextReader reader);
        XamlObjectWriterSettings SettingsWithRoot(object? root);
    }
}
