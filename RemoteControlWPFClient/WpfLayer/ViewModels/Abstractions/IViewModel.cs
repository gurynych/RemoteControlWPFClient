using System.Windows.Controls;

namespace RemoteControlWPFClient.WpfLayer.ViewModels.Abstractions;

public interface IViewModel
{
}

public interface IViewModel<TControl> : IViewModel where TControl : ContentControl 
{
}