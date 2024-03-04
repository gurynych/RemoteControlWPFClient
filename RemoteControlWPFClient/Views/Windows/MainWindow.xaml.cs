using System;
using System.ComponentModel;
using System.Windows;

namespace RemoteControlWPFClient.Views.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
            ni.Text = "RemoteControl";
            ni.Icon = new System.Drawing.Icon("Main.ico");
            ni.Visible = true;          
            ni.DoubleClick +=
				delegate (object sender, EventArgs args)
				{
					App.Current.Shutdown();
				};
			ni.Click +=
                delegate (object sender, EventArgs args)
                {
					Show();
					WindowState = WindowState.Normal;
                };
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            base.OnClosing(e);
        }
    }
}
