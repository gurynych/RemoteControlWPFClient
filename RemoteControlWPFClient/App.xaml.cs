using Microsoft.Win32;
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
            AutoStart();
		}

        private void AutoStart()
        {
			RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
			if (registryKey.GetValue("RemoteControlWPFClient") == null)
			{
				registryKey.SetValue("RemoteControlWPFClient", System.Reflection.Assembly.GetExecutingAssembly().Location);
			}
		}
    }
}
