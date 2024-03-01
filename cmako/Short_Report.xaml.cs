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
    /// Логика взаимодействия для Short_Report.xaml
    /// </summary>
    public partial class Short_Report : Window
    {
        string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        DataTable Section;
        DataTable Quiz_Result;
        DataTable Group_Result;
        DataTable Quiz_Slot;
        public int quiz_id;
        public int course_id;

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

        public Short_Report()
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
                    string query = "SELECT heading, firstslot FROM mdl_quiz_sections WHERE quizid = " + quiz_id + " ORDER BY firstslot";
                    Section = Read_Data(query, con);
                    string query_Question_Result = "SELECT questionattemptid, fraction " +
                            "FROM mdl_question_attempt_steps Q1, (SELECT questionattemptid AS QAI, MAX(sequencenumber) AS sequencenumber FROM mdl_question_attempt_steps GROUP BY QAI) Q2 " +
                            "WHERE Q1.questionattemptid = Q2.QAI AND Q1.sequencenumber = Q2.sequencenumber";
                    
                    Quiz_Slot = Read_Data("SELECT slot, ROUND(maxmark,1) FROM mdl_quiz_slots WHERE mdl_quiz_slots.quizid = " + quiz_id, con);
                    
                    Read_Data("SET @row_number = 0", con);
                    query = "SELECT (@row_number:=@row_number + 1) AS `№`, user.lastname AS 'Фамилия', user.firstname AS 'Имя', groups.name AS 'Группа', from_unixtime(QUIZ_ATTEMPTS.timestart-3600, '%d.%m.%Y %H:%i:%s') AS 'Тест начат', from_unixtime(QUIZ_ATTEMPTS.timefinish-3600, '%d.%m.%Y %H:%i:%s') AS 'Тест завершен'";
                    int count_section = Section.Rows.Count;
                    double mask = 0;
                    for (int i = 0; i < count_section; i++)
                    {
                        mask = 0;
                        if (i != count_section - 1)
                        {
                            for (int q = Convert.ToInt32(Section.Rows[i][1].ToString()); q < Convert.ToInt32(Section.Rows[i + 1][1]); q++)
                            {
                                mask += Convert.ToDouble(Quiz_Slot.Rows[q - 1][1]);
                            }
                        }
                        else
                        {
                            for (int q = Convert.ToInt32(Section.Rows[i][1].ToString()); q <= Convert.ToInt32(Quiz_Slot.Rows.Count); q++)
                            {
                                mask += Convert.ToDouble(Quiz_Slot.Rows[q - 1][1]);
                            }
                        }
                        query += ", SECTION" + i + ".MARK AS `" + Section.Rows[i][0].ToString() + " (" + mask + ")`, SECTION" + i + ".res AS `Уровень сформированности " + Section.Rows[i][0].ToString() + "`";
                    }
                    query += " FROM mdl_user as user, mdl_groups as groups, mdl_groups_members as groups_member, mdl_quiz_attempts as QUIZ_ATTEMPTS";
                    for (int i = 0; i < count_section; i++)
                    {
                        //query += " LEFT JOIN (SELECT questionusageid, ROUND(SUM(QUESTION_RESULT.fraction * maxmark),1) as MARK , CASE WHEN SUM(QUESTION_RESULT.fraction)/ COUNT(QUESTION_RESULT.fraction) >= 0.6 THEN 'Усвоена' ELSE 'Не усвоена' END AS res FROM mdl_question_attempts, (" + query_Question_Result + ") AS QUESTION_RESULT";
                        query += " LEFT JOIN (SELECT questionusageid, ROUND(SUM(QUESTION_RESULT.fraction * maxmark),1) as MARK , CASE WHEN SUM(QUESTION_RESULT.fraction)/ COUNT(QUESTION_RESULT.fraction) >= 0.9 THEN 'Высокий уровень' ELSE CASE WHEN SUM(QUESTION_RESULT.fraction)/ COUNT(QUESTION_RESULT.fraction) >= 0.7 THEN 'Средний уровень' ELSE CASE WHEN SUM(QUESTION_RESULT.fraction)/ COUNT(QUESTION_RESULT.fraction) >= 0.6 THEN 'Низкий уровень' ELSE 'Не сформирована' END END END AS res FROM mdl_question_attempts, (" + query_Question_Result + ") AS QUESTION_RESULT";
                        if (i != count_section - 1)
                        {
                            query += " WHERE slot >= " + Section.Rows[i][1].ToString() + " AND slot < " + Section.Rows[i + 1][1].ToString();
                        }
                        else
                        {
                            query += " WHERE slot >= " + Section.Rows[i][1].ToString();
                        }
                        query += " AND QUESTION_RESULT.questionattemptid = mdl_question_attempts.id GROUP BY questionusageid) SECTION" + i + " ON QUIZ_ATTEMPTS.uniqueid= SECTION" + i + ".questionusageid";
                    }
                    query += " WHERE user.id IN (SELECT userid FROM mdl_quiz_attempts WHERE quiz = " + quiz_id + ") AND user.id = QUIZ_ATTEMPTS.userid AND QUIZ_ATTEMPTS.quiz = " + quiz_id + " AND groups.courseid = " + course_id + " AND groups.id = groups_member.groupid AND groups_member.userid = user.id"; ;
                    Quiz_Result = Read_Data(query, con);
                    
                    dataGridView1.DataContext = Quiz_Result;


                    Read_Data("SET @row_number = 0", con);
                    query = "SELECT (@row_number:=@row_number + 1) AS `№`, groups.name AS 'Группа', COUNT(SECTION0.MARK) AS `Обучающихся`";
                    count_section = Section.Rows.Count;
                    mask = 0;
                    for (int i = 0; i < count_section; i++)
                    {
                        mask = 0;
                        if (i != count_section - 1)
                        {
                            for (int q = Convert.ToInt32(Section.Rows[i][1].ToString()); q < Convert.ToInt32(Section.Rows[i + 1][1]); q++)
                            {
                                mask += Convert.ToDouble(Quiz_Slot.Rows[q - 1][1]);
                            }
                        }
                        else
                        {
                            for (int q = Convert.ToInt32(Section.Rows[i][1].ToString()); q <= Convert.ToInt32(Quiz_Slot.Rows.Count); q++)
                            {
                                mask += Convert.ToDouble(Quiz_Slot.Rows[q - 1][1]);
                            }
                        }
                        query += ", ROUND(SUM(SECTION" + i + ".MARK),0) AS `" + Section.Rows[i][0].ToString()+  " кол`, ROUND(SUM(SECTION" + i + ".MARK) / COUNT(SECTION" + i + ".MARK) * 100,1) AS `% сформ "+ Section.Rows[i][0].ToString() + "`, CASE WHEN SUM(SECTION" + i + ".MARK) / COUNT(SECTION" + i + ".MARK) >= 0.6 THEN 'Компетенция сформирована' ELSE 'Компетенция не сформирована' END AS `" + Section.Rows[i][0].ToString() + "`";
                    }
                    query += " FROM mdl_user as user, mdl_groups as groups, mdl_groups_members as groups_member, mdl_quiz_attempts as QUIZ_ATTEMPTS";
                    for (int i = 0; i < count_section; i++)
                    {
                        query += " LEFT JOIN (SELECT questionusageid, CASE WHEN SUM(QUESTION_RESULT.fraction)/ COUNT(QUESTION_RESULT.fraction) >= 0.6 THEN 1 ELSE 0 END AS MARK FROM mdl_question_attempts, (" + query_Question_Result + ") AS QUESTION_RESULT";
                        if (i != count_section - 1)
                        {
                            query += " WHERE slot >= " + Section.Rows[i][1].ToString() + " AND slot < " + Section.Rows[i + 1][1].ToString();
                        }
                        else
                        {
                            query += " WHERE slot >= " + Section.Rows[i][1].ToString();
                        }
                        query += " AND QUESTION_RESULT.questionattemptid = mdl_question_attempts.id GROUP BY questionusageid) SECTION" + i + " ON QUIZ_ATTEMPTS.uniqueid= SECTION" + i + ".questionusageid";
                    }
                    query += " WHERE user.id IN (SELECT userid FROM mdl_quiz_attempts WHERE quiz = " + quiz_id + ") AND user.id = QUIZ_ATTEMPTS.userid AND QUIZ_ATTEMPTS.quiz = " + quiz_id + " AND groups.courseid = " + course_id + " AND groups.id = groups_member.groupid AND groups_member.userid = user.id GROUP BY groups.name ORDER BY groups.name"; ;
                    Group_Result = Read_Data(query, con);
                    dataGridView2.DataContext = Group_Result;

                    con.Close();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
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
            worksheet.Name = "Сформированность компетенций";

            worksheet.Cells[1, 1] = "№ п/п";
            worksheet.Cells[1, 2] = "Группа";
            worksheet.Cells[1, 3] = "Количество обучающихся, прошедших тестирование";
            worksheet.Cells[1, 4] = "Количество (процент) обучающихсяб у которых сформирована компетенция";
            worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[2, 1]].Merge();
            worksheet.Range[worksheet.Cells[1, 2], worksheet.Cells[2, 2]].Merge();
            worksheet.Range[worksheet.Cells[1, 3], worksheet.Cells[2, 3]].Merge();
            worksheet.Range[worksheet.Cells[1, 4], worksheet.Cells[1, 3 + (Group_Result.Columns.Count - 3) / 3]].Merge();

            for (int i = 0; i < Section.Rows.Count; i++)
            {
                worksheet.Cells[2, i + 4] = Section.Rows[i][0].ToString();
            }

            application.Visible = true;
            int r = 3, c = 4;
            for (int i = 0; i < Group_Result.Rows.Count; i++, r += 3)
            {
                c = 4;
                worksheet.Cells[r, 1] = Group_Result.Rows[i][0].ToString();
                worksheet.Cells[r, 2] = Group_Result.Rows[i][1].ToString();
                worksheet.Cells[r, 3] = Group_Result.Rows[i][2].ToString();
                worksheet.Range[worksheet.Cells[r, 1], worksheet.Cells[r+2, 1]].Merge();
                worksheet.Range[worksheet.Cells[r, 2], worksheet.Cells[r + 2, 2]].Merge();
                worksheet.Range[worksheet.Cells[r, 3], worksheet.Cells[r + 2, 3]].Merge();
                for (int j = 3; j < Group_Result.Columns.Count; j += 3, c++)
                {
                    worksheet.Cells[r, c] = Group_Result.Rows[i][j].ToString();
                    worksheet.Cells[r + 1, c] = Group_Result.Rows[i][j + 1].ToString() + "%";
                    worksheet.Cells[r + 2, c] = Group_Result.Rows[i][j + 2].ToString();
                }
            }

            worksheet.Cells[r, 1] = "ИТОГО:";
            worksheet.Range[worksheet.Cells[r, 1], worksheet.Cells[r + 2, 2]].Merge();

            int sum = 0;
            for (int i = 0; i < Group_Result.Rows.Count; i++)
            {
                sum += Convert.ToInt32(Group_Result.Rows[i][2].ToString());
            }
            int count = sum;
            worksheet.Cells[r, 3] = Convert.ToString(count);
            worksheet.Range[worksheet.Cells[r, 3], worksheet.Cells[r + 2, 3]].Merge();
            double p = 0;
            c = 4;
            for (int j = 3; j < Group_Result.Columns.Count; j += 3, c++)
            {
                sum = 0;
                for (int i = 0; i < Group_Result.Rows.Count; i++)
                {
                    sum += Convert.ToInt32(Group_Result.Rows[i][j].ToString());
                }
                worksheet.Cells[r, c] = Convert.ToString(sum);
                p = Math.Round(sum / Convert.ToDouble(count),3);
                string str = Convert.ToString(p * 100) + "%";
                str = str.Replace(".", ",");
                worksheet.Cells[r + 1, c] = str;
                if (p >= 0.6)
                {
                    worksheet.Cells[r + 2, c] = "Компетенция сформирована";
                }
                else
                {
                    worksheet.Cells[r + 2, c] = "Компетенция не сформирована";
                }
            }

            worksheet.Columns.AutoFit();

            Excel.Range excelCells1 = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[r + 2, c - 1]];

            excelCells1.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            excelCells1.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            excelCells1.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;



        }
    }
}
