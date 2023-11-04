using DevExpress.Mvvm.POCO;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RemoteControlWPFClient.Views.UserControls.Authentification
{
    /// <summary>
    /// Логика взаимодействия для AuthorizationUC.xaml
    /// </summary>
    public partial class AuthorizationUC : UserControl
    {
        private bool isFirstPassEntry = true;

        public AuthorizationUC()
        {
            InitializeComponent();
        }

        private void authPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            if (isFirstPassEntry)
            {
                isFirstPassEntry = false;
                BindingOperations.ClearBinding(authPassword, PasswordBoxAssist.PasswordProperty);
                Binding binding = new Binding();
                binding.Source = DataContext;
                binding.Path = new PropertyPath("AuthPassword");
                binding.Mode = BindingMode.TwoWay;
                binding.ValidatesOnDataErrors = true;
                binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                authPassword.SetBinding(PasswordBoxAssist.PasswordProperty, binding);                
                authPassword.GotFocus -= authPassword_GotFocus;
            }
        }
    }
}
