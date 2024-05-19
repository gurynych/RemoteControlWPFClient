using System.Windows;

namespace RemoteControlWPFClient.WpfLayer.AttachedProperties
{
    public class LoadedBindings : DependencyObject
    {
        #region LoadedEnabledProperty
        public static readonly DependencyProperty LoadedEnabledProperty =
            DependencyProperty.RegisterAttached("LoadedEnabled",
                typeof(bool),
                typeof(LoadedBindings),
                new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnLoadedEnabledPropertyChanged)));

        public static bool GetLoadedEnabled(DependencyObject sender) => 
            (bool)sender.GetValue(LoadedEnabledProperty);
        public static void SetLoadedEnabled(DependencyObject sender, bool value) =>
            sender.SetValue(LoadedEnabledProperty, value);
        #endregion

        #region LoadedActionProperty
        public static readonly DependencyProperty LoadedActionProperty =
            DependencyProperty.RegisterAttached("LoadedAction",
                typeof(ILoadedAction),
                typeof(LoadedBindings),
                new FrameworkPropertyMetadata(null));

        public static ILoadedAction GetLoadedAction(DependencyObject sender) => 
            (ILoadedAction)sender.GetValue(LoadedActionProperty);
        public static void SetLoadedAction(DependencyObject sender, ILoadedAction value) => 
            sender.SetValue(LoadedActionProperty, value);
        #endregion

        public static void OnLoadedEnabledPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                bool newValue = (bool)e.NewValue;
                bool oldValue = (bool)e.OldValue;

                //Если было зарегестрированно, а сейчас отписка => надо отписаться
                if (oldValue && !newValue)
                {                    
                    element.Loaded -= LoadedEvent;
                }                
                else if (!oldValue && newValue)
                {
                    element.Loaded += LoadedEvent;
                }
            }
        }

        public static async void LoadedEvent(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                ILoadedAction loadedAction = GetLoadedAction(element);
                if (loadedAction != null)
                {
                    await loadedAction.FrameworkElementLoaded();
                }
            }
        }
    }
}
