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
    /// Логика взаимодействия для Statistics_Window.xaml
    /// </summary>
    public partial class Statistics_Window : Window
    {
        string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        public int quiz_id;
        DataTable Statistics;

        public Statistics_Window()
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                try
                {
                    con.Open();
                    string query = "SELECT mdl_question.name AS 'Вопрос', mdl_question_statistics.s AS 'Попытки', " +
                                        "ROUND(mdl_question_statistics.facility * 100, 2) AS 'Индекс лёгкости', " +
                                        "ROUND(mdl_question_statistics.sd * 100, 2) AS 'Стандартное отклонение', " +
                                        "ROUND(mdl_quiz_slots.maxmark / mark_quiz.mark * 100, 2) AS 'Намеченный вес', " +
                                        "ROUND(mdl_question_statistics.effectiveweight, 2) AS 'Эффективный вес', " +
                                        "ROUND(mdl_question_statistics.discriminationindex, 2) AS 'Индекс дискриминации', " +
                                        "ROUND(mdl_question_statistics.discriminativeefficiency, 2) AS 'Эффективность дискриминации' " +
                                    "FROM mdl_question, mdl_question_attempts, mdl_question_usages, " +
                                        "mdl_quiz_attempts, mdl_quiz, mdl_quiz_slots, mdl_question_statistics, " +
                                        "(SELECT questionid, MAX(id) as id " +
                                        "FROM mdl_question_statistics " +
                                        "GROUP BY  questionid) AS questionstatmax, " +
                                        "(SELECT SUM(maxmark) as mark FROM mdl_quiz_slots WHERE mdl_quiz_slots.quizid = " + quiz_id + ") AS mark_quiz " +
                                    "WHERE mdl_question_attempts.questionid = mdl_question.id " +
                                        "AND mdl_question_usages.id = mdl_question_attempts.questionusageid " +
                                        "AND mdl_quiz_attempts.uniqueid = mdl_question_usages.id " +
                                        "AND mdl_quiz_attempts.quiz = mdl_quiz.id " +
                                        "AND mdl_quiz.id = " + quiz_id + " " +
                                        "AND mdl_quiz_slots.quizid = mdl_quiz.id " +
                                        "AND mdl_quiz_slots.questionid = mdl_question.id " +
                                        "AND mdl_question_statistics.id = questionstatmax.id " +
                                        "AND mdl_question_statistics.questionid = mdl_question.id " +
                                    "GROUP BY mdl_question.id";

                    Statistics = Read_Data(query, con);

                    dataGridView1.DataContext = Statistics;
                    con.Close();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }
        }
    }
}
