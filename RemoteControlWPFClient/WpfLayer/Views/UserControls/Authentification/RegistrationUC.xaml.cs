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

namespace RemoteControlWPFClient.WpfLayer.Views.UserControls.Authentification
{
    /// <summary>
    /// Логика взаимодействия для RegistrationUC.xaml
    /// </summary>
    public partial class RegistrationUC : UserControl
    {
        private bool isFirstPassEntry = true;
        public RegistrationUC()
        {
            InitializeComponent();
        }

        private void regPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            if (isFirstPassEntry)
            {
                isFirstPassEntry = false;
                BindingOperations.ClearBinding(regPassword, PasswordBoxAssist.PasswordProperty);
                Binding binding = new Binding();
                binding.Source = DataContext;
                binding.Path = new PropertyPath("RegPassword");
                binding.Mode = BindingMode.TwoWay;
                binding.ValidatesOnDataErrors = true;
                binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                regPassword.SetBinding(PasswordBoxAssist.PasswordProperty, binding);
                regPassword.GotFocus -= regPassword_GotFocus;
            }
        }
    }
}
