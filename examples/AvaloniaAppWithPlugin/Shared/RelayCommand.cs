using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Shared
{
    [Serializable]
    public abstract class CommandBase : NotifyBase, ICommand
    {
        public event EventHandler CanExecuteChanged;

        protected CommandBase(bool isEnabled, bool isAsynchronous)
        {
            IsEnabled = isEnabled;
            IsAsynchronous = isAsynchronous;
        }

        #region properties

        public bool IsEnabled
        {
            get => Get<bool>();
            set
            {
                if (Set(value))
                    RaiseCanExecuteChanged();
            }
        }

        public bool IsExecuting
        {
            get => Get<bool>();
            protected set
            {
                if (Set(value))
                    RaiseCanExecuteChanged();
            }
        }

        public bool IsAsynchronous { get; set; }

        #endregion

        private void RaiseCanExecuteChanged()
        {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler.Invoke(this, EventArgs.Empty);
        }

        public bool CanExecute(object parameter)
        {
            return IsEnabled && !IsExecuting;
        }

        public abstract void Execute(object parameter);
    }

    [Serializable]
    public class RelayCommand : CommandBase
    {
        readonly Action _action;

        public RelayCommand(Action action, bool isEnabled = true, bool isAsynchronous = false) : base(isEnabled, isAsynchronous)
        {
            _action = action;
        }

        public async override void Execute(object parameter)
        {
            if (!CanExecute(parameter))
                return;

            IsExecuting = true;

            if (IsAsynchronous)
                await Task.Run(_action);
            else
                _action.Invoke();

            IsExecuting = false;
        }
    }

    [Serializable]
    public class RelayCommand<P> : CommandBase
    {
        readonly Action<P> _action;

        public RelayCommand(Action<P> action, bool isEnabled = true, bool isAsynchronous = false) : base(isEnabled, isAsynchronous)
        {
            _action = action;
        }

        public async override void Execute(object parameter)
        {
            if (!CanExecute(parameter))
                return;

            IsExecuting = true;

            if (IsAsynchronous)
            {
                var argument = (P)parameter;
                await Task.Run(() => _action.Invoke(argument));
            }
            else
                _action.Invoke((P)parameter);

            IsExecuting = false;
        }
    }
}