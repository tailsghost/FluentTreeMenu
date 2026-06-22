namespace FluentTreeMenu.Controls
{
    /// <summary>
    /// Логика взаимодействия для WindowItem.xaml
    /// </summary>
    public partial class WindowItem : IDisposable
    {
        public bool Disposable { get; private set; }
        public WindowItem()
        {
            InitializeComponent();
        }

        public void Dispose()
        {
            if (Disposable) return;
            Close();
            DataContext = null;
            Disposable = true;
        }
    }
}
