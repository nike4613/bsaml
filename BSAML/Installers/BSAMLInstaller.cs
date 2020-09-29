using Knit;
using System;
using Serilog;
using Zenject;
using Microsoft.Extensions.DependencyInjection;

namespace BSAML.Installers
{
    internal class BSAMLInstaller : Installer<IServiceProvider, BSAMLInstaller>
    {
        internal const string PROVIDER_ID = "BSAMLProvider";

        private readonly IServiceProvider serviceProvider;

        public BSAMLInstaller(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public override void InstallBindings()
        {
            Container.BindInstance(serviceProvider).WithId(PROVIDER_ID).AsSingle();
            Container.Bind<DynamicParser>().FromFactory<ParserFactory>();
        }
    }

    internal class ParserFactory : IFactory<DynamicParser>
    {
        private readonly IServiceProvider serviceProvider;

        public ParserFactory([Inject(Id = BSAMLInstaller.PROVIDER_ID)] IServiceProvider provider) => serviceProvider = provider;

        public DynamicParser Create()
        {
            var parser = new DynamicParser(
                    serviceProvider.GetRequiredService<IXamlReaderProvider>(),
                    serviceProvider.GetRequiredService<ILogger>(),
                    serviceProvider
                );
            return parser;
        }
    }
}