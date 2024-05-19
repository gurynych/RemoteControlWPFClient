using Microsoft.Win32;
using RemoteControlWPFClient.WpfLayer.IoC;
using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using RemoteControlWPFClient.WpfLayer.Views.Windows;
using RemoteControlWPFClient.WpfLayer.ViewModels;

namespace RemoteControlWPFClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                AutoStart();
                IoCContainer.Init();
                base.OnStartup(e);
                IoCContainer.OpenViewModel<StartupViewModel, StartupWindow>().Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Непредвиденная ошибка.\n{ex.Message}\n{ex.StackTrace}", "Ошибка", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public string ApplicationName => "g-eye";
        
        public ResourceDictionary ThemeDictionary => Resources.MergedDictionaries[0];
        
        public void ChangeTheme(Uri uri)
        {
            ThemeDictionary.MergedDictionaries.Clear();
            ThemeDictionary.MergedDictionaries.Add(new ResourceDictionary() { Source = uri });
        } 
        
        private void AutoStart()
        {
            RegistryKey registryKey =
                Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            string directory = Directory.GetCurrentDirectory();
            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            string path = Path.Combine(directory, assemblyName + ".exe");
            
            if (registryKey.GetValue(ApplicationName) == null)
            {
                registryKey.SetValue(ApplicationName, path);
            }
        }
    }
}