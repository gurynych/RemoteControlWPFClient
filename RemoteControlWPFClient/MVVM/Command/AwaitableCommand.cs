using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RemoteControlWPFClient.MVVM.Command
{
    public class AwaitableCommand : ICommand
    {
        private readonly Func<object, Task> command;

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool IsExecuting { get; set; }

        public AwaitableCommand(Func<object, Task> command)
        {
            this.command = command;
        }

        public AwaitableCommand(Func<Task> command)
        {
            this.command = obj => command.Invoke();
        }

        public bool CanExecute(object parameter)
        {
            return !IsExecuting;
        }

        public async void Execute(object parameter)
        {
            IsExecuting = true;
            CommandManager.InvalidateRequerySuggested();

            await command.Invoke(parameter);

            IsExecuting = false;
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
