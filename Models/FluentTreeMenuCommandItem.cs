using System.Windows.Input;
using Wpf.Ui.Controls;

namespace FluentTreeMenu.Models;

public class FluentTreeMenuCommandItem : IDisposable
{
    public SymbolRegular Icon { get; }
    public ICommand Command { get; private set; }
    public object? CommandParameter { get; private set; }
    public string CommandName { get; }

    public FluentTreeMenuCommandItem(string header, SymbolRegular icon, ICommand command, object? parameter = null)
    {
        Icon = icon;
        CommandName = header;
        Command = command;
        CommandParameter = parameter;
    }

    public void Dispose()
    {
        Command = null;
        CommandParameter = null;
    }
}
