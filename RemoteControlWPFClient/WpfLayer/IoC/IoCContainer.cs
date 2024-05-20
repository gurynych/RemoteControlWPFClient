using Microsoft.Extensions.DependencyInjection;
using NetworkMessage.CommandFactory;
using NetworkMessage.Communicator;
using NetworkMessage.Cryptography.AsymmetricCryptography;
using NetworkMessage.Cryptography.Hash;
using NetworkMessage.Cryptography.KeyStore;
using NetworkMessage.Cryptography.SymmetricCryptography;
using NetworkMessage.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Windows.Controls;
using RemoteControlWPFClient.BusinessLayer.KeyStore;
using RemoteControlWPFClient.BusinessLayer.Services;
using RemoteControlWPFClient.WpfLayer.ViewModels.Abstractions;

namespace RemoteControlWPFClient.WpfLayer.IoC
{
    public static class IoCContainer
    {
        private static IServiceProvider provider;

        public static void Init()
        {
            ServiceCollection services = new ServiceCollection();
            IEnumerable<Type> assemblyTypes = typeof(IoCContainer).Assembly.GetTypes().Where(x => x.IsClass);
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
            services.AddSingleton<ICommandFactory, WindowsCommandFactory>();
            services.AddSingleton<TcpCryptoClientCommunicator, Client>();
            services.AddSingleton<AsymmetricKeyStoreBase, ClientKeyStore>();
            services.AddSingleton<CommandsRecipientService>();
            services.AddSingleton<CurrentUserServices>();

            provider = services.BuildServiceProvider();

            // Registration Singleton dependencies
            foreach (Type singltoneType in services.Where(x => x.Lifetime == ServiceLifetime.Singleton).Select(x => x.ServiceType))
            {
                _ = provider.GetRequiredService(singltoneType);
            }
        }

        public static T GetRequiredService<T>() where T : notnull
        {
            return provider.GetRequiredService<T>();
        }

        public static TControl OpenViewModel<TViewModel, TControl>()
            where TViewModel : IViewModel<TControl>
            where TControl : ContentControl
        {
            try
            {
                TViewModel viewModel = ActivatorUtilities.CreateInstance<TViewModel>(provider);
                TControl control = typeof(TControl).GetConstructor(Type.EmptyTypes)?.Invoke(null) as TControl;
                control!.DataContext = viewModel;
                return control;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        
        public static TControl OpenViewModel<TViewModel, TControl>(params object[] parameters)
            where TViewModel : IViewModel<TControl>
            where TControl : ContentControl
        {
            try
            {
                TViewModel viewModel = ActivatorUtilities.CreateInstance<TViewModel>(provider, parameters);
                TControl control = typeof(TControl).GetConstructor(Type.EmptyTypes)?.Invoke(null) as TControl;
                control!.DataContext = viewModel;
                return control;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }

    public interface ISingleton
    {
    }

    public interface ITransient
    {
    }
}
