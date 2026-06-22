using FluentTreeMenu.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluentTreeMenu.Commands;

public class OpenAllNodesCommand<T> : RelayCommand<T>
{
    public OpenAllNodesCommand() : base("Открыть все узлы")
    {
        Init(Execute);
    }

    private void Execute(T item)
    {
        if (item is not FluentTreeMenuList list) return;
        foreach (var rowViewModel in list.Children)
        {
            if (rowViewModel is not FluentTreeMenuList rowViewModelList) continue;
            if (rowViewModelList.Children.Count == 0) continue;
            if (!rowViewModelList.IsExpanded)
            {
                rowViewModelList.IsExpanded = true;
                rowViewModelList.OpenAllNodes(rowViewModelList);
            }
        }
        list.IsExpanded = true;
    }
}

