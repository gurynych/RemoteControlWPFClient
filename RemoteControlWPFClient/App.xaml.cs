using RemoteControlWPFClient.MVVM.IoC;
using System.Windows;

namespace RemoteControlWPFClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            IoC.Init();
            base.OnStartup(e);
        }
    }
}
