using FluentTreeMenu.Helpers;
using FluentTreeMenu.Models;
using FluentTreeMenu.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using Wpf.Ui.Controls;
using MenuItem = System.Windows.Controls.MenuItem;
using TreeViewItem = System.Windows.Controls.TreeViewItem;

namespace FluentTreeMenu.Controls
{
    /// <summary>
    /// Логика взаимодействия для FluentTreeMenuControl.xaml
    /// </summary>
    public partial class FluentTreeMenuControl : UserControl, IDisposable
    {
        private bool _updatingWidths;

        public static readonly RoutedEvent ItemMouseLeftButtonDownEvent
            = EventManager.RegisterRoutedEvent(
                nameof(ItemMouseLeftButtonDown),
                RoutingStrategy.Bubble,
                typeof(MouseButtonEventHandler),
                typeof(FluentTreeMenuControl));

        public static readonly RoutedEvent ItemMouseRightButtonDownEvent
            = EventManager.RegisterRoutedEvent(
                nameof(ItemMouseRightButtonDown),
                RoutingStrategy.Bubble,
                typeof(MouseButtonEventHandler),
                typeof(FluentTreeMenuControl));

        public static readonly RoutedEvent ItemMouseLeftButtonUpEvent
            = EventManager.RegisterRoutedEvent(
                nameof(ItemMouseLeftButtonUp),
                RoutingStrategy.Bubble,
                typeof(MouseButtonEventHandler),
                typeof(FluentTreeMenuControl));

        public static readonly RoutedEvent ItemMouseMoveEvent =
            EventManager.RegisterRoutedEvent(
                nameof(ItemMouseMove),
                RoutingStrategy.Bubble,
                typeof(MouseEventHandler),
                typeof(FluentTreeMenuControl));

        public event MouseButtonEventHandler ItemMouseLeftButtonDown
        {
            add => AddHandler(ItemMouseLeftButtonDownEvent, value);
            remove => RemoveHandler(ItemMouseLeftButtonDownEvent, value);
        }

        public event MouseButtonEventHandler ItemMouseRightButtonDown
        {
            add => AddHandler(ItemMouseRightButtonDownEvent, value);
            remove => RemoveHandler(ItemMouseRightButtonDownEvent, value);
        }

        public event MouseButtonEventHandler ItemMouseLeftButtonUp
        {
            add => AddHandler(ItemMouseLeftButtonUpEvent, value);
            remove => RemoveHandler(ItemMouseLeftButtonUpEvent, value);
        }

        public event MouseEventHandler ItemMouseMove
        {
            add => AddHandler(ItemMouseMoveEvent, value);
            remove => RemoveHandler(ItemMouseMoveEvent, value);
        }

        public FluentTreeMenuViewModel ViewModel
        {
            get => (FluentTreeMenuViewModel)GetValue(ViewModelProperty);
            private set => SetValue(ViewModelProperty, value);
        }

        public static DependencyProperty ViewModelProperty
            = DependencyProperty.Register(
                nameof(ViewModel),
                typeof(FluentTreeMenuViewModel),
                typeof(FluentTreeMenuControl));

        public FluentTreeMenuControl(FluentTreeMenuViewModel viewModel)
        {
            DataContext = viewModel;
            ViewModel = viewModel;
            SetPropertyChanged();
            ViewModel.Columns.CollectionChanged += OnColumnsCollectionChanged;
            Loaded += FluentTreeMenuControl_Loaded;
            InitializeComponent();
            SizeChanged += OnSizeChanged;
        }

        private void SetPropertyChanged()
        {
            foreach (var items in ViewModel.Collections)
            {
                SetPropertyChanged(items);
            }
        }

        private void RemovePropertyChanged()
        {
            foreach (var items in ViewModel.Collections)
            {
                RemovePropertyChanged(items);
            }
        }

        private void RemovePropertyChanged(FluentTreeMenuBase element)
        {
            if (element is FluentTreeMenuList list)
            {

                if (list.IsMovingChildren)
                {
                    list.Children.CollectionChanged -= Children_CollectionChanged;
                }
                foreach (var child in list.Children)
                {
                    RemovePropertyChanged(child);
                }
            }

            if (element is FluentTreeMenuItem item)
            {
                if (item.Parent?.IsMovingChildren == true)
                {
                    item.PropertyChanged -= Row_PropertyChanged;
                }
            }
        }

        private void SetPropertyChanged(FluentTreeMenuBase element)
        {
            if (element is FluentTreeMenuList list)
            {

                if (list.IsMovingChildren)
                {
                    list.Children.CollectionChanged += Children_CollectionChanged;
                }
                foreach (var child in list.Children)
                {
                    SetPropertyChanged(child);
                }
            }

            if (element is FluentTreeMenuItem item)
            {
                if (item.Parent?.IsMovingChildren == true)
                {
                    item.PropertyChanged += Row_PropertyChanged;
                }
            }
        }

