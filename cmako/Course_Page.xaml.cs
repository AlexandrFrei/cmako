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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace cmako
{
    /// <summary>
    /// Логика взаимодействия для Course_Page.xaml
    /// </summary>
    public partial class Course_Page : Page
    {
        string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        public Course_Page()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Test_ListView.Visibility = Visibility.Hidden;

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                try
                {
                    string Query_Course = "SELECT shortname, fullname, id " +
                            "FROM mdl_course " +
                            "WHERE id IN " +
                                "(SELECT instanceid FROM mdl_context WHERE id IN " +
                                    "(SELECT contextid FROM mdl_role_assignments WHERE userid = " + Log.ID_Login + " AND roleid >= 3 AND roleid <= 4)" +
                                    "AND contextlevel = 50)";
                    con.Open();
                    MySqlDataAdapter SDA_Course = new MySqlDataAdapter(Query_Course, con);
                    DataTable DATA_Course = new DataTable();
                    SDA_Course.Fill(DATA_Course);
                    Course_ListView.ItemsSource = DATA_Course.DefaultView;
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

        private void Course_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string Query_Test = "SELECT id, name FROM mdl_quiz WHERE course = " + Course_ListView.SelectedValue;
            Test_ListView.Visibility = Visibility.Visible;

            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                try
                {

                    con.Open();
                    MySqlDataAdapter SDA_Test = new MySqlDataAdapter(Query_Test, con);
                    DataTable DATA_Test = new DataTable();
                    SDA_Test.Fill(DATA_Test);
                    Test_ListView.ItemsSource = DATA_Test.DefaultView;
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

        private void Short_Report_Click(object sender, RoutedEventArgs e)
        {
            Short_Report short_Report = new Short_Report();
            short_Report.quiz_id = Convert.ToInt32(Test_ListView.SelectedValue);
            short_Report.course_id = Convert.ToInt32(Course_ListView.SelectedValue);
            short_Report.Show();

        }

        private void Full_Report_Click(object sender, RoutedEventArgs e)
        {
            Full_Report full_Report = new Full_Report();
            full_Report.quiz_id = Convert.ToInt32(Test_ListView.SelectedValue);
            full_Report.course_id = Convert.ToInt32(Course_ListView.SelectedValue);
            full_Report.Show();
        }



        private void Statistics_Button_Click(object sender, RoutedEventArgs e)
        {
            Statistics_Window statistics = new Statistics_Window();
            statistics.quiz_id = Convert.ToInt32(Test_ListView.SelectedValue);
            statistics.Show();
        }
    }
}
