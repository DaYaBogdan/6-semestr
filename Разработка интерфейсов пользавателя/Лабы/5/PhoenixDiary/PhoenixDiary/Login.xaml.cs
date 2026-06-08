using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Windows; // если нужно
// но using для GlobalData не нужен, так как он в том же пространстве имён

namespace PhoenixDiary
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            // Поддержка Enter для отправки формы
            LoginTextBox.KeyDown += OnKeyDown;
            PasswordBox.KeyDown += OnKeyDown;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _ = Login();
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            await Login();
        }

        private async Task Login()
        {
            string login = LoginTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите логин и пароль", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Включаем индикатор загрузки (меняем текст кнопки)
                LoginButton.Content = "Вход...";
                LoginButton.IsEnabled = false;
                LoginButton.Cursor = Cursors.Wait;

                // TODO: Здесь ваш реальный вызов API/сервиса
                // await store.dispatch("login", new { login, password });
                bool isAuthenticated = await AuthenticateUser(login, password);

                if (isAuthenticated)
                {
                    // Успешный вход - открываем главное окно
                    var diaryWindow = new Diary();
                    diaryWindow.Show();


                    // Закрываем окно входа
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль", "Ошибка входа",
                        MessageBoxButton.OK, MessageBoxImage.Error);

                    // Очищаем поля
                    PasswordBox.Password = "";
                    LoginTextBox.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при входе: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Возвращаем кнопку в исходное состояние
                LoginButton.Content = "Вход";
                LoginButton.IsEnabled = true;
                LoginButton.Cursor = Cursors.Hand;
            }
        }

        private async Task<bool> AuthenticateUser(string login, string password)
        {
            // TODO: Заменить на реальную авторизацию через API
            // Сейчас просто заглушка для теста
            await Task.Delay(500); // Имитация задержки сети

            // Пример: админ / admin123
            if (login == "admin" && password == "admin123")
            {
                GlobalData.CurrentUser = new AppUser { Login = login, Role = "admin" };
                return true;
            }

            // Пример: мастер / master123
            if (login == "master" && password == "master123")
            {
                // Сохраняем роль пользователя (можно через глобальный класс)
                GlobalData.CurrentUser = new AppUser { Login = login, Role = "master" };
                return true;
            }

            return false;
        }
    }
}