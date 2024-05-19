using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using MaterialDesignThemes.Wpf;
using Application = System.Windows.Application;

namespace RemoteControlWPFClient.WpfLayer.Views.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            string applicationName = (Application.Current as App)!.ApplicationName;
            Title = applicationName;
            NotifyIcon ni = new NotifyIcon();
            ni.Icon = new System.Drawing.Icon("Main.ico");
            ni.Visible = true;
            ni.Click +=
                delegate
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                };
            
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            ToolStripItem menuItem = new ToolStripButton($"Закрыть {applicationName}")
            {
                
            };
            menuItem.Click += OnExitMenuClick;
            
            contextMenu.Items.Add(menuItem);
            ni.ContextMenuStrip = contextMenu;
            contextMenu.PerformLayout();
        }

        private void OnExitMenuClick(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(Application.Current.Shutdown);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            base.OnClosing(e);
        }
    }
}
