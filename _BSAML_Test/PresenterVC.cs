using HMUI;
using BSAML;
using UnityEngine;
using BSAML.Elements;
using System.Collections;
using IPA.Utilities.Async;
using BSAML.ViewControllers;

namespace _BSAML_Test
{
    internal class PresenterVC : PanelViewController
    {
        public override string? XAML => null;

        protected override void DidActivate(bool firstActivation, ActivationType activationType)
        {
            base.DidActivate(firstActivation, activationType);
        }
    }
}
