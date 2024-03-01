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

namespace cmako
{
    /// <summary>
    /// Логика взаимодействия для Search_Category_Window.xaml
    /// </summary>
    public partial class Search_Category_Window : Window
    {
        string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        public List<string> categories = new List<string>();
        public Search_Category_Window()
        {
            InitializeComponent();
        }

        private DataTable Read_Data(String query, MySqlConnection connection)
        {
            DataTable data = new DataTable();
            data.Clear();
            MySqlDataReader DataRead = null;
            MySqlCommand myCommand = new MySqlCommand(query, connection);
            if (query.TrimStart().ToUpper().StartsWith("SELECT"))
            {
                DataRead = myCommand.ExecuteReader();
                data.Load(DataRead);
            }
            else
                myCommand.ExecuteNonQuery();
            return data;
        }

        private void Find_BTZ_Click(object sender, RoutedEventArgs e)
        {
            if (Categories_ComboBox.SelectedValue != null)
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    try
                    {
                        con.Open();

                        DataTable Parent = Read_Data("SELECT categories.id " +
                                    "FROM mdl_question_categories as categories, " +
                                        "(SELECT id " +
                                        "FROM mdl_context " +
                                        "WHERE instanceid = " + Categories_ComboBox.SelectedValue + " and contextlevel = 50) as course " +
                                "WHERE categories.contextid = course.id AND parent = 0", con);

                        string Query_Course =
                            "SELECT CONCAT(tab1.name,'/', tab2.name,'/', tab3.name), count.count " +
                            "FROM mdl_question_categories AS tab1 " +
                                "LEFT JOIN(SELECT id, name, parent FROM `mdl_question_categories`) tab2 on tab1.id = tab2.parent " +
                                "LEFT JOIN(SELECT id, name, parent FROM `mdl_question_categories`) tab3 on tab2.id = tab3.parent " +
                                "LEFT JOIN(SELECT c.id, COUNT(q.category) as count " +
                                    "FROM mdl_question as q, mdl_question_categories as c " +
                                    "WHERE q.category = c.id " +
                                    "GROUP BY c.id) count on tab3.id = count.id " +
                                "WHERE tab1.parent = " + Parent.Rows[0][0] + " AND tab3.name IS NOT null";

                        MySqlDataAdapter SDA_Categories = new MySqlDataAdapter(Query_Course, con);
                        DataTable DATA_Categories = new DataTable();
                        SDA_Categories.Fill(DATA_Categories);
                        con.Close();
                        bool find = false;
                        List<string> no_categories = new List<string>();
                        List<string> no_questions = new List<string>();
                        if (DATA_Categories.Rows.Count != 0)
                        {
                            var row = DATA_Categories.Rows[0].ItemArray; ;
                            for (int j = 0; j < categories.Count; j++)
                            {
                                find = false;
                                for (int i = 0; i < DATA_Categories.Rows.Count; i++)
                                {
                                    row = DATA_Categories.Rows[i].ItemArray;
                                    if (categories[j].Equals(row[0].ToString()))
                                    {
                                        if (row[1] != null)
                                        {
                                            find = true;
                                            break;
                                        }
                                        else
                                        {
                                            no_questions.Add(categories[j].ToString());
                                            break;
                                        }
                                    }
                                }
                                if (!find)
                                {
                                    no_categories.Add(categories[j].ToString());
                                }

                            }
                            if (no_questions.Count == 0 && no_categories.Count == 0)
                            {
                                Result_TextBlock.Text = "Банк тестовых материалов содержит все необходимые материалы для проведения тестирования";
                            }
                            else if(no_categories.Count == categories.Count)
                            {
                                Result_TextBlock.Text = "В выбранном курсе отсутствуют тестовые материалы, для данной группы.";
                            }
                            else if (no_categories.Count > 0)
                            {
                                Result_TextBlock.Text = "В банке тестовых материалов отсутствуют следующие категории:\n";
                                for (int i = 0; i < no_categories.Count; i++)
                                {
                                    Result_TextBlock.Text += no_categories[i].ToString() + "\n";
                                }
                            }
                            else if (no_questions.Count > 0)
                            {
                                Result_TextBlock.Text = "В банке тестовых материалов отсутствуют задания в следующих категориях:\n";
                                for (int i = 0; i < no_questions.Count; i++)
                                {
                                    Result_TextBlock.Text += no_questions[i].ToString() + "\n";
                                }
                            }
                        }
                        else
                        {
                            Result_TextBlock.Text = "В выбранном курсе отсутствуют тестовые материалы, для данной группы.";
                        }
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
            else
            {
                MessageBox.Show("Не выбран курс");
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
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
                    Categories_ComboBox.ItemsSource = DATA_Course.DefaultView;
                    
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
    }
}
