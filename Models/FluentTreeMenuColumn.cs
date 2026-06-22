using FluentTreeMenu.ViewModels;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows;

namespace FluentTreeMenu.Models;

public class FluentTreeMenuColumn : BaseViewModel
{
    public string Header
    {
        get => field;
        set => SetValue(ref field, value);
    } = string.Empty;

    public GridLength Width
    {
        get => field;
        set => SetValue(ref field, value);
    } = new GridLength(1, GridUnitType.Star);

    public ObservableCollection<FluentTreeMenuCommandItem> Commands { get; } = [];

    public double ActualWidth
    {
        get => field;
        set => SetValue(ref field, value);
    }

    public bool IsLast
    {
        get => field;
        set => SetValue(ref field, value);
    }

    public string? BindingPath
    {
        get => field;
        set => SetValue(ref field, value);
    }

    public Brush? Foreground
    {
        get => field;
        set => SetValue(ref field, value);
    }

    public double MinWidth
    {
        get => field;
        set => SetValue(ref field, value);
    }

    public double MaxWidth
    {
        get => field;
        set => SetValue(ref field, value);
    } = double.MaxValue;

    public bool IsMainColumn
    {
        get => field;
        set => SetValue(ref field, value);
    }

    public bool CanResize
    {
        get => field;
        set => SetValue(ref field, value);
    } = true;
}

