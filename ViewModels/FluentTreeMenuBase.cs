using FluentTreeMenu.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Xml.Linq;
using Wpf.Ui.Controls;

namespace FluentTreeMenu.ViewModels;

public abstract class FluentTreeMenuBase : BaseViewModel, IDisposable
{
    private const double _connectorWidth = 10d;

    public FluentTreeMenuViewModel Owner { get; set; }

    public string Header
    {
        get => field;
        set => SetValue(ref field, value);
    }

    public Guid Id
    {
        get => field;
        set => SetValue(ref field, value);
    } = Guid.NewGuid();

    public string Description
    {
        get => field; 
        set => SetValue(ref field, value);
    }

    public SymbolRegular Icon
    {
        get => field;
        set => SetValue(ref field, value);
    }

    public SymbolRegular TrailingIcon
    {
        get => field;
        set => SetValue(ref field, value);
    }

    public ICommand TrailingCommand { get; set; }

    public bool IsSelected
    {
        get => field;
        set => SetValue(ref field, value);
    }

    public int Depth
    {
        get => field;
        set => SetValue(ref field, value);
    }

    public bool IsExpanded
    {
        get => field;
        set => SetValue(ref field, value);
    }

    public bool IsRenameMode
    {
        get => field;
        set => SetValue(ref field, value);
    }

    public bool IsPlaceHolder
    {
        get => field;
        set => SetValue(ref field, value);
    }
    public double ConnectorLineLength => HasParent ? _connectorWidth : 0d;

    public bool HasParent => Parent != null;

    public ObservableCollection<bool> ConnectorPath { get; } = [];

    public FluentTreeMenuList? Parent
    {
        get => field;
        set
        {
            if (!SetValue(ref field, value)) return;
            OnPropertyChanged(nameof(HasParent));
            OnPropertyChanged(nameof(ConnectorLineLength));
        }
    }

    public FluentTreeMenuList? TopParent
    {
        get => field;
        set => SetValue(ref field, value);
    }

    public bool IsBlocked { get; }

    public bool IsEnable
    {
        get => field;
        set => SetValue(ref field, value);
    } = true;

    public bool IsBlockedItem
    {
        get => field;
        set => SetValue(ref field, value);
    } = false;

    public bool IsLast
    {
        get => field;
        set => SetValue(ref field, value);
    }

    public ObservableCollection<FluentTreeMenuCommandItem> Commands { get; } = [];

    public ObservableCollection<FluentTreeMenuItemElement> ItemElements { get; } = [];

    public abstract IEnumerable<FluentTreeMenuCommandItem> GenerateCommands(Func<FluentTreeMenuCommandItem, bool> predicate);

    public bool HasCommands => Commands.Count > 0;

    public override string ToString() => Header;
    protected FluentTreeMenuBase(string header, SymbolRegular icon, bool isBlocked = false)
    {
        Header = header;
        Icon = icon;
        IsBlocked = isBlocked;
        Commands.CollectionChanged += Commands_CollectionChanged;
    }

    public void UpdateTitle(string newTitle)
    {
        Header = newTitle;
        OnPropertyChanged(nameof(Header));
    }

    private void Commands_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasCommands));
    }

    public FluentTreeMenuItemElement? GetElement(string key)
        => ItemElements.FirstOrDefault(x => x.Key == key);

    public void SetElement(string key, FluentTreeMenuItemElement element, SymbolRegular? icon = null)
    {
        var existing = GetElement(key);
        if (existing != null)
            ItemElements.Remove(existing);

        element.Key = key;
        if(icon != null)
            element.Icon = icon.Value;
        ItemElements.Add(element);
    }

    public void AddCommand(string commandName, SymbolRegular icon, ICommand command, object? parameter = null)
    {
        Commands.Add(new FluentTreeMenuCommandItem(commandName, icon, command, parameter));
    }

    internal void SetConnectorPath(IEnumerable<bool> path)
    {
        ConnectorPath.Clear();
        foreach (var i in path)
        {
            ConnectorPath.Add(i);
        }
        OnPropertyChanged(nameof(ConnectorPath));
        OnPropertyChanged(nameof(Depth));
        OnPropertyChanged(nameof(ConnectorLineLength));
    }


    public virtual void Dispose()
    {
        Commands.CollectionChanged -= Commands_CollectionChanged;
        Owner = null;
        foreach (var item in ItemElements)
        {
            item.Dispose();
        }
        ItemElements.Clear();
        foreach (var command in Commands)
        {
            command.Dispose();
        }
        Commands.Clear();
        Parent = null;
        TrailingCommand = null;
    }
}

