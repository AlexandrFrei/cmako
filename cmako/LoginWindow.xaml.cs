using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
using System.Windows.Shapes;
using System.Security.Cryptography;

namespace cmako
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            Password_TextBox.PasswordChar = '*';
        }


        private void Login_Button_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string login = Login_TextBox.Text;
            string password = Password_TextBox.Password;

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                try
                {
                    con.Open();
                    MySqlDataAdapter SDA = new MySqlDataAdapter("SELECT id, firstname, lastname FROM mdl_user WHERE username = '" + login + "'", con);
                    DataTable DATA = new DataTable();
                    SDA.Fill(DATA);
                    int count = DATA.Rows.Count;
                    if (count == 1)
                    {
                        Log.ID_Login = Convert.ToInt32(DATA.Rows[0][0]);
                        Log.User_Name = Convert.ToString(DATA.Rows[0][2]) + " " + Convert.ToString(DATA.Rows[0][1]);
                        MainWindow MainWindow = new MainWindow();
                        MainWindow.Show();
                        this.Hide();
                    }
                    else
                    {
                        MessageBox.Show("Неверный логин или пароль", "Ошибка!");
                    }
                    con.Close();
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    con.Close();
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Login_TextBox.Text = "cmakoagu";
            Login_Button_Click(sender, e);
        }
    }

    public class Log
    {
        public Log()
        { }
        public static int ID_Login;
        public static string User_Name;
    }
}
