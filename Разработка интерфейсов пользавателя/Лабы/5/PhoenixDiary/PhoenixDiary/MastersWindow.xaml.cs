using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows; // если нужно
// но using для GlobalData не нужен, так как он в том же пространстве имён

namespace PhoenixDiary
{
    public partial class MastersWindow : Window, INotifyPropertyChanged
    {
        // Модель мастера
        public class MasterModel : INotifyPropertyChanged
        {
            private bool _isSelected;
            public int Id { get; set; }
            public string Login { get; set; }
            public string Role { get; set; }
            public string Password { get; set; } // В реальном приложении не хранить пароль в открытом виде

            public bool IsSelected
            {
                get => _isSelected;
                set { _isSelected = value; OnPropertyChanged(); }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string name = null) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // Коллекция мастеров
        private ObservableCollection<MasterModel> _masters = new ObservableCollection<MasterModel>();
        public ObservableCollection<MasterModel> Masters
        {
            get => _masters;
            set { _masters = value; OnPropertyChanged(); }
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

        public MastersWindow()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += MastersWindow_Loaded;
        }

        private async void MastersWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadMasters();
        }

        private async Task LoadMasters()
        {
            try
            {
                // TODO: Загрузка из API / БД
                // var masters = await Api.GetMasters();

                // Заглушка для теста
                var mockMasters = new[]
                {
                    new MasterModel { Id = 1, Login = "admin", Role = "admin", Password = "admin123" },
                    new MasterModel { Id = 2, Login = "annamaster", Role = "master", Password = "master123" },
                    new MasterModel { Id = 3, Login = "irinamaster", Role = "master", Password = "master456" },
                };

                Masters.Clear();
                foreach (var master in mockMasters)
                {
                    Masters.Add(master);
                }

                UpdateUIState();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки мастеров: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateUIState()
        {
            bool hasMasters = Masters.Count > 0;
            HeaderPanel.Visibility = hasMasters ? Visibility.Visible : Visibility.Collapsed;
            EmptyStatePanel.Visibility = hasMasters ? Visibility.Collapsed : Visibility.Visible;

            // Обновляем текст кнопки "Выбрать все"
            UpdateSelectAllButtonText();
        }

        private void UpdateSelectAllButtonText()
        {
            if (Masters.Count == 0)
            {
                SelectAllText.Text = "Выбрать все";
                SelectAllButton.IsEnabled = false;
            }
            else
            {
                SelectAllButton.IsEnabled = true;
                if (SelectedCount == Masters.Count)
                {
                    SelectAllText.Text = "Снять все";
                }
                else
                {
                    SelectAllText.Text = "Выбрать все";
                }
            }
        }

        private void UpdateSelectedCount()
        {
            SelectedCount = Masters.Count(m => m.IsSelected);
            UpdateSelectAllButtonText();
        }

        private void MasterCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UpdateSelectedCount();
        }

        private void MasterCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateSelectedCount();
        }

        private void OpenAddModal_Click(object sender, RoutedEventArgs e)
        {
            var addMasterWindow = new AddMasterWindow();
            addMasterWindow.Owner = this;
            if (addMasterWindow.ShowDialog() == true)
            {
                // Обновляем список мастеров
                _ = LoadMasters();
            }
        }

        private async void DeleteSelectedMasters_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCount == 0) return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить {SelectedCount} мастера(ов)?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                var toDelete = Masters.Where(m => m.IsSelected).ToList();

                foreach (var master in toDelete)
                {
                    // TODO: API DELETE /masters/{id}
                    // await Api.DeleteMaster(master.Id);
                    Masters.Remove(master);
                }

                UpdateSelectedCount();
                UpdateUIState();

                MessageBox.Show("Мастера успешно удалены", "Удаление",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            if (Masters.Count == 0) return;

            // Если все уже выбраны - снимаем все
            if (SelectedCount == Masters.Count)
            {
                foreach (var master in Masters)
                {
                    master.IsSelected = false;
                }
            }
            else
            {
                foreach (var master in Masters)
                {
                    master.IsSelected = true;
                }
            }

            UpdateSelectedCount();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}