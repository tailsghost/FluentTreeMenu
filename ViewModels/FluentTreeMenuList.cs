using FluentTreeMenu.Models;
using System.Collections.ObjectModel;
using Wpf.Ui.Controls;

namespace FluentTreeMenu.ViewModels;

public class FluentTreeMenuList : FluentTreeMenuBase
{
    public ObservableCollection<FluentTreeMenuBase> Children { get; } = [];
    public bool HasChildren 
        => Children.Count > 0;

    public bool IsExpanded
    {
        get => field;
        set => SetValue(ref field, value);
    }

    public bool IsMovingChildren
    {
        get => field;
        set => SetValue(ref field, value);
    } = false;

    public FluentTreeMenuBase SelectedItem
    {
        get => field;
        set => SetValue(ref field, value);
    }

    public FluentTreeMenuList(string header, SymbolRegular icon, bool isBlocked = false) : base(header, icon, isBlocked)
    {
        Children.CollectionChanged += Children_CollectionChanged;
    }

    public FluentTreeMenuList AddChildrenList(RowHolder holder)
    {
        var row = new FluentTreeMenuList(holder.Title, holder.Symbol);
        AddChild(row, holder);
        return row;
    }

    public FluentTreeMenuList AddChildrenList(FluentTreeMenuList children)
    {
        Children.Add(children);
        children.Owner = Owner;
        return children;
    }

    public FluentTreeMenuItem AddChildrenItem(RowHolder holder)
    {
        var row = new FluentTreeMenuItem(holder.Title, holder.Symbol);
        AddChild(row, holder);
        return row;
    }

    public FluentTreeMenuItem AddChildrenItem(FluentTreeMenuItem children)
    {
        Children.Add(children);
        children.Owner = Owner;
        return children;
    }

    public FluentTreeMenuList AddChildrenListInsert(RowHolder holder, int index)
    {
        var row = new FluentTreeMenuList(holder.Title, holder.Symbol);
        AddChildInsert(row, holder, index);
        return row;
    }

    public FluentTreeMenuList AddChildrenListInsert(FluentTreeMenuList children, int index)
    {
        Children.Insert(index, children);
        children.Owner = Owner;
        return children;
    }

    public FluentTreeMenuItem AddChildrenItemInsert(RowHolder holder, int index)
    {
        var row = new FluentTreeMenuItem(holder.Title, holder.Symbol);
        AddChildInsert(row, holder, index);
        return row;
    }


    public void CloseAllNodes(FluentTreeMenuList row)
    {
        foreach (var child in row.Children)
        {
            if (child is not FluentTreeMenuList rowList) continue;
            if (rowList.IsExpanded)
            {
                rowList.IsExpanded = false;
                CloseAllNodes(rowList);
            }
        }
    }

    public void OpenAllNodes(FluentTreeMenuList row)
    {
        foreach (var child in row.Children)
        {
            if (child is not FluentTreeMenuList rowList) continue;
            if (!rowList.IsExpanded)
            {
                rowList.IsExpanded = true;
                OpenAllNodes(rowList);
            }
        }
    }

    private FluentTreeMenuBase AddChild(FluentTreeMenuBase child, RowHolder holder)
    {
        foreach (var command in holder.Commands)
        {
            child.Commands.Add(command);
        }
        child.Owner = Owner;
        child.Parent = this;
        Children.Add(child);
        return child;
    }

    private FluentTreeMenuBase AddChildInsert(FluentTreeMenuBase child, RowHolder holder, int index)
    {
        child.Owner = Owner;
        child.Parent = this;
        Children.Insert(index,child);
        return child;
    }

    private void Children_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (var item in e.OldItems)
            {
                if (item is FluentTreeMenuBase node)
                {
                    node.Parent = null;
                }
            }
        }

        if (e.NewItems is not null)
        {
            foreach (var item in e.NewItems)
            {
                if (item is FluentTreeMenuBase node)
                {
                    node.Parent = this;
                }
            }
        }

        OnPropertyChanged(nameof(HasChildren));
    }

    public override IEnumerable<FluentTreeMenuCommandItem> GenerateCommands(Func<FluentTreeMenuCommandItem, bool> predicate)
    {
        var newCommands = new List<FluentTreeMenuCommandItem>();
        foreach (var command in Commands)
        {
            if (predicate(command)) continue;
            newCommands.Add(command);
        }

        return newCommands;
    }

    public override void Dispose()
    {
        Children.CollectionChanged -= Children_CollectionChanged;
        base.Dispose();
    }
}