        private void Children_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is FluentTreeMenuBase itemMenu)
                        itemMenu.PropertyChanged += Row_PropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is FluentTreeMenuBase itemMenu)
                        itemMenu.PropertyChanged -= Row_PropertyChanged;
                }
            }
        }

        private WindowItem _window;

        private void Row_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not FluentTreeMenuBase item) return;
            if (e.PropertyName is nameof(FluentTreeMenuBase.IsPlaceHolder))
            {
                if (item.IsPlaceHolder)
                {
                    if (_window != null) throw new Exception("Окно уже существует!");

                    _window = new WindowItem()
                    {
                        DataContext = item
                    };

                    var mouse = Mouse.GetPosition(Application.Current.MainWindow);
                    _window.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    _window.Arrange(new Rect(0, 0, _window.DesiredSize.Width, _window.DesiredSize.Height));
                    var width = _window.DesiredSize.Width;
                    var height = _window.DesiredSize.Height;
                    _window.Left = mouse.X - width / 2;
                    _window.Top = mouse.Y - height / 2;
                    _window.Show();
                    StartDrag(item);
                }
                else
                {
                    if (_window == null) throw new Exception("Окно не существует!");
                    CancelSwap();
                    _window.Dispose();
                    _isDragging = false;
                    _window = null;
                }
            }
        }

        private bool _isDragging;
        private FluentTreeMenuBase _lastHoveredItem;
        private FluentTreeMenuBase? _lastHoveredItemChanged;
        private Point _startWindow;
        private Point _mouseOffset;
        private Dictionary<FluentTreeMenuBase, Rect> _selections = [];
        private List<FluentTreeMenuBase> _original;

        private void StartDrag(FluentTreeMenuBase item)
        {
            _window.DataContext = item;
            _isDragging = true;
            _lastHoveredItem = null;

            var mouse = Mouse.GetPosition(Application.Current.MainWindow);

            _startWindow = new Point(_window.Left, _window.Top);

            _mouseOffset = new Point(
                mouse.X - _window.Left,
                mouse.Y - _window.Top
            );
            _original = item.Parent.Children.ToList();
            RebuildSelections(item);
        }

        private void RebuildSelections(FluentTreeMenuBase draggedItem)
        {
            _selections.Clear();

            if (draggedItem.Parent == null)
                return;

            foreach (var child in draggedItem.Parent.Children)
            {
                if (ReferenceEquals(child, draggedItem))
                    continue;

                var container = GetTreeViewItem(PART_TreeView, child);

               var res = FindChild<Border>(container, "UiElement");

                var topLeft = container.TranslatePoint(new Point(0, 0), Application.Current.MainWindow);

                _selections[child] = new Rect(
                    topLeft.X,
                    topLeft.Y,
                    container.ActualWidth,
                    container.ActualHeight);
            }

        }

        private void UpdateDrag()
        {
            if (!_isDragging)
                return;

            var draggedItem = _window.DataContext as FluentTreeMenuBase;
            if (draggedItem == null)
                return;

            var mouse = Mouse.GetPosition(Application.Current.MainWindow);

            _window.Left = mouse.X - _mouseOffset.X;
            _window.Top = mouse.Y - _mouseOffset.Y;

            var hovered = FindNearestSelection(mouse, draggedItem);

            if (hovered == null)
            {
                _lastHoveredItem = null;
                return;
            }

            if (_lastHoveredItem != null && ReferenceEquals(_lastHoveredItem, hovered))
                return;

            SwapRow(hovered, draggedItem);
            Application.Current.MainWindow.UpdateLayout();
            RebuildSelections(draggedItem);

            _lastHoveredItem = hovered;
            _lastHoveredItemChanged = hovered;
        }

        private void SwapRow(FluentTreeMenuBase swapped, FluentTreeMenuBase draggedItem)
        {
            var collection = draggedItem.Parent.Children;

            int dragIndex = collection.IndexOf(draggedItem);
            int swappedIndex = collection.IndexOf(swapped);

            if (dragIndex < 0 || swappedIndex < 0 || dragIndex == swappedIndex)
                return;

            if (dragIndex < swappedIndex)
            {
                collection.Move(dragIndex, swappedIndex);
                collection.Move(swappedIndex - 1, dragIndex);
            }
            else
            {
                collection.Move(dragIndex, swappedIndex);
                collection.Move(swappedIndex + 1, dragIndex);
            }
        }

        private FluentTreeMenuBase? FindNearestSelection(Point mousePoint, FluentTreeMenuBase draggedItem)
        {
            if (_selections == null || _selections.Count == 0)
                return null;

            foreach (var selection in _selections)
            {
                if (ReferenceEquals(selection.Key, draggedItem))
                    continue;

                if (selection.Value.Contains(mousePoint))
                    return selection.Key;
            }

            return null;
        }

        private void CancelSwap()
        {
            if (!_isDragging) return;

            var draggedItem = _window.DataContext as FluentTreeMenuBase;
            if (draggedItem == null)
                throw new Exception("Потерян контекст!");

            if (ReferenceEquals(_lastHoveredItemChanged, draggedItem))
                return;

            var container = GetTreeViewItem(PART_TreeView, draggedItem);

            var element = FindChild<Border>(container, "UiElement");
            var mouse = Mouse.GetPosition(Application.Current.MainWindow);
            var topLeft = element.TranslatePoint(new Point(0, 0), Application.Current.MainWindow);
            var rect = new Rect(
                topLeft.X,
                topLeft.Y,
                element.ActualWidth,
                element.ActualHeight
            );

            if (rect.Contains(mouse) && _lastHoveredItemChanged != null)
            {
                ViewModel.Swap(draggedItem, _lastHoveredItemChanged, _selections);

                _isDragging = false;
                _lastHoveredItemChanged = null;
                return;
            }

            for (int targetIndex = 0; targetIndex < _original.Count; targetIndex++)
            {
                var item = _original[targetIndex];

                var currentIndex = item.Parent.Children.IndexOf(item);

                if (currentIndex != targetIndex)
                {
                    item.Parent.Children.Move(currentIndex, targetIndex);
                }
            }
            ViewModel.Swap(draggedItem, _lastHoveredItemChanged, _selections,false);
            _isDragging = false;
        }


        public FluentTreeMenuControl()
        {
            DataContextChanged += FluentTreeMenuControl_DataContextChanged;
            InitializeComponent();
        }

        public static readonly DependencyProperty ItemElementTemplateSelectorProperty =
            DependencyProperty.Register(
                nameof(ItemElementTemplateSelector),
                typeof(DataTemplateSelector),
                typeof(FluentTreeMenuControl),
                new PropertyMetadata(null));

        public DataTemplateSelector? ItemElementTemplateSelector
        {
            get => (DataTemplateSelector?)GetValue(ItemElementTemplateSelectorProperty);
            set => SetValue(ItemElementTemplateSelectorProperty, value);
        }

        private void OnSizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            UpdateColumnWidths();
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            if (ScrollViewer is null)
                return;

            const double step = 48.0;

            if (Keyboard.IsKeyDown(Key.LeftCtrl) ||
                Keyboard.IsKeyDown(Key.RightCtrl))
            {
                var offset = ScrollViewer.HorizontalOffset -
                             Math.Sign(e.Delta) * step;

                ScrollViewer.ScrollToHorizontalOffset(
                    Math.Clamp(offset, 0, ScrollViewer.ScrollableWidth));
            }
            else
            {
                var offset = ScrollViewer.VerticalOffset -
                             Math.Sign(e.Delta) * step;

                ScrollViewer.ScrollToVerticalOffset(
                    Math.Clamp(offset, 0, ScrollViewer.ScrollableHeight));
            }

            e.Handled = true;
        }

        private void FluentTreeMenuControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            FluentTreeMenuLayoutHelper.Build(ViewModel.Collections);
        }

        private void FluentTreeMenuControl_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (sender is not FluentTreeMenuControl { DataContext: FluentTreeMenuViewModel vm }) return;
            ViewModel = vm;
            ViewModel.Columns.CollectionChanged += OnColumnsCollectionChanged;
            SetPropertyChanged();
            UpdateColumnWidths();
            FluentTreeMenuControl_Loaded(null, null);
            SizeChanged += OnSizeChanged;
        }

        private void OnColumnsCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems is not null)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is FluentTreeMenuColumn column)
                    {
                        column.PropertyChanged -= OnColumnPropertyChanged;
                    }
                }
            }

            if (e.NewItems is null) return;
            {
                foreach (var item in e.NewItems)
                {
                    if (item is FluentTreeMenuColumn column)
                    {
                        column.PropertyChanged += OnColumnPropertyChanged;
                    }
                }
            }
        }

        private void OnColumnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(FluentTreeMenuColumn.Width)
                or nameof(FluentTreeMenuColumn.MinWidth)
                or nameof(FluentTreeMenuColumn.MaxWidth)
                or nameof(FluentTreeMenuColumn.IsMainColumn))
            {
                UpdateColumnWidths();
            }
        }

        private void UpdateColumnWidths()
        {
            if (_updatingWidths || ViewModel.Columns.Count == 0)
                return;

            _updatingWidths = true;
            try
            {
                var availableWidth = ActualWidth;
                if (double.IsNaN(availableWidth) || double.IsInfinity(availableWidth) || availableWidth <= 0d)
                    availableWidth = 0d;

                var fixedTotal = 0d;
                var starTotal = 0d;

                for (var i = 0; i < ViewModel.Columns.Count; i++)
                {
                    var column = ViewModel.Columns[i];
                    var resolved = ResolveNaturalWidth(column);
                    if (column.Width.IsStar)
                        starTotal += column.Width.Value;
                    else
                        fixedTotal += resolved;
                }

                var remaining = availableWidth > 0d ? Math.Max(0d, availableWidth - fixedTotal) : 0d;

                for (var i = 0; i < ViewModel.Columns.Count; i++)
                {
                    var column = ViewModel.Columns[i];
                    var width = ResolveNaturalWidth(column);

                    if (column.Width.IsStar)
                    {
                        if (starTotal > 0d && remaining > 0d)
                            width = remaining * (column.Width.Value / starTotal);
                        else
                            width = Math.Max(column.MinWidth, 120d);
                    }

                    width = Math.Max(column.MinWidth, width);
                    if (!double.IsPositiveInfinity(column.MaxWidth))
                        width = Math.Min(column.MaxWidth, width);

                    column.ActualWidth = width;
                }
            }
            finally
            {
                _updatingWidths = false;
            }
        }

        private static double ResolveNaturalWidth(FluentTreeMenuColumn column)
        {
            if (column.Width.IsAbsolute)
                return column.Width.Value;

            if (column.Width.IsAuto)
                return Math.Max(column.MinWidth, 120d);

            return Math.Max(column.MinWidth, 120d);
        }

        public void Dispose()
        {
            ViewModel.Columns.CollectionChanged -= OnColumnsCollectionChanged;
            RemovePropertyChanged();
            ViewModel = null;
        }

        private void ColumnResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (sender is not Thumb { DataContext: FluentTreeMenuColumn { CanResize: true } column }) return;

            var currentWidth = column.Width.IsAbsolute ? column.Width.Value : column.ActualWidth;
            var newWidth = currentWidth + e.HorizontalChange;

            newWidth = Math.Max(column.MinWidth, newWidth);
            if (!double.IsPositiveInfinity(column.MaxWidth))
                newWidth = Math.Min(column.MaxWidth, newWidth);

            column.Width = new GridLength(newWidth, GridUnitType.Pixel);
            UpdateColumnWidths();
        }

        private void UIElement_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not SymbolIcon icon)
                return;

            if (icon.Tag is not FluentTreeMenuBase item || item.Commands.Count == 0)
                return;

            var menu = new ContextMenu
            {
                PlacementTarget = icon,
                StaysOpen = false
            };

            foreach (var commandItem in item.Commands)
            {
                menu.Items.Add(new MenuItem
                {
                    Header = commandItem.CommandName,
                    Command = commandItem.Command,
                    CommandParameter = commandItem.CommandParameter
                });
            }

            menu.IsOpen = true;

            e.Handled = true;
        }

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            RaiseEvent(new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, e.ChangedButton)
            {
                RoutedEvent = ItemMouseLeftButtonDownEvent,
                Source = e.Source
            });
        }

        private void UIElement_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            RaiseEvent(new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, e.ChangedButton)
            {
                RoutedEvent = ItemMouseRightButtonDownEvent,
                Source = e.Source
            });
            e.Handled = true;
        }

        private void UIElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            RaiseEvent(new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, e.ChangedButton)
            {
                RoutedEvent = ItemMouseLeftButtonUpEvent,
                Source = e.Source
            });
            e.Handled = true;
        }

        private void UIElement_OnMouseMove(object sender, MouseEventArgs e)
        {
            RaiseEvent(new MouseEventArgs(e.MouseDevice, e.Timestamp)
            {
                RoutedEvent = ItemMouseMoveEvent,
                Source = e.Source
            });
        }

        private void PART_TreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is FluentTreeMenuBase selectedItem)
            {
                ViewModel.SelectedItem = selectedItem;
            }
        }

        private void Fluent_OnMouseMove(object sender, MouseEventArgs e)
        {
            UpdateDrag();
        }

        private TreeViewItem? GetTreeViewItem(
            ItemsControl parent,
            object item)
        {
            var container =
                parent.ItemContainerGenerator.ContainerFromItem(item)
                    as TreeViewItem;

            if (container != null)
                return container;

            foreach (var obj in parent.Items)
            {
                var childContainer =
                    parent.ItemContainerGenerator.ContainerFromItem(obj)
                        as TreeViewItem;

                if (childContainer == null)
                    continue;

                container = GetTreeViewItem(childContainer, item);

                if (container != null)
                    return container;
            }

            return null;
        }

        public static T? FindChild<T>(DependencyObject parent, string? name = null)
            where T : FrameworkElement
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T element &&
                    (name == null || element.Name == name))
                {
                    return element;
                }

                var result = FindChild<T>(child, name);
                if (result != null)
                    return result;
            }

            return null;
        }
    }
}
