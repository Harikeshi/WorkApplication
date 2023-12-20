using Charts;
using EJ.logic.ej_get;
using EJ.logic.ej_xlsx;
using Erl.logic.nominals;
using Stat.logic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace Wpf.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // TODO: в setting добавить для дома и нет

        // Work
        //readonly string path = @"C:\Users\schegolihin\Documents\Задания";
        //readonly string excel_from = @"c:\Users\schegolihin\Documents\Задания"; // Откуда
        //readonly string excel_to = @"c:\Users\schegolihin\Documents\Задания"; // Куда

        //Home
        readonly string path = @"d:\Projects\work\actual"; // Home
        readonly string excel_from = @"d:\Projects\work\actual"; // Home
        readonly string excel_to = @"d:\Projects\work\actual"; // Home

        readonly string erlp = @"\erl.xlsx";
        readonly string ejp = @"\ej.txt";
        readonly string exc_out = @"\excel.xlsx";
        readonly string pdepo = @"\depo.xlsx";
        readonly string pdisp = @"\disp.xlsx";

        readonly string set_path = @"settings.txt";
        public MainWindow()
        {
            InitializeComponent();
            this.Top = 85;//0;
            this.Left = 0;// 0;
            this.Height = 70;
            //this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            this.Width = 635;
            this.Topmost = true;

            InitParam(set_path);

        }

        private void InitParam(string param)
        {
            Dictionary<string, string> settings = new Dictionary<string, string>();
            if (File.Exists(param))
            {
                var lines = File.ReadAllLines(param);
                foreach (var item in lines)
                {
                    var parameters = item.Split('=');
                    settings.Add(parameters[0], parameters[1]);
                }
                atmType.Text = settings["Atm"];
                StartTime.Text = settings["TimeBegin"];
                EndTime.Text = settings["TimeEnd"];
                atmNumber.Text = settings["Number"];
            }
        }

        private void SetParam(string param)
        {
            List<string> values = new List<string>()
            {
            "TimeBegin" + "=" + StartTime.Text,
            "TimeEnd" +"="+EndTime.Text,
            "Atm"+"="+atmType.Text,
            "Number"+"="+atmNumber.Text
            };


            File.WriteAllLines(param, values.ToArray());
        }
        private void EjournalGenerateButton(object sender, RoutedEventArgs e)
        {
            // Создаем файл ej.txt
            if (CreatingEjournalFile())
            {
                // Если все нормально, то обрабатываем ej.txt
                //ProcessingEjournalFile();
            }
        }
        private void ErlGenerateButton(object sender, RoutedEventArgs e)
        {

            var files = Directory.GetFiles(excel_from);
            bool exists = false;
            foreach (var item in files)
            {
                if (Regex.IsMatch(item, @"\.erl|\.ERL"))
                { exists = true; break; }
            }

            if (exists)
            {
                GenerateErlExcel erl = new GenerateErlExcel(excel_from, excel_to + erlp);

                try
                {
                    Process.Start(path + erlp);
                }
                catch
                {
                    MessageBox.Show("Файл erl.xlsx не создан.");
                }
            }
            else
            {
                MessageBox.Show("Не найден файл .erl.");
            }
        }

        private void FullErlGenerateButton(object sender, RoutedEventArgs e)
        {
            var ej = new EJDownloader(targetPath: this.path);
            ej.CreateEjFromPrj();
        }

        private bool CreatingEjournalFile()
        {
            try
            {
                SetParam(set_path);
                var culture = CultureInfo.CreateSpecificCulture("ru-RU");

                var start = DateTime.Parse(StartTime.Text, culture);
                var end = DateTime.Parse(EndTime.Text, culture);

                if (start <= end)
                {
                    if (atmType.Text == "M" || atmType.Text == "T")
                    {
                        if (atmNumber.Text.Length >= 4 && atmNumber.Text.Length <= 6)
                        {
                            EJDownloader downloader = new EJDownloader(atmNumber.Text, atmType.Text, start, end, path + ejp);
                            downloader.CreateEjFile();
                            return true;
                        }
                        else
                        {
                            MessageBox.Show("Неверно задан номер устройства.");
                        }
                    }
                    else { MessageBox.Show("Неверное указан тип устройства."); }
                }
                else
                {
                    MessageBox.Show("Дата начала меньше даты окончания.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\nФайл ej.txt не был создан.");
            }
            return false;
        }
        private void EjGenerateCountsButton(object sender, RoutedEventArgs e)
        {
            //EJournal ej = new EJournal(path + "\\tej.txt");            
        }

        private void FullExcelGenerateButton(object sender, RoutedEventArgs e)
        {
            // Ejournal

            if (File.Exists(excel_from + pdisp) || File.Exists(excel_from + pdepo))
            {
                if (File.Exists(path + ejp))
                {
                    GenerateEJExcel ej = new GenerateEJExcel(path + ejp, excel_to + exc_out, excel_from + pdisp, excel_from + pdepo);

                    try
                    {
                        Process.Start(excel_to + exc_out);
                    }
                    catch
                    {
                        MessageBox.Show("Файл excel.xslx не создан.");
                    }
                }
            }
            else
            {
                MessageBox.Show("Отсутствуют файлы depo.xlsx и disp.xslx.");
            }
        }

        private void ChartsButton_Click(object sender, RoutedEventArgs e)
        {
            string dirs = @"d:\temp\dirs";
            //InfoBox.
            TasksBase tb = new TasksBase(dirs);

            var res = tb.ListOfLastWeek();

            InfoBox.Text = "" ;

            foreach ( var item in res )
            {
                InfoBox.Text += item;
            }

            var lst = tb.ListOfTodayWork();

            foreach ( var item in lst)
            {
                InfoBox.Text += item;
            }


            //InfoBox.Children.OfType<Canvas>().ToList().ForEach(p => InfoBox.Children.Remove(p));

            //Chart chart = null;

            //Button button = sender as Button;

            //// Создаём новый график выбранного вида.
            //switch (button.Name)
            //{
            //    case "BarsButton":
            //        if ((chart is BarChart) == false)
            //        {
            //            chart = new BarChart();
            //        }

            //        break;
            //    case "LineButton":
            //        if ((chart is LineChart) == false)
            //        {
            //            chart = new LineChart();
            //        }

            //        break;
            //    case "PieButton":
            //        if ((chart is PieChart) == false)
            //        {
            //            chart = new PieChart();
            //        }

            //        break;
            //}

            //// Добавляем новую диаграмму на поле контейнера для графиков.
            //InfoBox.Children.Add(chart.ChartBackground);

            //// Принудительно обновляем размеры контейнера для графика.
            //InfoBox.UpdateLayout();

            //// Создаём график.
            //CreateChart(chart);
        }
        private static void CreateChart(Chart chart)
        {
            chart.Clear();

            Random random = new Random();

            for (int i = 0; i < random.Next(1, 25); i++)
            {
                chart.AddValue(random.Next(0, 2001));
            }
        }
    }
}
