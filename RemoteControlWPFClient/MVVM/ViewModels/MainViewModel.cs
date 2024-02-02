using CommunityToolkit.Mvvm.ComponentModel;
using RemoteControlWPFClient.BusinessLogic.Services;
using RemoteControlWPFClient.MVVM.Events;
using RemoteControlWPFClient.MVVM.IoC;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RemoteControlWPFClient.MVVM.ViewModels
{
    public partial class MainViewModel : ObservableValidator, ITransient
    {
        private readonly Client client;
        private readonly EventBus eventBus;

        [ObservableProperty]
        private UserControl userControl;

        public MainViewModel(Client client, EventBus eventBus)
        {
            this.client = client;
            this.eventBus = eventBus;
            eventBus?.Subscribe<ChangeUserControlEvent>(@event => ChangeUserControl(@event.UserControl));
        }

        /// <summary>
        /// Метод для смены UserControl
        /// </summary>
        /// <param name="newUserControl"></param>
        private Task ChangeUserControl(UserControl newUserControl)
        {
            UserControl = newUserControl;
            return Task.CompletedTask;
        }
    }
}