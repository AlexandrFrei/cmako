using ExcelDataReader;
using Microsoft.Win32;
using Syncfusion.Windows.Tools.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
    /// Логика взаимодействия для Matrix.xaml
    /// </summary>
    public partial class Matrix : Page
    {
        public DataTableCollection tableCollection = null;
        public List<string> categories = new List<string>();
        DataTable resultFilter;
        public Matrix()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            string filePath = "";
            OpenFileDialog ofd = new OpenFileDialog();

            try
            {
                if (ofd.ShowDialog() == true)
                {
                    filePath = ofd.FileName;

                    OpenExcelFile(filePath);

                    DataTable Syllabus = tableCollection["Учебный план"];
                    DataTable Competencies = tableCollection["Матрица компетенций"];
                    if (Syllabus == null || Competencies == null)
                    {
                        MessageBox.Show("Загружаемый файл имеет некорректное содержимое", "Ошибка!");
                        this.Content = null;
                        return;
                    }
                    //Компетенции
                    string s;
                    List<string> listCompetencies = new List<string> { };

                    Label_Speciality.Content = Syllabus.Rows[4].ItemArray[1].ToString() + " " + Syllabus.Rows[6].ItemArray[1].ToString().Split(' ')[1] + " г.п.";
                    var Rows = Competencies.Rows[1].ItemArray;
                    for (int i = 2; i < Competencies.Columns.Count; i++)
                    {
                        listCompetencies.Add(Rows[i].ToString());
                    }

                    for (int i = 0; i < listCompetencies.Count; i++)
                    {
                        CheckListCompetence.Items.Add(listCompetencies[i]);
                    }
                    DataTable Matrix = new DataTable("Matrix");
                    tableCollection.Add(Matrix);

                    Matrix.Columns.Add(new DataColumn("Блок", Type.GetType("System.String")));
                    Matrix.Columns.Add(new DataColumn("Дисциплина (семестр)", Type.GetType("System.String")));

                    for (int i = 0; i < listCompetencies.Count; i++)
                    {
                        Matrix.Columns.Add(new DataColumn(listCompetencies[i], Type.GetType("System.String")));
                    }

                    DataRow rowMatrix = Matrix.NewRow();

                    for (int i = 0; i < Syllabus.Rows.Count; i++)
                    {
                        var rowSyllabus = Syllabus.Rows[i].ItemArray;
                        if (rowSyllabus[0].ToString() != "" && !rowSyllabus[0].ToString().Contains("00") && rowSyllabus[0].ToString().Contains(".") && rowSyllabus[0].ToString().Length > 1)
                        {

                            for (int j = 0; j < Competencies.Rows.Count; j++)
                            {
                                var rowCompetencies = Competencies.Rows[j].ItemArray;
                                int len = rowCompetencies.Length;
                                if (rowCompetencies[0].ToString() != "" && !rowCompetencies[0].ToString().Contains("00") && rowCompetencies[0].ToString().Contains(".") && rowCompetencies[0].ToString().Length > 1)
                                {
                                    if (rowSyllabus[1].ToString().ToLower() == rowCompetencies[1].ToString().ToLower())
                                    {
                                        for (int x = 1; x <= 12; x++)
                                        {
                                            if (rowSyllabus[23 + x].ToString().Length > 0)
                                            {
                                                s = rowSyllabus[1].ToString().ToLower();
                                                s = s.First().ToString().ToUpper() + s.Substring(1);
                                                s = s + " (" + Convert.ToString(x) + " семестр)";

                                                var r = new object[Matrix.Columns.Count];
                                                r[0] = rowSyllabus[0].ToString();
                                                r[1] = s;
                                                for (int y = 2; y < len; y++)
                                                {
                                                    r[y] = rowCompetencies[y];
                                                }

                                                Matrix.Rows.Add(r);
                                            }

                                            else if (rowSyllabus[23 + x].ToString().Length == 0 && rowSyllabus[0].ToString().Contains(".Д.") && Syllabus.Rows[i - 1].ItemArray[23 + x].ToString().Length > 0)
                                            {
                                                s = rowSyllabus[1].ToString().ToLower();
                                                s = s.First().ToString().ToUpper() + s.Substring(1);
                                                s = s + " (" + Convert.ToString(x) + " семестр)";

                                                var r = new object[Matrix.Columns.Count];
                                                r[0] = rowSyllabus[0].ToString();
                                                r[1] = s;
                                                for (int y = 2; y < len; y++)
                                                {
                                                    r[y] = rowCompetencies[y];
                                                }

                                                Matrix.Rows.Add(r);
                                            }
                                        }
                                        break;
                                    }
                                }

                            }
                        }
                    }
                    dataGridView1.DataContext = Matrix;
                }
                else
                {
                    throw new Exception("Файл не выбран!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!");
                Content = null;
            }
        }

        private void OpenExcelFile(string path)
        {
            FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read);

            IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream);
            DataSet ds = reader.AsDataSet(new ExcelDataSetConfiguration()
            {
                ConfigureDataTable = (x) => new ExcelDataTableConfiguration()
                { UseHeaderRow = false }
            });
            tableCollection = ds.Tables;
            stream.Close();
        }

        private void Filter_Click(object sender, RoutedEventArgs e)
        {

            categories.Clear();
            int count = 0;
            DataTable Matrix = tableCollection["Matrix"];
            string query = "";
            if (CheckListSemester.SelectedItems.Count != 0)
            {
                query = "(";
                foreach (CheckListBoxItem item in CheckListSemester.SelectedItems)
                    query += " [Дисциплина (семестр)] LIKE '%" + item.Content.ToString() + "%' OR";
                query = query.TrimEnd('R');
                query = query.TrimEnd('O');
                query += ")";
            }
            
            if (CheckListSemester.SelectedItems.Count != 0 && CheckListCompetence.SelectedItems.Count != 0)
                query += " AND (";
            if (CheckListCompetence.SelectedItems.Count != 0)
            {
                foreach (String item in CheckListCompetence.SelectedItems)
                    query += " [" + item + "] = '+' OR";
                query = query.TrimEnd('R');
                query = query.TrimEnd('O');
                
            }
            if (CheckListSemester.SelectedItems.Count != 0 && CheckListCompetence.SelectedItems.Count != 0)
                query += ")";

            DataRow[] result = Matrix.Select(query);

            if (result.Length > 0)
            {
                resultFilter = result.CopyToDataTable<DataRow>();
                if (CheckListCompetence.SelectedItems.Count != 0)
                {
                    for (int i = resultFilter.Columns.Count - 1; i > 1; i--)
                    {
                        count = 0;
                        foreach (String item in CheckListCompetence.SelectedItems)
                            if (item == resultFilter.Columns[i].ColumnName)
                            {
                                count++;
                                break;
                            }
                        if (count == 0)
                            resultFilter.Columns.RemoveAt(i);
                    }
                }
                resultFilter.DefaultView.Sort = "[Блок]";
            }
            else
            {
                resultFilter = null;
            }

            
            dataGridView2.DataContext = resultFilter;

            for (int i = 0; i < result.Length; i++)
            {
                var row = resultFilter.Rows[i].ItemArray;
                for (int j = 2; j < row.Length; j++)
                {
                    if (row[j].ToString() == "+")
                    {
                        categories.Add(Label_Speciality.Content + "/" + row[1].ToString() + "/" + resultFilter.Columns[j].ColumnName);
                    }
                }

            }

        }

        private void FindQuestion_Click(object sender, RoutedEventArgs e)
        {
            Search_Category_Window search_Category_Window = new Search_Category_Window();
            search_Category_Window.categories = categories;
            search_Category_Window.Show();
        }

        private void PrintList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Create an instance for word app  
                Microsoft.Office.Interop.Word.Application winword = new Microsoft.Office.Interop.Word.Application();

                //Set animation status for word application  
                winword.ShowAnimation = false;

                //Set status for word application is to be visible or not.  
                winword.Visible = true;

                //Create a missing variable for missing value  
                object missing = System.Reflection.Missing.Value;

                //Create a new document  
                Microsoft.Office.Interop.Word.Document document = winword.Documents.Add(ref missing, ref missing, ref missing, ref missing);                

                //adding text to document  
                document.Content.SetRange(0, 0);
                document.Content.Text = Label_Speciality.Content + Environment.NewLine;
                //document.Content.Text += "\tТекст 1";
                //document.Content.Text += "\t\tТекст 2";
                int count;
                for(int i = 2; i < resultFilter.Columns.Count; i++)
                {
                    count = 0;
                    document.Content.Text += resultFilter.Columns[i].ColumnName;
                    for (int j=0; j < resultFilter.Rows.Count; j++)
                    {
                        if (resultFilter.Rows[j][i].ToString() == "+")
                        {
                            document.Content.Text += "\t" + resultFilter.Rows[j][0].ToString() + " " + resultFilter.Rows[j][1].ToString();
                            count++;
                        }
                    }
                    if (count == 0)
                    {
                        document.Content.Text += "\tДисциплины формирующие данную компетенцию отсутствуют";
                    }
                }

                ////Add paragraph with Heading 1 style  
                //Microsoft.Office.Interop.Word.Paragraph para1 = document.Content.Paragraphs.Add(ref missing);
                //object styleHeading1 = "Heading 1";
                //para1.Range.set_Style(ref styleHeading1);
                //para1.Range.Text = "Para 1 text";
                //para1.Range.InsertParagraphAfter();              

                //Save the document  
                //object filename = @"c:\temp1.docx";
                //document.SaveAs2(ref filename);
                //document.Close(ref missing, ref missing, ref missing);
                //document = null;
                //winword.Quit(ref missing, ref missing, ref missing);
                //winword = null;
                //MessageBox.Show("Document created successfully !");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
