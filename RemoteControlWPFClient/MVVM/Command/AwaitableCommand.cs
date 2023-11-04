using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RemoteControlWPFClient.MVVM.Command
{
    public class AwaitableCommand : ICommand
    {
        private readonly Func<object, Task> command;
        private Func<object, bool> canExecuteCommand;
        private bool canExecute = true;

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }              

        public AwaitableCommand(Func<object, Task> command, Func<object, bool> canExecuteCommand = default)
        {
            this.command = command;
            this.canExecuteCommand = canExecuteCommand ?? (obj => canExecute);
        }

        public AwaitableCommand(Func<Task> command, Func<bool> canExecuteCommand = default)
        {
            this.command = obj => command.Invoke();
            if (canExecuteCommand == default) this.canExecuteCommand = obj => canExecute;
            else this.canExecuteCommand = obj => canExecuteCommand.Invoke();
        }

        public bool CanExecute(object parameter)
        {
            return canExecuteCommand(parameter);
        }

        public async void Execute(object parameter)
        {
            Func<object, bool> saveState = canExecuteCommand;
            canExecuteCommand = obj => false;
            CommandManager.InvalidateRequerySuggested();

            await command.Invoke(parameter);

            canExecuteCommand = saveState;
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
