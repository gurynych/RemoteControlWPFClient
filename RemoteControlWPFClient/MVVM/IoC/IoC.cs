using Microsoft.Extensions.DependencyInjection;
using RemoteControlServer.BusinessLogic.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControlWPFClient.MVVM.IoC
{
    public static class IoC
    {
        private static IServiceProvider provider;

        public static void Init()
        {
            ServiceCollection services = new ServiceCollection();
            Type[] assemblyTypes = typeof(IoC).Assembly.GetTypes();
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

            provider = services.BuildServiceProvider();

            // Регистрируем ISingltone зависимости
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
