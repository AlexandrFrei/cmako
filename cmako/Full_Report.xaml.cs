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

using Excel = Microsoft.Office.Interop.Excel;

namespace cmako
{
    /// <summary>
    /// Логика взаимодействия для Full_Report.xaml
    /// </summary>
    public partial class Full_Report : Window
    {
        string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        DataTable Section;
        DataTable Quiz_Slot;
        DataTable Quiz_Result;
        public int quiz_id;
        public int course_id;
        List<int> selection = new List<int> { };

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

        public Full_Report()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                try
                {
                    con.Open();
                    //string query = "SELECT heading, firstslot, sumgrades FROM mdl_quiz_sections, mdl_quiz WHERE quizid = " + quiz_id + " AND mdl_quiz_sections.quizid=mdl_quiz.id ORDER BY firstslot";
                    string query = "SELECT heading, firstslot FROM mdl_quiz_sections, mdl_quiz WHERE quizid = " + quiz_id + " AND mdl_quiz_sections.quizid=mdl_quiz.id ORDER BY firstslot";
                    Section = Read_Data(query, con);

                    Quiz_Slot = Read_Data("SELECT slot, ROUND(maxmark,1) FROM mdl_quiz_slots WHERE mdl_quiz_slots.quizid = " + quiz_id, con);

                    string query_Question_Result = "SELECT questionattemptid, fraction " +
                            "FROM mdl_question_attempt_steps Q1, (SELECT questionattemptid AS QAI, MAX(sequencenumber) AS sequencenumber FROM mdl_question_attempt_steps GROUP BY QAI) Q2 " +
                            "WHERE Q1.questionattemptid = Q2.QAI AND Q1.sequencenumber = Q2.sequencenumber";

                    Read_Data("SET @row_number = 0", con);
                    query = "SELECT (@row_number:=@row_number + 1) AS `№`, user.lastname, user.firstname, groups.name, from_unixtime(QUIZ_ATTEMPTS.timestart-3600, '%d.%m.%Y %H:%i:%s'), from_unixtime(QUIZ_ATTEMPTS.timefinish-3600, '%d.%m.%Y %H:%i:%s')";
                    int count_section = Section.Rows.Count;
                    double mask = 0;
                    for (int i = 0; i < count_section; i++)
                    {
                        mask = 0;
                        if (i != count_section - 1)
                        {
                            for (int q = Convert.ToInt32(Section.Rows[i][1].ToString()); q < Convert.ToInt32(Section.Rows[i + 1][1]); q++)
                            {
                                query += ", SECTION" + i + ".Q" + q + "_fraction AS `В" + q+ " (" + Convert.ToString(Quiz_Slot.Rows[q - 1][1]) + ")`";
                                mask += Convert.ToDouble(Quiz_Slot.Rows[q - 1][1]);
                            }
                        }
                        else
                        {
                            for (int q = Convert.ToInt32(Section.Rows[i][1].ToString()); q <= Convert.ToInt32(Quiz_Slot.Rows.Count); q++)
                            {
                                query += ", SECTION" + i + ".Q" + q + "_fraction AS `В" + q + " (" + Convert.ToString(Quiz_Slot.Rows[q-1][1]) + ")`";
                                mask += Convert.ToDouble(Quiz_Slot.Rows[q - 1][1]);
                            }
                        }
                        query += ", SECTION" + i + ".MARK AS `" + Section.Rows[i][0].ToString() + " (" + mask + ")`, SECTION" + i + ".res AS `Уровень сформированности " + Section.Rows[i][0].ToString() + "`";
                    }
                    query += " FROM mdl_user as user, mdl_groups as groups, mdl_groups_members as groups_member, mdl_quiz_attempts as QUIZ_ATTEMPTS";
                    for (int i = 0; i < count_section; i++)
                    {
                        
                        if (i != count_section - 1)
                        {
                            query += " LEFT JOIN (SELECT questionusageid,";
                            for (int q= Convert.ToInt32(Section.Rows[i][1].ToString());q < Convert.ToInt32(Section.Rows[i + 1][1]); q++)
                            {
                                query += " ROUND(SUM(CASE WHEN slot = " + q + " THEN QUESTION_RESULT.fraction * maxmark END),1) Q" + q + "_fraction,";
                            }
                            query += " ROUND(SUM(QUESTION_RESULT.fraction * maxmark),1) as MARK , CASE WHEN SUM(QUESTION_RESULT.fraction)/ COUNT(QUESTION_RESULT.fraction) >= 0.9 THEN 'Высокий уровень' ELSE CASE WHEN SUM(QUESTION_RESULT.fraction)/ COUNT(QUESTION_RESULT.fraction) >= 0.7 THEN 'Средний уровень' ELSE CASE WHEN SUM(QUESTION_RESULT.fraction)/ COUNT(QUESTION_RESULT.fraction) >= 0.6 THEN 'Низкий уровень' ELSE 'Не сформирована' END END END AS res FROM mdl_question_attempts, (" + query_Question_Result + ") AS QUESTION_RESULT";
                            query += " WHERE slot >= " + Section.Rows[i][1].ToString() + " AND slot < " + Section.Rows[i + 1][1].ToString();
                        }
                        else
                        {
                            query += " LEFT JOIN (SELECT questionusageid, ";
                            for (int q = Convert.ToInt32(Section.Rows[i][1].ToString()); q <= Convert.ToInt32(Quiz_Slot.Rows.Count); q++)
                            {
                                query += " ROUND(SUM(CASE WHEN slot = " + q + " THEN QUESTION_RESULT.fraction * maxmark END),1) Q" + q + "_fraction,";
                            }
                            query += " ROUND(SUM(QUESTION_RESULT.fraction * maxmark),1) as MARK , CASE WHEN SUM(QUESTION_RESULT.fraction)/ COUNT(QUESTION_RESULT.fraction) >= 0.9 THEN 'Высокий уровень' ELSE CASE WHEN SUM(QUESTION_RESULT.fraction)/ COUNT(QUESTION_RESULT.fraction) >= 0.7 THEN 'Средний уровень' ELSE CASE WHEN SUM(QUESTION_RESULT.fraction)/ COUNT(QUESTION_RESULT.fraction) >= 0.6 THEN 'Низкий уровень' ELSE 'Не сформирована' END END END AS res FROM mdl_question_attempts, (" + query_Question_Result + ") AS QUESTION_RESULT"; query += " WHERE slot >= " + Section.Rows[i][1].ToString();
                        }
                        query += " AND QUESTION_RESULT.questionattemptid = mdl_question_attempts.id GROUP BY questionusageid) SECTION" + i + " ON QUIZ_ATTEMPTS.uniqueid= SECTION" + i + ".questionusageid";
                    }
                    query += " WHERE user.id IN (SELECT userid FROM mdl_quiz_attempts WHERE quiz = " + quiz_id + ") AND user.id = QUIZ_ATTEMPTS.userid AND QUIZ_ATTEMPTS.quiz = " + quiz_id+ " AND groups.courseid = " + course_id+ " AND groups.id = groups_member.groupid AND groups_member.userid = user.id";
                    Quiz_Result = Read_Data(query, con);


                    Quiz_Result.Columns[1].ColumnName = "Фамилия";
                    Quiz_Result.Columns[2].ColumnName = "Имя";
                    Quiz_Result.Columns[3].ColumnName = "Группа";
                    Quiz_Result.Columns[4].ColumnName = "Тест начат";
                    Quiz_Result.Columns[5].ColumnName = "Тест завершен";
                    //for (int i = 0; i < count_section; i++)
                    //{
                    //    //Quiz_Result.Columns[4 + i].ColumnName = Section.Rows[i][0].ToString();
                    //    Quiz_Result.Columns[6 + i * 2].ColumnName = Section.Rows[i][0].ToString() + " усвоенность";
                    //}
                    dataGridView1.DataContext = Quiz_Result;

                    
                    con.Close();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }
            //button1_Click(sender, e);
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            Style style = new Style(typeof(DataGridCell));
            selection.Clear();
            //style.Setters.Add(new Setter(HorizontalAlignmentProperty, HorizontalAlignment.Center));
            style.Setters.Add(new Setter(FontWeightProperty, FontWeights.Bold));
            //style.Setters.Add(new Setter(ForegroundProperty, Brushes.Red));
            style.Setters.Add(new Setter(BackgroundProperty, Brushes.LightGray));

