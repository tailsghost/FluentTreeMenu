using FluentTreeMenu.ViewModels;

namespace FluentTreeMenu.Helpers;

public class FluentTreeMenuLayoutHelper
{
    public static void Build(IEnumerable<FluentTreeMenuBase>? roots)
    {
        if(roots is null) return;
        FluentTreeMenuBase last = null;
        if (roots.Any())
            last = roots.Last();
        foreach (var item in roots)
        {
            BuildNode(item, [], null,item == last);
        }
    }


    private static void BuildNode(FluentTreeMenuBase node, IList<bool> path, FluentTreeMenuList parent, bool isLast)
    {
        node.Parent = parent;
        node.IsLast = isLast;
        node.Depth = path.Count;
        node.SetConnectorPath(path);

        if(node is not FluentTreeMenuList list) return;
        if(list.Children.Count == 0) return;

        var nextPath = new List<bool>(path.Count + 1);
        nextPath.AddRange(path);
        nextPath.Add(!isLast);
        var last = list.Children.Last();
        foreach (var i in list.Children)
        {
            BuildNode(
                i,
                nextPath,
                list,
                i == last);
        }
    }
}
