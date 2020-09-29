using Zenject;
using SiraUtil;

namespace _BSAML_Test
{
    public class BSAMLTestMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<PresenterVC>().FromNewComponentOnNewGameObject().AsSingle().OnInstantiated(Utilities.SetupViewController);
            Container.Bind<PresenterFC>().FromNewComponentOnNewGameObject().AsSingle();
            Container.Bind<TestMenuManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
        }
    }
}
