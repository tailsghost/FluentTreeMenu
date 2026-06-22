using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace FluentTreeMenu.Helpers;

public static class DependencyHelper
{
    public static T? FindDescendant<T>(DependencyObject root) where T : DependencyObject
    {
        var count = VisualTreeHelper.GetChildrenCount(root);
        for (var i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T typed)
                return typed;

            var descendant = FindDescendant<T>(child);
            if (descendant is not null)
                return descendant;
        }
        return null;
    }
}

