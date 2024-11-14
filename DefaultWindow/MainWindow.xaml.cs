using CustomWindow.CornerWindow;
using Microsoft.Win32;
using System.Windows;

namespace CustomWindow.DefaultWindow
{
    public partial class MainWindow : Window
    {
        private double[] _normalWorkArea;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            _normalWorkArea = new double[2];
            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
            StateChanged += Window_StateChanged;
            LocationChanged += Window_LocationChanged;
        }

        /// <summary>
        /// Первично вызывает метод SetMaximizedWindowSizeWithoutTaskbarOverlap
        /// </summary>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            SetMaximizedWindowSizeWithoutTaskbarOverlap();
        }

        /// <summary>
        /// Срабатывает при закрытии окна. Отписывается от событий
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            SystemEvents.DisplaySettingsChanged -= SystemEvents_DisplaySettingsChanged;
            StateChanged -= Window_StateChanged;
            LocationChanged -= Window_LocationChanged;
            base.OnClosed(e);
        }

        /// <summary>
        /// Срабатывает при изменении настроек монитора, реагирует на подключение нового монитора
        /// Вызывает метод SetMaximizedWindowSizeWithoutTaskbarOverlap
        /// </summary>
        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            SetMaximizedWindowSizeWithoutTaskbarOverlap();
        }

        /// <summary>
        /// Срабатывает при изменении расположения окна на мониторе
        /// Вызывает метод SetMaximizedWindowSizeWithoutTaskbarOverlap
        /// </summary>
        private void Window_LocationChanged(object sender, EventArgs e)
        {
            SetMaximizedWindowSizeWithoutTaskbarOverlap();
        }

        /// <summary>
        /// Устанавливает максимальные размеры окна опираясь на значения рабочей области
        /// </summary>
        private void SetMaximizedWindowSizeWithoutTaskbarOverlap()
        {
            // По умолчанию окно с WindowStyle = None при переходе в полноэкранный режим съедается монитором слева и сверху на 7 пикселей.
            // Чтобы избежать такой ситуации в главном контейнере окна устанавливаем Margin="7" и здесь плюсуем 14.
            // При этом значение свойства ResizeBorderThickness у WindowChrome = 14.
            // Если окно принудительно имеет скруглённые края будет необходим обработчик для Border'ов убирающий скругления в полноэкранном режиме.
            MaxWidth = SystemParameters.WorkArea.Width + 14;
            MaxHeight = SystemParameters.WorkArea.Height + 14;
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
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }

        /// <summary>
        /// Закрывает окно
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Реагирует на изменение свойства WindowState. Корректирует переход состояний Maximized | Normal
        /// </summary>
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                _normalWorkArea[0] = ActualWidth;
                _normalWorkArea[1] = ActualHeight;
            }
            else if (WindowState == WindowState.Normal)
            {
                Width = _normalWorkArea[0];
                Height = _normalWorkArea[1];
            }
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
                    this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                }
                else
                {
                    // Если состояния окна - полноэкранное
                    if (WindowState == WindowState.Maximized)
                    {
                        // Получаем текущую позицию курсора в координатах экрана
                        var mousePosition = PointToScreen(e.GetPosition(this));

                        // Переходим в состояние Normal
                        this.WindowState = WindowState.Normal;

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