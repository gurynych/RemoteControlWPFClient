using System;

namespace RemoteControlWPFClient.WpfLayer.Events
{
    public class EventSubscriber : IDisposable
    {
        private readonly Action<EventSubscriber> disponseAct;

        public Type EventType { get; set; }

        public EventSubscriber(Type eventType, Action<EventSubscriber> disponseAct)
        {
            EventType = eventType;
            this.disponseAct = disponseAct;
        }

        public void Dispose()
        {
            disponseAct?.Invoke(this);
        }
    }
}