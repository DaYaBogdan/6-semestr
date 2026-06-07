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
    public partial class ClientsWindow : Window, INotifyPropertyChanged
    {
        // Модель клиента
        public class ClientModel : INotifyPropertyChanged
        {
            private bool _isSelected;
            public int Id { get; set; }
            public string FIO { get; set; }
            public string Phone { get; set; }

            public bool IsSelected
            {
                get => _isSelected;
                set { _isSelected = value; OnPropertyChanged(); }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string name = null) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // Коллекция клиентов
        private ObservableCollection<ClientModel> _clients = new ObservableCollection<ClientModel>();
        public ObservableCollection<ClientModel> Clients
        {
            get => _clients;
            set { _clients = value; OnPropertyChanged(); }
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

        public ClientsWindow()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += ClientsWindow_Loaded;
        }

        private async void ClientsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadClients();
        }

        private async Task LoadClients()
        {
            try
            {
                // TODO: Загрузка из API / БД
                // var clients = await Api.GetClients();

                // Заглушка для теста
                var mockClients = new[]
                {
                    new ClientModel { Id = 1, FIO = "Анна Иванова", Phone = "+7 (999) 123-45-67" },
                    new ClientModel { Id = 2, FIO = "Мария Петрова", Phone = "+7 (999) 234-56-78" },
                    new ClientModel { Id = 3, FIO = "Елена Сидорова", Phone = "+7 (999) 345-67-89" },
                };

                Clients.Clear();
                foreach (var client in mockClients)
                {
                    Clients.Add(client);
                }

                UpdateUIState();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки клиентов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateUIState()
        {
            bool hasClients = Clients.Count > 0;
            HeaderPanel.Visibility = hasClients ? Visibility.Visible : Visibility.Collapsed;
            EmptyStatePanel.Visibility = hasClients ? Visibility.Collapsed : Visibility.Visible;

            // Обновляем текст кнопки "Выбрать все"
            UpdateSelectAllButtonText();
        }

        private void UpdateSelectAllButtonText()
        {
            if (Clients.Count == 0)
            {
                SelectAllText.Text = "Выбрать все";
                SelectAllButton.IsEnabled = false;
            }
            else
            {
                SelectAllButton.IsEnabled = true;
                if (SelectedCount == Clients.Count)
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
            SelectedCount = Clients.Count(c => c.IsSelected);
            UpdateSelectAllButtonText();
        }

        private void ClientCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UpdateSelectedCount();
        }

        private void ClientCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateSelectedCount();
        }

        private void OpenAddModal_Click(object sender, RoutedEventArgs e)
        {
            var addClientWindow = new AddClientWindow();
            addClientWindow.Owner = this;
            if (addClientWindow.ShowDialog() == true)
            {
                // Обновляем список клиентов
                _ = LoadClients();
            }
        }

        private async void DeleteSelectedClients_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCount == 0) return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить {SelectedCount} клиента(ов)?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                var toDelete = Clients.Where(c => c.IsSelected).ToList();

                foreach (var client in toDelete)
                {
                    // TODO: API DELETE /clients/{id}
                    Clients.Remove(client);
                }

                UpdateSelectedCount();
                UpdateUIState();

                MessageBox.Show("Клиенты успешно удалены", "Удаление",
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
            if (Clients.Count == 0) return;

            // Если все уже выбраны - снимаем все
            if (SelectedCount == Clients.Count)
            {
                foreach (var client in Clients)
                {
                    client.IsSelected = false;
                }
            }
            else
            {
                foreach (var client in Clients)
                {
                    client.IsSelected = true;
                }
            }

            UpdateSelectedCount();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}