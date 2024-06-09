using System.ComponentModel;
using System.Windows.Controls;

namespace RemoteControlWPFClient.WpfLayer.ViewModels.Abstractions;

public interface IViewModel : INotifyPropertyChanged
{
}

public interface IViewModel<TControl> : IViewModel where TControl : ContentControl 
{
}