using FluentTreeMenu.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace FluentTreeMenu.ViewModels;

public class FluentTreeMenuViewModel : BaseViewModel, IDisposable
{
    public ObservableCollection<FluentTreeMenuColumn> Columns { get; } = [];
    public ObservableCollection<FluentTreeMenuList> Collections { get; } = [];

    public event Action<FluentTreeMenuBase, string> RenameItemAction;
    public event Action<string> RenameItemErrorAction;
    public event Action<FluentTreeMenuList, bool> Swapped;
    public event Action<FluentTreeMenuBase?> ChangeSelectedItem;

    public FluentTreeMenuBase? SelectedItem
    {
        get => field;
        set
        {
            if (!SetValue(ref field, value)) return;
            if (value == null)
            {
                SelectedHeader = string.Empty;
                SelectedPath = string.Empty;
                ChangeSelectedItem?.Invoke(value);
                return;
            }
            SelectedHeader = value.Header;
            SelectedPath = BuildPath(value);
            ChangeSelectedItem?.Invoke(value);
            OnPropertyChanged(nameof(IsSelectedItem));
        }
    }

    public bool IsSelectedItem => SelectedItem != null;

    public string SelectedHeader
    {
        get => field;
        set => SetValue(ref field, value);
    }

    public string SelectedPath
    {
        get => field;
        set => SetValue(ref field, value);
    }

    public FluentTreeMenuViewModel()
    {
        Columns.CollectionChanged += Columns_CollectionChanged;
        Collections.CollectionChanged += Collections_CollectionChanged;
    }


    public FluentTreeMenuBase FindRowToName(string name)
    {
        foreach (var row in Collections)
        {
            var find = FindRowToName(row, name);
            if (find != null)
                return find;
        }
        return null;
    }

    public void Swap(FluentTreeMenuBase drag, FluentTreeMenuBase swapped, Dictionary<FluentTreeMenuBase, Rect> selections, bool result = true)
    {
        var parent = drag.Parent;

        Swapped?.Invoke(parent, result);
    }

    public bool Rename(FluentTreeMenuBase rowViewModel, string oldName, string newName)
    {
        if (IsExistsName(newName))
        {
            RenameItemErrorAction?.Invoke(newName);
            return false;
        }

        rowViewModel.Header = newName;
        RenameItemAction?.Invoke(rowViewModel, oldName);
        return true;
    }

    private bool IsExistsName(string name)
    {
        foreach (var row in Collections)
        {
            if (IsExistsName(row, name))
                return true;
        }
        return false;
    }

    private static string BuildPath(FluentTreeMenuBase node)
    {
        var stack = new Stack<string>();
        var current = node;

        while (current is not null)
        {
            stack.Push(current.Header);
            current = current.Parent;
        }

        return string.Join(" / ", stack);
    }

    private bool IsExistsName(FluentTreeMenuBase row, string name)
    {
        if (row is FluentTreeMenuList list)
        {
            foreach (var child in list.Children)
            {
                if (IsExistsName(child, name)) return true;
            }
        }
        else if (row is FluentTreeMenuItem item)
        {
            if (item.Header == name) return true;
        }

        return false;
    }

    private FluentTreeMenuBase FindRowToName(FluentTreeMenuBase row, string name)
    {
        if (row.Header == name) return row;
        if (row is not FluentTreeMenuList list) return null;
        foreach (var child in list.Children)
        {
            var find = FindRowToName(child, name);
            if (find != null) return find;
        }
        return null;
    }

    private void Collections_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (var item in e.NewItems)
            {
                if(item is FluentTreeMenuBase itemBase)
                    itemBase.Owner = this;
            }
        }
        if (e.OldItems != null)
        {
            foreach (var item in e.OldItems)
            {
                if (item is FluentTreeMenuBase itemBase)
                    itemBase.Owner = null;
            }
        }
    }

    private void Columns_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        for (var i = 0; i < Columns.Count; i++)
        {
            if (i == 0)
            {
                Columns[0].IsMainColumn = true;
                continue;
            }
            Columns[i].IsMainColumn = false;
        }
    }

    public void Dispose()
    {
        Columns.CollectionChanged -= Columns_CollectionChanged;
        Collections.CollectionChanged -= Collections_CollectionChanged;
        Columns.Clear();
        foreach (var collection in Collections)
        {
            collection.Dispose();
        }
        Collections.Clear();
    }
}

