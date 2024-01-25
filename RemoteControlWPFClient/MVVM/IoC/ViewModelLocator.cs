using RemoteControlWPFClient.MVVM.ViewModels;

namespace RemoteControlWPFClient.MVVM.IoC
{
    public class ViewModelLocator
    {
        public MainViewModel MainViewModel => IoC.GetRequiredService<MainViewModel>();

        public AuthentificationViewModel AuthentificationViewModel => IoC.GetRequiredService<AuthentificationViewModel>();

        public StartupViewModel StartupViewModel => IoC.GetRequiredService<StartupViewModel>();

        public HomeViewModel HomeViewModel => IoC.GetRequiredService<HomeViewModel>(); 

        //public static ITcpClientService TcpClientService => IoC.GetRequiredService<TcpClientService>();
    }
}
