using FluentTreeMenu.ViewModels;
using Wpf.Ui.Controls;

namespace FluentTreeMenu.Models;

public abstract class FluentTreeMenuItemElement : BaseViewModel, IDisposable
{
    public string Key { get; set; } = string.Empty;

    public string Text
    {
        get => field;
        set => SetValue(ref field, value);
    }

    public object?  VisualContent { get; protected set; }

    public SymbolRegular Icon
    {
        get => field;
        set => SetValue(ref field, value);
    }

    public bool IsRenameMode
    {
        get => field;
        set => SetValue(ref field, value);
    }

    public bool IsRename { get; set; } = false;

    public virtual void Dispose()
    {
        VisualContent = null;
    }
}

