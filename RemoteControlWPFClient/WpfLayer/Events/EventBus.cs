using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RemoteControlWPFClient.WpfLayer.IoC;

namespace RemoteControlWPFClient.WpfLayer.Events
{
    public class EventBus : ISingleton
    {
        private readonly ConcurrentDictionary<EventSubscriber, Func<IEvent, Task>> subscribers;

        public EventBus()
        {
            subscribers = new ConcurrentDictionary<EventSubscriber, Func<IEvent, Task>>();
        }        

        public IDisposable Subscribe<TEvent>(Func<TEvent, Task> action)
            where TEvent : IEvent
        {
            EventSubscriber subscriber = new EventSubscriber(typeof(TEvent), e => subscribers.TryRemove(e, out var _));
            subscribers.TryAdd(subscriber, e => action.Invoke((TEvent)e));
            return subscriber;
        }

        public async Task Publish<TEvent>(TEvent @event)
            where TEvent : IEvent
        {
            IEnumerable<Task> tasks = subscribers
                .Where(x => x.Key.EventType.Equals(typeof(TEvent)))
                .Select(x => x.Value(@event));
            await Task.WhenAll(tasks);
        }
    }
}
