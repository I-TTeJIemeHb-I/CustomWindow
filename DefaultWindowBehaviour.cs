using Microsoft.Win32;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace CustomWindow
{
    /// <summary>
    /// Класс, переопределяющий поведение кастомного окна - окно со свойством WindowStyle="None".
    /// Реализует корректный выход/вход полноэкранного режима.
    /// Управляется через свойство MyState (игнорирует значение WindowState.Minimized).
    /// Требуется ссылка на окно, а также простые реализации команд окна, которые будут влиять на свойство MyState.
    /// 
    /// # Если вы изменяете настройки панели задач во время работы приложения, то корректным размер полноэкранного режима будет только при второй попытке (не нашёл событие, которое бы отслеживало изменение настроек панели задач)
    /// # На моём компьютере при включённом параметре Windows "Автоматически скрывать панель задач в режиме рабочего стола" панель не выдвигается. 
    ///   Если в Grid окна задать отступ снизу > 1, то панель начнёт выдвигаться, но окно приложения будет её перекрывать (как будто Z индекс окна выше индекса панели задач)
    /// # У меня нет двух мониторов с разным разрешением чтобы проверить кооректность изменения параметров в maximizeRect
    /// </summary>
    public class DefaultWindowBehaviour : INotifyPropertyChanged, IDisposable
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion


        private Window myWindow;
        private WindowState myState;
        private double[] maximizeRect;        // Описывает размеры окна в полноэкранном режиме (Left, Top, Width, Height)
        private double[] normalRect;         // Описывает размеры окна в нормальном режиме (Left, Top, Width, Height)
        private bool haveUnusualSituation;  // Сигнализирует о нестандартном переходе окна в полноэкранный режим (перетягивание вверх)

        /// <summary>
        /// Текущее состояние окна (отличается от реального состояния)
        /// </summary>
        public WindowState MyState
        {
            get => myState;
            set
            {
                if (myState != value && value != WindowState.Minimized)
                {
                    myState = value;
                    OnPropertyChanged(nameof(MyState));

                    if (!haveUnusualSituation)
                        SwitchingBetweenFullscreenAndNormal();
                }
            }
        }


        public DefaultWindowBehaviour(Window window)
        {
            myWindow = window;
            myState = window.WindowState;
            maximizeRect = new double[4];
            normalRect = new double[4];

            SetMaximizedWindowSizeWithoutTaskbarOverlap();
            window.StateChanged += Window_StateChanged;
            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
        }


        #region События

        /// <summary>
        /// Реагирует на изменение WindowState ОКНА. 
        /// myWindow.WindowState == WindowState.Maximized сообщает о том, что переход к полноэкранному режиму был выполнен путём перетаскивания окна вверх.
        /// А проверка MyState == WindowState.Normal && myWindow.WindowState == WindowState.Maximized в методе SwitchingBetweenFullscreenAndNormal это подтверждает
        /// </summary>
        private void Window_StateChanged(object? sender, EventArgs e)
        {
            if (myWindow.WindowState == WindowState.Maximized)
            {
                haveUnusualSituation = true;
                SwitchingBetweenFullscreenAndNormal();
            }
        }

        /// <summary>
        /// Реагирует на изменение настроек монитора
        /// </summary>
        private void SystemEvents_DisplaySettingsChanged(object? sender, EventArgs e)
        {
            SetMaximizedWindowSizeWithoutTaskbarOverlap();
        }

        #endregion


        #region Вспомогательные методы

        /// <summary>
        /// Устанавливает максимальные размеры окна опираясь на значения рабочей области
        /// </summary>
        private void SetMaximizedWindowSizeWithoutTaskbarOverlap()
        {
            Rect rect = SystemParameters.WorkArea;
            
            maximizeRect[0] = rect.Left;
            maximizeRect[1] = rect.Top;
            maximizeRect[2] = rect.Width;
            maximizeRect[3] = rect.Height;
        }

        /// <summary>
        /// Изменяет размеры окна на максимально доступные
        /// </summary>
        private void ChangeWindowSizeToMaximize()
        {
            SetNormalWindowSize();

            myWindow.Left = maximizeRect[0];
            myWindow.Top = maximizeRect[1];
            myWindow.Width = maximizeRect[2];
            myWindow.Height = maximizeRect[3];

            myWindow.ResizeMode = ResizeMode.NoResize;
        }

        /// <summary>
        /// Устанавливает нормальные размеры окна опираясь на текущие значения
        /// </summary>
        private void SetNormalWindowSize()
        {
            normalRect[0] = myWindow.Left;
            normalRect[1] = myWindow.Top;
            normalRect[2] = myWindow.Width;
            normalRect[3] = myWindow.Height;
        }

        /// <summary>
        /// Изменяет размеры окна на нормальные
        /// </summary>
        private void ChangeWindowSizeToNormal()
        {
            SetMaximizedWindowSizeWithoutTaskbarOverlap();

            myWindow.Left = normalRect[0];
            myWindow.Top = normalRect[1];
            myWindow.Width = normalRect[2];
            myWindow.Height = normalRect[3];

            myWindow.ResizeMode = ResizeMode.CanResize;
        }

        #endregion


        // Переключение между полноэкранным и нормальным режимом отображения окна за счёт изменений размеров
        public void SwitchingBetweenFullscreenAndNormal()
        {
            if (MyState == WindowState.Normal && myWindow.WindowState == WindowState.Maximized)
            {
                myWindow.WindowState = WindowState.Normal;
                MyState = WindowState.Maximized;

                ChangeWindowSizeToMaximize();

                haveUnusualSituation = false;
            }
            else if (MyState == WindowState.Normal)
                ChangeWindowSizeToNormal();
            else if (MyState == WindowState.Maximized)
                ChangeWindowSizeToMaximize();
        }


        #region IDisposable

        public void Dispose()
        {
            myWindow.StateChanged -= Window_StateChanged;

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
