using FluentTreeMenu.Models;
using Wpf.Ui.Controls;

namespace FluentTreeMenu.ViewModels;

public class FluentTreeMenuItem : FluentTreeMenuBase
{
    public FluentTreeMenuItem(string header, SymbolRegular icon, bool isBlocked = false, bool isUnique = false) : base(header, icon, isBlocked, isUnique)
    {
    }

    public override IEnumerable<FluentTreeMenuCommandItem> GenerateCommands(Func<FluentTreeMenuCommandItem, bool> predicate)
    {
        return Commands;
    }
}

