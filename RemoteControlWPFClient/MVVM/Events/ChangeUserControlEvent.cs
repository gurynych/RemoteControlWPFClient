using System;
using System.Windows.Controls;

namespace RemoteControlWPFClient.MVVM.Events
{
    public class ChangeUserControlEvent : IEvent
    {
        public UserControl UserControl { get; private set; }

        public ChangeUserControlEvent(UserControl userControl)
        {         
            UserControl = userControl ?? throw new ArgumentNullException(nameof(userControl));
        }
    }
}
