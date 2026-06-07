using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PhoenixDiary
{
    public partial class AllTasksWindow : Window, INotifyPropertyChanged
    {
        // Модель задачи (расширенная)
        public class AllTasksTaskItem : INotifyPropertyChanged
        {
            private bool _isSelected;
            public int Id { get; set; }
            public string ClientName { get; set; }
            public string ServiceName { get; set; }
            public DateTime DateTime { get; set; }
            public string TimeRange => $"{DateTime:HH:mm}";
            public int MasterId { get; set; }
            public string MasterName { get; set; }

            public bool IsSelected
            {
                get => _isSelected;
                set { _isSelected = value; OnPropertyChanged(); }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string name = null) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // Модель дня
        public class DayModel : INotifyPropertyChanged
        {
            private ObservableCollection<AllTasksTaskItem> _tasks = new ObservableCollection<AllTasksTaskItem>();

            public DateTime Date { get; set; }
            public string WeekdayName { get; set; }
            public string FormattedDate => $"{Date:dd.MM}";

            public ObservableCollection<AllTasksTaskItem> Tasks
            {
                get => _tasks;
                set { _tasks = value; OnPropertyChanged(); }
            }

            public bool IsToday { get; set; }
            public string HasNoTasks => Tasks.Count == 0 ? "Visible" : "Collapsed";

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string name = null) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private DateTime _referenceDate = DateTime.Today;
        private ObservableCollection<DayModel> _weekDays = new ObservableCollection<DayModel>();
        private bool _isLoading = false;

        public ObservableCollection<DayModel> WeekDays
        {
            get => _weekDays;
            set { _weekDays = value; OnPropertyChanged(); }
        }

        private int _selectedCount;
        public int SelectedCount
        {
            get => _selectedCount;
            set
            {
                _selectedCount = value;
                OnPropertyChanged();
                DeleteButton.IsEnabled = value > 0;
                SelectedCountText.Text = $"Выбрано: {value}";
            }
        }

        public AllTasksWindow()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += AllTasksWindow_Loaded;
        }

        private async void AllTasksWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadAllTasks();
            RefreshWeekDays();
        }

        private async Task LoadAllTasks()
        {
            _isLoading = true;
            LoadingPanel.Visibility = Visibility.Visible;

            try
            {
                // TODO: Загрузить все задачи всех мастеров из API
                // var tasks = await Api.GetAllTasks();

                // Заглушка для теста
                var mockTasks = new[]
                {
                    new AllTasksTaskItem { Id = 1, ClientName = "Анна Иванова", ServiceName = "Маникюр + гель-лак", DateTime = DateTime.Today.AddHours(10), MasterId = 1, MasterName = "Анна (мастер)" },
                    new AllTasksTaskItem { Id = 2, ClientName = "Мария Петрова", ServiceName = "Педикюр", DateTime = DateTime.Today.AddHours(14), MasterId = 1, MasterName = "Анна (мастер)" },
                    new AllTasksTaskItem { Id = 3, ClientName = "Елена Сидорова", ServiceName = "Маникюр", DateTime = DateTime.Today.AddDays(1).AddHours(11), MasterId = 2, MasterName = "Ирина (мастер)" },
                    new AllTasksTaskItem { Id = 4, ClientName = "Ольга Смирнова", ServiceName = "Покрытие гель-лак", DateTime = DateTime.Today.AddDays(2).AddHours(15), MasterId = 1, MasterName = "Анна (мастер)" },
                };

                GlobalData.AllTasksList?.Clear();
                if (GlobalData.AllTasksList == null)
                {
                    GlobalData.AllTasksList = new ObservableCollection<AllTasksTaskItem>();
                }

                foreach (var task in mockTasks)
                {
                    GlobalData.AllTasksList.Add(task);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки записей: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isLoading = false;
                LoadingPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void RefreshWeekDays()
        {
            WeekDays.Clear();
            var monday = GetMonday(_referenceDate);

            for (int i = 0; i < 7; i++)
            {
                var currentDay = monday.AddDays(i);
                var tasksForDay = GlobalData.AllTasksList?
                    .Where(t => t.DateTime.Date == currentDay.Date)
                    .ToList() ?? new List<AllTasksTaskItem>();

                var dayModel = new DayModel
                {
                    Date = currentDay,
                    WeekdayName = GetWeekdayName(currentDay.DayOfWeek),
                    IsToday = currentDay.Date == DateTime.Today
                };

                foreach (var task in tasksForDay)
                {
                    dayModel.Tasks.Add(task);
                    // Подписываемся на изменение выбора
                    task.PropertyChanged += Task_PropertyChanged;
                }

                WeekDays.Add(dayModel);
            }

            UpdateSelectedCount();
        }

        private void Task_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AllTasksTaskItem.IsSelected))
            {
                UpdateSelectedCount();
            }
        }

        private void UpdateSelectedCount()
        {
            SelectedCount = GlobalData.AllTasksList?.Count(t => t.IsSelected) ?? 0;
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

        private void OpenAddModal_Click(object sender, RoutedEventArgs e)
        {
            // TODO: открыть окно добавления задачи
            MessageBox.Show("Добавление записи (в разработке)", "Информация",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void DeleteSelectedTasks_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCount == 0) return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить {SelectedCount} запись(ей)?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                var toDelete = GlobalData.AllTasksList?.Where(t => t.IsSelected).ToList();

                if (toDelete != null)
                {
                    foreach (var task in toDelete)
                    {
                        // TODO: API DELETE /tasks/{id}
                        GlobalData.AllTasksList.Remove(task);
                    }
                }

                RefreshWeekDays();

                MessageBox.Show("Записи успешно удалены", "Удаление",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
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
    }
}