using HMUI;
using Zenject;
using BeatSaberMarkupLanguage;

namespace _BSAML_Test
{
    public class PresenterFC : FlowCoordinator
    {
        [Inject] private PresenterVC presenterA = null!;
        [Inject] private MainFlowCoordinator mainFlowCoordinator = null!;

        protected override void DidActivate(bool firstActivation, ActivationType activationType)
        {
            if (firstActivation)
            {
                title = "BSAML Tests";
                showBackButton = true;
            }
            ProvideInitialViewControllers(presenterA);
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            base.BackButtonWasPressed(topViewController);
            mainFlowCoordinator.DismissFlowCoordinator(this);
        }
    }
}
