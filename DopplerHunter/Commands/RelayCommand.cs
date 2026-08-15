using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace DopplerHunter.Commands
{
    public class RelayCommand(Action<object> execute, Predicate<object>? canExecute = null) : ICommand
    {
        private readonly Action<object> _execute = execute;
        private readonly Predicate<object>? _canExecute = canExecute;

        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute(parameter!);
        public void Execute(object? parameter) => _execute(parameter!);
        public event EventHandler? CanExecuteChanged 
        { 
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
