using System.Collections.ObjectModel;

namespace PhoenixDiary
{
    public static class GlobalData
    {
        // Задачи для главного дневника (текущего мастера)
        public static ObservableCollection<Diary.TaskItem> AllTasks { get; set; }
            = new ObservableCollection<Diary.TaskItem>();

        // Все задачи для администратора
        public static ObservableCollection<AllTasksWindow.AllTasksTaskItem> AllTasksList { get; set; }
            = new ObservableCollection<AllTasksWindow.AllTasksTaskItem>();

        public static AppUser CurrentUser { get; set; }
    }

    public class AppUser
    {
        public string Login { get; set; }
        public string Role { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
    }
}