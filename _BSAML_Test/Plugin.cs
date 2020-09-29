using BeatSaberMarkupLanguage;
using BS_Utils.Utilities;
using BSAML;
using BSAML.Elements;
using HMUI;
using IPA;
using IPA.Logging;
using SiraUtil.Zenject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = IPA.Logging.Logger;

namespace _BSAML_Test
{
    [Plugin(RuntimeOptions.DynamicInit)]
    internal class Plugin
    {
        public readonly Logger Logger;
        public readonly DynamicParser Parser;
        
        [Init]
        public Plugin(Logger logger, DynamicParser parser, Zenjector zenjector)
        {
            Logger = logger;
            Parser = parser;
            logger.Debug($"Initialized with {Parser}");

            zenjector.OnMenu<BSAMLTestMenuInstaller>();
        }

        [OnEnable]
        public void OnEnable()
        {
            var obj = Parser.ParseXaml(Xaml);
            Logger.Notice($"Parsed Xaml into {obj}");

            GlobalDataContext.DoTheThingChanged();
            GlobalDataContext.FirstThing.ThingChanged();
            GlobalDataContext.FirstThingChanged();

            BSEvents.menuSceneActive += OnMenuLoaded;
        }

        [OnDisable]
        public void OnDisable()
        {
            BSEvents.menuSceneActive -= OnMenuLoaded;
        }

        private void OnMenuLoaded()
        {
            SharedCoroutineStarter.instance.StartCoroutine(SetupCoro());
        }

        private IEnumerator SetupCoro()
        {
            yield return new WaitForSeconds(1f);
            /*
            var parsed = (ViewPanel)Parser.ParseXaml(TestActualElements);

            var presenter = BeatSaberUI.CreateViewController<PresenterVC>();
            presenter.Panel = parsed;

            Resources.FindObjectsOfTypeAll<MainFlowCoordinator>().First().InvokeMethod("PresentViewController", presenter, null, false);*/
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

        const string TestActualElements = @"
<ViewPanel xmlns=""bsaml""
           xmlns:k=""knit""
           xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
    <Text Value=""Hello, default text!"" />
    <Text Value=""Big font!"" FontSize=""6"" />
</ViewPanel>
";
    }
}
