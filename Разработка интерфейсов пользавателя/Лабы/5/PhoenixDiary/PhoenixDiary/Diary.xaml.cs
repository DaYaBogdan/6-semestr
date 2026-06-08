using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace PhoenixDiary
{
    public partial class Diary : Window, INotifyPropertyChanged
    {
        // Модели данных
        public class TaskItem : INotifyPropertyChanged
        {
            private bool _isSelected;
            public int Id { get; set; }
            public string ClientName { get; set; }
            public string ServiceName { get; set; }
            public DateTime DateTime { get; set; }
            public string TimeRange => $"{DateTime:HH:mm}";
            public int MasterId { get; set; }

            public bool IsSelected
            {
                get => _isSelected;
                set { _isSelected = value; OnPropertyChanged(); }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string name = null) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public class DayModel
        {
            public DateTime Date { get; set; }
            public string WeekdayName { get; set; }
            public string FormattedDate => $"{Date:dd.MM}";
            public ObservableCollection<TaskItem> Tasks { get; set; } = new ObservableCollection<TaskItem>();
            public bool IsToday { get; set; }
        }

        // Свойства
        private DateTime _referenceDate = DateTime.Today;
        private ObservableCollection<DayModel> _weekDays = new ObservableCollection<DayModel>();

        public ObservableCollection<DayModel> WeekDays
        {
            get => _weekDays;
            set { _weekDays = value; OnPropertyChanged(); }
        }

        private int _selectedCount;
        public int SelectedCount
        {
            get => _selectedCount;
            set { _selectedCount = value; OnPropertyChanged(); OnPropertyChanged(nameof(SelectedCountText)); }
        }

        public string SelectedCountText => $"Выбрано: {SelectedCount}";

        public Diary()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadTasks();
            await LoadClients();
            RefreshWeekDays();
        }

        private async System.Threading.Tasks.Task LoadTasks()
        {
            // TODO: Загрузка из API / БД
            // Пример:
            // var tasks = await Api.GetTasks();
            // Сейчас просто заглушка
            var mockTasks = new[]
            {
                new TaskItem { Id = 1, ClientName = "Анна Иванова", ServiceName = "Маникюр + гель-лак", DateTime = DateTime.Today.AddHours(10), MasterId = 1 },
                new TaskItem { Id = 2, ClientName = "Мария Петрова", ServiceName = "Педикюр", DateTime = DateTime.Today.AddHours(14), MasterId = 1 },
            };

            // Сохраняем в глобальную коллекцию (можно через статический класс или сервис)
            GlobalData.AllTasks = new ObservableCollection<TaskItem>(mockTasks);
        }

        private async System.Threading.Tasks.Task LoadClients()
        {
            // TODO: Загрузка клиентов
        }

        private void RefreshWeekDays()
        {
            WeekDays.Clear();

            // Получаем понедельник текущей недели
            var monday = GetMonday(_referenceDate);

            for (int i = 0; i < 7; i++)
            {
                var currentDay = monday.AddDays(i);
                var tasksForDay = GlobalData.AllTasks?.Where(t => t.DateTime.Date == currentDay.Date).ToList() ?? new List<TaskItem>();

                var dayModel = new DayModel
                {
                    Date = currentDay,
                    WeekdayName = GetWeekdayName(currentDay.DayOfWeek),
                    IsToday = currentDay.Date == DateTime.Today
                };

                foreach (var task in tasksForDay)
                    dayModel.Tasks.Add(task);

                WeekDays.Add(dayModel);
            }

            UpdateSelectedCount();
        }

        private DateTime GetMonday(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-diff).Date;
        }

        private string GetWeekdayName(DayOfWeek dow)
        {
            return dow switch
            {
                DayOfWeek.Monday => "Понедельник",
                DayOfWeek.Tuesday => "Вторник",
                DayOfWeek.Wednesday => "Среда",
                DayOfWeek.Thursday => "Четверг",
                DayOfWeek.Friday => "Пятница",
                DayOfWeek.Saturday => "Суббота",
                DayOfWeek.Sunday => "Воскресенье",
                _ => ""
            };
        }

        private void UpdateSelectedCount()
        {
            SelectedCount = GlobalData.AllTasks?.Count(t => t.IsSelected) ?? 0;
            //DeleteButton.IsEnabled = SelectedCount > 0;
        }

        private void OpenAddModal(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddTaskWindow();
            addWindow.Owner = this;
            if (addWindow.ShowDialog() == true)
            {
                // Обновляем список
                _ = LoadTasks();
                RefreshWeekDays();
            }
        }

        private async void DeleteSelectedTasks(object sender, RoutedEventArgs e)
        {
            if (SelectedCount == 0) return;

            var result = MessageBox.Show($"Удалить выбранные записи ({SelectedCount})?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var toDelete = GlobalData.AllTasks.Where(t => t.IsSelected).ToList();
                foreach (var task in toDelete)
                {
                    // TODO: API DELETE
                    GlobalData.AllTasks.Remove(task);
                }
                RefreshWeekDays();
            }
        }

        private void PrevWeek(object sender, RoutedEventArgs e)
        {
            _referenceDate = _referenceDate.AddDays(-7);
            RefreshWeekDays();
        }

        private void NextWeek(object sender, RoutedEventArgs e)
        {
            _referenceDate = _referenceDate.AddDays(7);
            RefreshWeekDays();
        }

        private void ResetToToday(object sender, RoutedEventArgs e)
        {
            _referenceDate = DateTime.Today;
            RefreshWeekDays();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // Навигация по Sidebar
        private void Sidebar_DiaryClick(object sender, RoutedEventArgs e)
        {
            // Уже на главной странице - просто обновляем
            RefreshWeekDays();
        }

        private void Sidebar_ClientsClick(object sender, RoutedEventArgs e)
        {
            var clientsWindow = new ClientsWindow();
            clientsWindow.Owner = this;
            clientsWindow.ShowDialog();

            // Обновляем данные на главной странице, если нужно
            RefreshWeekDays();
        }

        private void Sidebar_MastersClick(object sender, RoutedEventArgs e)
        {
            // Проверка прав доступа
            if (GlobalData.CurrentUser?.Role != "admin")
            {
                MessageBox.Show("Доступно только администраторам", "Доступ запрещён",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var mastersWindow = new MastersWindow();
            mastersWindow.Owner = Window.GetWindow(this);
            mastersWindow.ShowDialog();
        }

        private void Sidebar_AllTasksClick(object sender, RoutedEventArgs e)
        {
            if (GlobalData.CurrentUser?.Role != "admin")
            {
                MessageBox.Show("Доступно только администраторам", "Доступ запрещён",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var allTasksWindow = new AllTasksWindow();
            allTasksWindow.Owner = this;
            allTasksWindow.ShowDialog();
        }

        private void Sidebar_LogoutClick(object sender, RoutedEventArgs e)
        {
            // Выход уже обработан в SidebarControl
            // Закрываем текущее окно
            this.Close();
        }
    }
}