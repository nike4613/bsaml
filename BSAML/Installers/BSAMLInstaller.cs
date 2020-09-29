using System;
using Zenject;

namespace BSAML.Installers
{
    internal class BSAMLInstaller : Installer<IServiceProvider, BSAMLInstaller>
    {
        private readonly IServiceProvider serviceProvider;

        public BSAMLInstaller(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public override void InstallBindings()
        {
            Container.BindInstance(serviceProvider).WithId("BSAMLProvider").AsSingle();    
        }
    }
}