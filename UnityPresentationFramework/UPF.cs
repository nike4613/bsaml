using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPresentationFramework.Parsing;

namespace UnityPresentationFramework
{
    public static class UPF
    {
        private static readonly IServiceProvider services = CreateServices();

        private static IServiceProvider CreateServices()
        {
            var collection = new ServiceCollection()
                .AddSingleton<IBindingReflector, SystemReflectionReflector>()
                .AddSingleton<IXamlReaderProvider, UpfXamlReaderProvider>()
                .AddSingleton<DynamicParser>();

            return collection.BuildServiceProvider();
        }

        public static DynamicParser Parser => services.GetRequiredService<DynamicParser>();
    }
}
