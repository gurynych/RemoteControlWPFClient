using System.Threading.Tasks;

namespace RemoteControlWPFClient.WpfLayer.AttachedProperties
{
    public interface ILoadedAction
    {
        Task FrameworkElementLoaded();
    }
}
