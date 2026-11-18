using OpenTK.Platform.Windows;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace ProductMonitor.OperateCommand
{
    public class BaseCommands : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        private readonly Action<object?> _execute;

        // 构造时保存委托；点击执行命令时才调用。兼容原来的无参数命令。
        public BaseCommands(Action action)
        {
            ArgumentNullException.ThrowIfNull(action);
            _execute = _ => action();
        }

        // 接收 XAML 的 CommandParameter，例如当前点击的车间对象。
        public BaseCommands(Action<object?> action)
        {
            ArgumentNullException.ThrowIfNull(action);
            _execute = action;
        }
        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            if (_execute!=null)
            {
                _execute.Invoke(parameter);
            }
        }
    }
}
