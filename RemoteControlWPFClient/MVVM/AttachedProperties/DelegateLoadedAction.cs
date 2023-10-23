using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControlWPFClient.MVVM.AttachedProperties
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