            for (int i = 0; i < Section.Rows.Count; i++)
            {
                for (int j = 0; j < Quiz_Result.Columns.Count; j++)
                {
                    if (Quiz_Result.Columns[j].ColumnName.ToString().Contains(Section.Rows[i][0].ToString()))
                    {
                        dataGridView1.Columns[j].CellStyle = style;
                        selection.Add(j);
                    }
                }
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Excel.Application application = new Excel.Application();
            Excel.Workbook workbook = application.Workbooks.Add(Type.Missing);
            Excel.Worksheet worksheet = null;
            
            worksheet = workbook.Sheets["Лист1"];
            worksheet = workbook.ActiveSheet;
            worksheet.Name = "Результаты тестирования";
            
            
            for (int i = 0; i < Quiz_Result.Rows.Count; i++)
            {
                for (int j = 0; j < Quiz_Result.Columns.Count; j++)
                {
                    worksheet.Cells[i + 2, j + 1] = Quiz_Result.Rows[i][j].ToString();
                    if (selection.Contains(j+2))
                    {
                        worksheet.Cells[i + 2, j + 3].Font.Bold = true;
                        worksheet.Cells[i + 2, j+3].Interior.ColorIndex = 15;
                        worksheet.Cells[i + 2, j+3].Interior.PatternColorIndex = Excel.Constants.xlAutomatic;

                    }
                }
            }
            for (int i = 1; i < Quiz_Result.Columns.Count + 1; i++)
            {
                worksheet.Cells[1, i] = Quiz_Result.Columns[i - 1].ColumnName;
                if (selection.Contains(i + 1))
                {
                    worksheet.Cells[1, i + 2].Font.Bold = true;
                    worksheet.Cells[1, i + 2].Interior.ColorIndex = 15;
                    worksheet.Cells[1, i + 2].Interior.PatternColorIndex = Excel.Constants.xlAutomatic;
                }
                worksheet.Columns[i].AutoFit();
            }

            application.Visible = true;

        }
    }
}
