using RemoteControlServer.BusinessLogic.Services;
using RemoteControlWPFClient.MVVM.IoC;

namespace RemoteControlWPFClient.MVVM.ViewModels
{
    public class MainViewModel : BaseViewModel, ITransient
    {
        private readonly Сlient tcpClient;

        public string Email { get; set; }

        public string Password { get; set; }

        public MainViewModel(Сlient tcpClient)
        {
            //this.tcpClient = tcpClient;
        }
    }    
}
