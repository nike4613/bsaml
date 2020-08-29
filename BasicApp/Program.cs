using System;
using System.ComponentModel;
using System.Linq;
using BSAML;

namespace BasicApp
{
    public class Program
    {
        static void Main(string[] args)
        {
            /*var obj = BSAMLCore.Parser.ParseXaml(Xaml);

            GlobalDataContext.DoTheThingChanged();
            GlobalDataContext.FirstThing.ThingChanged();
            GlobalDataContext.FirstThingChanged();

            BSAMLCore.Close();*/
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
<ExampleElement xmlns=""bsaml""
                xmlns:k=""knit""
                xmlns:a=""clr-namespace:BasicApp;assembly=BasicApp""
                xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
                DataContext=""{x:Static a:Program.GlobalDataContext}"">
    <ExampleElement Text=""{k:Binding DoTheThing}"">
        <ExampleElement Text=""{k:Binding DataContextIsInherited}""/>
    </ExampleElement>
    <ExampleElement Text=""{k:Binding FirstThing.Thing}"" ExampleElement.ScrollTarget=""true""/>
    <ExampleElement Text=""{k:Binding Thing}"" DataContext=""{k:Binding FirstThing}"" />
</ExampleElement>
";
    }
}
