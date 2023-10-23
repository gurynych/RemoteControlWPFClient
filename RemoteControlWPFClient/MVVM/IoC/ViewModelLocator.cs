using RemoteControlWPFClient.MVVM.ViewModels;

namespace RemoteControlWPFClient.MVVM.IoC
{
    public class ViewModelLocator
    {
        public MainViewModel MainViewModel => IoC.GetRequiredService<MainViewModel>();

        //public static ITcpClientService TcpClientService => IoC.GetRequiredService<TcpClientService>();
    }
}
