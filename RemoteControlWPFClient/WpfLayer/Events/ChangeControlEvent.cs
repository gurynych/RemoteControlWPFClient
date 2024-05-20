using System;
using System.Windows.Controls;

namespace RemoteControlWPFClient.WpfLayer.Events
{
    public class ChangeControlEvent : IEvent
    {
        public Control NewControl { get; }
        public bool ClearHistory { get; }

        public ChangeControlEvent(Control newControl, bool clearHistory)
        {
            NewControl = newControl ?? throw new ArgumentNullException(nameof(newControl));
            ClearHistory = clearHistory;
        }
    }
}
