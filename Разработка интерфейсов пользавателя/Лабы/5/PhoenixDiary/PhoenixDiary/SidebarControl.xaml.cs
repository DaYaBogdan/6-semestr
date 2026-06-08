using System.Windows;
using System.Windows.Controls;

namespace PhoenixDiary
{
    public partial class SidebarControl : UserControl
    {
        public event RoutedEventHandler DiaryClick;
        public event RoutedEventHandler ClientsClick;
        public event RoutedEventHandler MastersClick;
        public event RoutedEventHandler AllTasksClick;
        public event RoutedEventHandler LogoutClick;

        public SidebarControl()
        {
            InitializeComponent();
        }

        private void DiaryButton_Click(object sender, RoutedEventArgs e)
        {
            DiaryClick?.Invoke(this, e);
        }

        private void ClientsButton_Click(object sender, RoutedEventArgs e)
        {
            ClientsClick?.Invoke(this, e);
        }

        private void MastersButton_Click(object sender, RoutedEventArgs e)
        {
            MastersClick?.Invoke(this, e);
        }

        private void AllTasksButton_Click(object sender, RoutedEventArgs e)
        {
            AllTasksClick?.Invoke(this, e);
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти?", "Выход",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                GlobalData.CurrentUser = null;
                var loginWindow = new LoginWindow();
                loginWindow.Show();
                var mainWindow = Window.GetWindow(this);
                mainWindow?.Close();
            }
        }
    }
}