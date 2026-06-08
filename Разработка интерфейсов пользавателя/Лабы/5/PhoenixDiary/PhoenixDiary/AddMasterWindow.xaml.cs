using System;
using System.Windows;
using System.Windows.Controls;

namespace PhoenixDiary
{
    public partial class AddMasterWindow : Window
    {
        public AddMasterWindow()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(LoginTextBox.Text))
            {
                MessageBox.Show("Введите логин", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (PasswordBox.Password.Length < 6)
            {
                MessageBox.Show("Пароль должен содержать не менее 6 символов", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (PasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Пароли не совпадают", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Получаем выбранную роль
            var selectedRole = (RoleComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "master";

            // TODO: Сохранение в API / БД
            // await Api.AddMaster(new { login, password, role })

            MessageBox.Show($"Мастер {LoginTextBox.Text} успешно добавлен с ролью {selectedRole}",
                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}