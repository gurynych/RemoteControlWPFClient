using Microsoft.Win32;
using RemoteControlWPFClient.MVVM.IoC;
using System;
using System.IO;
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
            string directory = Directory.GetCurrentDirectory();
			string path = directory.Substring(0,directory.Length)+"\\"+ System.Reflection.Assembly.GetExecutingAssembly().GetName().Name+".exe";
			if (registryKey.GetValue("RemoteControlWPFClient") == null)
			{
				registryKey.SetValue("RemoteControlWPFClient", path);
			}
		}
    }
}
