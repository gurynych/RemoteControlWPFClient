using System;
using System.Threading.Tasks;

namespace RemoteControlWPFClient.WpfLayer.AttachedProperties
{
    public class DelegateLoadedAction : ILoadedAction
    {
        public Func<Task> LoadedAction { get; set; }

        public DelegateLoadedAction(Func<Task> loadedAction)
        {
            LoadedAction = loadedAction;
        }

        public async Task FrameworkElementLoaded()
        {
            await LoadedAction?.Invoke();
        }
    }
}
