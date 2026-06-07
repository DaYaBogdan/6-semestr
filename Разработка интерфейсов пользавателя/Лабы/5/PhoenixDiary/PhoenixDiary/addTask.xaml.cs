using System;
using System.Windows;

namespace PhoenixDiary
{
    public partial class AddTaskWindow : Window
    {
        public AddTaskWindow()
        {
            InitializeComponent();
            DatePicker.SelectedDate = DateTime.Today;
            // TODO: Загрузить списки клиентов, услуг, мастеров
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Валидация и сохранение
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