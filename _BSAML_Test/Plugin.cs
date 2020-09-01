using BSAML;
using IPA;
using IPA.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _BSAML_Test
{
    [Plugin(RuntimeOptions.DynamicInit)]
    internal class Plugin
    {
        public readonly Logger Logger;
        public readonly DynamicParser Parser;
        
        [Init]
        public Plugin(Logger logger, DynamicParser parser)
        {
            Logger = logger;
            Parser = parser;
            logger.Debug($"Initialized with {Parser}");
        }

        [OnEnable]
        public void OnEnable()
        {
            var obj = Parser.ParseXaml(Xaml);
            Logger.Notice($"Parsed Xaml into {obj}");

            GlobalDataContext.DoTheThingChanged();
            GlobalDataContext.FirstThing.ThingChanged();
            GlobalDataContext.FirstThingChanged();
        }

        public static DataObject GlobalDataContext { get; } = new DataObject();

        public class DataObject : INotifyPropertyChanged
        {
            public string DoTheThing => "Hello!";
            public string DataContextIsInherited => "The data context is inherited!";

            public class FirstThing_ : INotifyPropertyChanged
            {
                public string Thing => "This is FirstThing.Thing";

                public void ThingChanged() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Thing)));

                public event PropertyChangedEventHandler? PropertyChanged;
            }
            public FirstThing_ FirstThing { get; } = new FirstThing_();

            public event PropertyChangedEventHandler? PropertyChanged;
            public void FirstThingChanged() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FirstThing)));
            public void DoTheThingChanged() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DoTheThing)));
        }

        const string Xaml = @"
<TestRoot xmlns=""bsaml""
          xmlns:k=""knit""
          xmlns:a=""clr-namespace:_BSAML_Test;assembly=_BSAML_Test""
          xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
          DataContext=""{x:Static a:Plugin.GlobalDataContext}"">
    <ExampleElement Text=""{k:Binding DoTheThing}"">
        <ExampleElement Text=""{k:Binding DataContextIsInherited}""/>
    </ExampleElement>
    <ExampleElement Text=""{k:Binding FirstThing.Thing}"" ExampleElement.ScrollTarget=""true""/>
    <ExampleElement Text=""{k:Binding Thing}"" DataContext=""{k:Binding FirstThing}"" />
</TestRoot>
";
    }
}
