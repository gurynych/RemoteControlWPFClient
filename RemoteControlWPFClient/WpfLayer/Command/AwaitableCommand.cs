using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RemoteControlWPFClient.WpfLayer.Command
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
            this.canExecuteCommand = canExecuteCommand ?? (_ => canExecute);
        }

        public AwaitableCommand(Func<Task> command, Func<bool> canExecuteCommand = default)
        {
            this.command = _ => command.Invoke();
            if (canExecuteCommand == default) this.canExecuteCommand = _ => canExecute;
            else this.canExecuteCommand = _ => canExecuteCommand.Invoke();
        }

        public virtual bool CanExecute(object parameter)
        {
            return canExecuteCommand(parameter);
        }

        public virtual async void Execute(object parameter)
        {
            Func<object, bool> saveState = canExecuteCommand;
            canExecuteCommand = _ => false;
            CommandManager.InvalidateRequerySuggested();

            await command.Invoke(parameter);

            canExecuteCommand = saveState;
            CommandManager.InvalidateRequerySuggested();
        }
    }
    
    public class AwaitableCommand<TParameter> : AwaitableCommand
    {
        public AwaitableCommand(Func<TParameter, Task> command, Func<TParameter, bool> canExecuteCommand = default) :
            base(obj => command.Invoke((TParameter)obj), canExecuteCommand == default ? default : obj => canExecuteCommand.Invoke((TParameter)obj))
        {
        }

        public AwaitableCommand(Func<Task> command, Func<bool> canExecuteCommand = default) : base(command, canExecuteCommand)
        {
        }
    }
}
