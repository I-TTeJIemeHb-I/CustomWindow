using CustomWindow.CornerWindow;
using System.Windows;

namespace CustomWindow.DefaultWindow
{
    public partial class MainWindow : Window
    {
        public DefaultWindowBehaviour behaviour { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            behaviour = new DefaultWindowBehaviour(this);
        }

        /// <summary>
        /// Сворачивает окно
        /// </summary>
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Изменяет размер окна (нормальный/полноэкранный)
        /// </summary>
        private void MaximizeRestoreButton_Click(object sender, RoutedEventArgs e)
        {
            // Меняем состояние окна между полноэкранным и нормальным
            behaviour.MyState = behaviour.MyState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        /// <summary>
        /// Закрывает окно
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Реализует перетаскивание окна, смена WindowState по двойному щелчку ЛКМ, выход из полноэкранного режима при операции перетаскивания
        /// </summary>
        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Если нажата ЛКМ (Левая Кнопка Мыши)
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                // Если был выполнен двойной клик
                if (e.ClickCount == 2)
                {
                    // Меняем состояние окна между полноэкранным и нормальным
                    behaviour.MyState = behaviour.MyState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                }
                else
                {
                    // Если состояния окна - полноэкранное
                    if (behaviour.MyState == WindowState.Maximized)
                    {
                        // Получаем текущую позицию курсора в координатах экрана
                        var mousePosition = PointToScreen(e.GetPosition(this));

                        // Переходим в состояние Normal
                        behaviour.MyState = WindowState.Normal;

                        // Задаем новое положение окна, учитывая положение курсора
                        this.Left = mousePosition.X - (this.RestoreBounds.Width / 2);
                        this.Top = mousePosition.Y - 10; // Чуть выше, чтобы курсор остался в заголовке
                    }

                    // Запускаем перетаскивание окна
                    this.DragMove();
                }
            }
        }


        private void OpenWindowWithCorners_Click(object sender, RoutedEventArgs e)
        {
            MainWindowCorner window = new MainWindowCorner();
            window.Show();
        }
    }
}