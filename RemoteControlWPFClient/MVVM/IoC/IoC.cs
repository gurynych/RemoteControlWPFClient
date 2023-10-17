﻿using Microsoft.Extensions.DependencyInjection;
using NetworkMessage.Cryptography;
using NetworkMessage.Cryptography.KeyStore;
using RemoteControlServer.BusinessLogic.Services;
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
            List<Type> singltoneTypes = new List<Type>();
            foreach (Type type in assemblyTypes)
            {
                Type[] interfaces = type.GetInterfaces();
                if (interfaces.Contains(typeof(ISingleton)))
                {
                    singltoneTypes.Add(type);
                    services.AddSingleton(type);
                }
                else if (interfaces.Contains(typeof(ITransient)))
                {
                    services.AddTransient(type);
                }
            }

            services.AddSingleton<IAsymmetricCryptographer, RSACryptographer>();
            services.AddSingleton<IHashCreater, BCryptCreater>();
            services.AddSingleton<AsymmetricKeyStoreBase, ClientKeyStore>();
            singltoneTypes.AddRange(new[] { typeof(IAsymmetricCryptographer), typeof(IHashCreater) });

            provider = services.BuildServiceProvider();

            // Registration Singleton dependencies
            foreach (Type singltoneType in singltoneTypes)
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