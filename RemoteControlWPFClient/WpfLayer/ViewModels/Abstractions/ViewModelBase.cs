using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RemoteControlWPFClient.WpfLayer.ViewModels.Abstractions;

public abstract class ViewModelBase<TControl> : ObservableObject, IViewModel<TControl>
    where TControl : ContentControl
{
}

public abstract class ValidatableViewModelBase<TControl> : ObservableValidator, IViewModel<TControl>
    where TControl : ContentControl
{
}