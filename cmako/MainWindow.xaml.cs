using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace cmako
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Width = SystemParameters.WorkArea.Width;
            this.Height = SystemParameters.WorkArea.Height;
            User_label.Content = Log.User_Name;
            
        }

        private void MenuItemMoodle_Click(object sender, RoutedEventArgs e)
        {
            Main.Content = new Course_Page();
        }

        private void MenuItemSyllabus_Click(object sender, RoutedEventArgs e)
        {
            Main.Content = new Matrix();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string name = "cmako";//процесс, который нужно убить
            System.Diagnostics.Process[] etc = System.Diagnostics.Process.GetProcesses();//получим процессы
            foreach (System.Diagnostics.Process anti in etc)//обойдем каждый процесс
                if (anti.ProcessName.ToLower().Contains(name.ToLower())) anti.Kill();
        }
    }
}
