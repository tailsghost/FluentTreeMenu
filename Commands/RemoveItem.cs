using FluentTreeMenu.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluentTreeMenu.Commands;

public class RemoveItem<T> : RelayCommand<T>
{
    public RemoveItem(string commandName) : base(commandName)
    {
        Init(Execute);
    }

    private void Execute(T item)
    {
        if (item is not FluentTreeMenuBase row) return;
        OnRemoveItem(row);
    }


    private void OnRemoveItem(FluentTreeMenuBase row)
    {
        if (row is FluentTreeMenuList rowList)
        {
            if (rowList.Parent is not null and FluentTreeMenuList parentList)
            {
                parentList.Children.Remove(rowList);
            }
            var count = rowList.Children.Count;
            for (int i = 0; i < count; i++)
            {
                var child = rowList.Children[0];
                rowList.Children.Remove(child);
                OnRemoveItem(child);
            }
        }
        else if (row is FluentTreeMenuItem item)
        {
            if (item.Parent is not null and FluentTreeMenuList parentList)
            {
                parentList.Children.Remove(item);
            }
        }
    }
}

