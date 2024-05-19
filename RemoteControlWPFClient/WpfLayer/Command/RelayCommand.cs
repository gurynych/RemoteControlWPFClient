using System;
using System.Windows.Input;

namespace RemoteControlWPFClient.WpfLayer.Command
{
    public class RelayCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        private readonly Action<object> command;
        private readonly Func<object, bool> canExecute;

        public RelayCommand(Action<object> command, Func<object, bool> canExecute = null)
        {
            this.command = command;
            this.canExecute = canExecute;
        }

        public RelayCommand(Action command, Func<bool> canExecute = null)
        {
            this.command = a => command.Invoke();

            if (canExecute != null)
            {
                this.canExecute = o => canExecute.Invoke();
            }
        }

        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            command?.Invoke(parameter);
        }
    }
}
