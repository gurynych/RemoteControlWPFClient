using Microsoft.Extensions.DependencyInjection;
using NetworkMessage.CommandFactory;
using NetworkMessage.Cryptography.AsymmetricCryptography;
using NetworkMessage.Cryptography.Hash;
using NetworkMessage.Cryptography.KeyStore;
using NetworkMessage.Cryptography.SymmetricCryptography;
using NetworkMessage.Windows;
using RemoteControlWPFClient.BusinessLogic.KeyStore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RemoteControlWPFClient.MVVM.IoC
{
    public static class IoC
    {
        private static IServiceProvider provider;

        public static void Init()
        {
            ServiceCollection services = new ServiceCollection();
            IEnumerable<Type> assemblyTypes = typeof(IoC).Assembly.GetTypes().Where(x => x.IsClass);
            foreach (Type type in assemblyTypes)
            {
                Type[] interfaces = type.GetInterfaces();
                if (interfaces.Contains(typeof(ISingleton)))
                {                    
                    services.AddSingleton(type);
                }
                else if (interfaces.Contains(typeof(ITransient)))
                {
                    services.AddTransient(type);
                }
            }

            services.AddSingleton<IAsymmetricCryptographer, RSACryptographer>();
            services.AddSingleton<ISymmetricCryptographer, AESCryptographer>();
            services.AddSingleton<IHashCreater, BCryptCreater>();
            services.AddTransient<ICommandFactory,WindowsCommandFactory>();
            services.AddSingleton<AsymmetricKeyStoreBase, ClientKeyStore>();

            provider = services.BuildServiceProvider();

            // Registration Singleton dependencies
            foreach (Type singltoneType in services.Where(x => x.Lifetime == ServiceLifetime.Singleton).Select(x => x.ServiceType))
            {
                _ = provider.GetRequiredService(singltoneType);
            }
        }

        public static T GetRequiredService<T>()
        {
            return provider.GetRequiredService<T>();
        }
    }

    public interface ISingleton
    {
    }

    public interface ITransient
    {
    }
}
