using FluentTreeMenu.ViewModels;

namespace FluentTreeMenu.Commands;

public class CloseAllNodesCommand<T> : RelayCommand<T>
{
    public CloseAllNodesCommand() : base("Закрыть все узлы")
    {
        Init(Execute);
    }

    private void Execute(T item)
    {
        if (item is FluentTreeMenuList list)
        {
            OnCloseAllNodes(list);
        }
    }

    private void OnCloseAllNodes(FluentTreeMenuList rows)
    {
        if (rows == null) return;
        foreach (var rowViewModel in rows.Children)
        {
            if (rowViewModel is not FluentTreeMenuList rowViewModelList) continue;
            if (rowViewModelList.Children.Count == 0) continue;
            if (rowViewModelList.IsExpanded)
            {
                rowViewModelList.IsExpanded = false;
                rowViewModelList.CloseAllNodes(rowViewModelList);
            }
        }
        rows.IsExpanded = false;
    }
}

