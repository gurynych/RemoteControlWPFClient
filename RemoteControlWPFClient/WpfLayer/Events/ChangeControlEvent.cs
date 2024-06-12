using System;
using System.Windows.Controls;

namespace RemoteControlWPFClient.WpfLayer.Events
{
    public class ChangeControlEvent : IEvent
    {
        public object Sender { get; }
        public Control NewControl { get; }
        public bool ClearHistory { get; }
        public bool AddToHistory { get; }

        public ChangeControlEvent(object sender, Control newControl, bool clearHistory, bool addToHistory)
        {
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
            NewControl = newControl ?? throw new ArgumentNullException(nameof(newControl));
            ClearHistory = clearHistory;
            AddToHistory = addToHistory;
        }
    }
}
