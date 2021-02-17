using System;
using Microsoft.Extensions.DependencyInjection;

namespace AppHost
{
    public static class AppServiceLocator
    {
        private static bool isConfigured;
        private static IServiceProvider internalServiceProvider;

        internal static void Configure(IServiceProvider serviceProvider)
        {
            if (isConfigured)
                throw new NotSupportedException("This AppServiceLocator is already configured");
                
            internalServiceProvider = serviceProvider;
            isConfigured = true;
        }

        public static T GetService<T>() => internalServiceProvider.GetRequiredService<T>();
    }
}