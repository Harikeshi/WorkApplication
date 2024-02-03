using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Table;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
//using static OfficeOpenXml.ExcelErrorValue;

namespace Stat.logic
{
    public class TasksBase
    {
        readonly string path;
        readonly string out_path;

        List<string> ExceptDirs = new List<string>() { "Архив", "шаблоны" };

        // Создать список овнеров
        Dictionary<string, string> workers = new Dictionary<string, string>();
        List<TaskInfo> content = new List<TaskInfo>();

        public int Count { get; set; }

        public TasksBase(string path, string o_path)
        {
            this.path = path;
            this.out_path = o_path;
            Initialize();

            //GetAllOwnerDirs("sshch");
            SaveExcelFile();
        }

        private void MakeWorkers()
        {
            foreach (var c in content)
                if (!workers.ContainsKey(c.owner))
                    workers.Add(c.owner, "");
        }

        

        public void ShowTodayWork()
        {
            var start = DateTime.Today;
            var end = start + TimeSpan.FromDays(1);

            var d = GetWorkersStat(start, end);

            ShowWorkersStat(d);
        }
        public List<string> ListOfTodayWork()
        {
            var start = DateTime.Today;
            var end = start + TimeSpan.FromDays(1);

            var d = GetWorkersStat(start, end);
            List<string> lst = new List<string>();
            foreach (var item in d)
            {
                lst.Add(item.Key + " - " + item.Value + "\n");
            }

            return lst;
        }

        private void ShowWorkersStat(Dictionary<string, int> dict)
        {
            foreach (var i in dict)
            {
                Console.WriteLine(i.Key + " - " + i.Value);
            }
        }

        private Dictionary<string, int> WorkersStatFor(int d1, int m1, int d2, int m2, int y1 = 2023, int y2 = 2023)
        {
            var date1 = new DateTime(y1, m1, d1, 0, 0, 0);
            var date2 = new DateTime(y2, m2, d2, 0, 0, 0);

            return GetWorkersStat(date1, date2);
        }

        private Dictionary<string, List<DateQuantities>> GetStatLastWeek()
        {
            // dict{person - list{dayqua}}
            Dictionary<string, List<DateQuantities>> result = new Dictionary<string, List<DateQuantities>>();
			
            foreach (var w in workers)
            {
                result.Add(w.Key, GetDaysOfLastWeek());
            }
			
			var days = GetDaysOfLastWeek();
			
            foreach (var c in content)
            {
                if (c.Time.Date >= days[0].Date && c.Time.Date <= days[days.Count - 1].Date){
                    	
					for (int i = 0; i < days.Count; ++i)
                    {	
                        if (c.Time.Date == days[i].Date)
                        {
                            result[c.owner][i].Quantity++;
                        }
                    }
				}
            }
			
            return result;
        }

        public List<string> ListOfLastWeek()
        {
            var stat = GetStatLastWeek();
            List<string> lst = new List<string>();
            string first = "";

            // header
            first += new string(' ', 20);
            foreach (var item in workers)
            {
                first += string.Format(" {0,3}", item.Key.Substring(0, 2));
            }
            lst.Add(first + "\n");

            // body
            List<int> total = new List<int>(new int[workers.Count]);

			var days = GetDaysOfLastWeek();
            for(int i =0; i < days.Count; ++i)
            {
                first = days[i].Date.ToShortDateString() + ":";
                int j = 0;
                foreach (var d in stat)
                {
                    first += string.Format(" {0, 3}", d.Value[i].Quantity);
                    total[j] += d.Value[i].Quantity;
					j++;
                }
				lst.Add(first + "\n");                
            }
           
            // feeter
            first = "Total"+ new string(' ', 8) + ":";
            foreach (var t in total)
            {
                first += string.Format(" {0,3}",t);
            }
            lst.Add(first + "\n");
            return lst;
        }
        public void ShowLastWeek()
        {
            var lst = ListOfLastWeek();
            foreach(var i in lst)
            Console.Write(i);         
        }

        public void GetAllOwnerDirs(string owner)
        {
            foreach (var item in content)
            {
                if (item.owner == owner) Console.WriteLine(item.FullPath);
            }
        }

        private void Initialize()
        {
            GetAllDirectories(path);

            Count = content.Count;
        }

        private void GetAllDirectories(string path)
        {
            var d = Directory.GetDirectories(path);

            ConcurrentQueue<string> listFiles = new ConcurrentQueue<string>();

            foreach (var file in d)
            {
                listFiles.Enqueue(file);
            }

            Parallel.ForEach(listFiles, (item =>
            {
                DirectoryInfo di = new DirectoryInfo(item);
                if ((di.Name[0] >= '0' && di.Name[0] <= '9') == false)
                {
                    if(di.Name != "Архив" && di.Name != "шаблоны")
						{				
							GetAllDirectories(item);	
						}	
                }
                else
                {
                    var acc = di.GetAccessControl().GetOwner(typeof(NTAccount)).Value.Split('\\')[1];
                    content.Add(new TaskInfo(acc, di.CreationTime, di.FullName.Split('\\')[3], di.FullName));
                }
            }));
        }
        private List<DateQuantities> StatWeekOfPerson(string owner)
        {
            var days = GetDaysOfLastWeek();
            return null;
        }

        private List<DateQuantities> GetDaysOfLastWeek()
        {
            List<DateQuantities> lst = new List<DateQuantities>();
            for (int i = -7; i < 0; ++i)
            {
                lst.Add(new DateQuantities { Date = DateTime.Today.AddDays(i), Quantity = 0 });
            }

            return lst;
        }

        private Dictionary<string, int> GetWorkersStat(DateTime start, DateTime end)
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            foreach (var worker in workers)
            {
                dict.Add(worker.Key, 0);
            }

            foreach (var worker in content)
            {
                if (worker.Time >= start && worker.Time <= end)
                {
                    dict[worker.owner]++;
                }
                else if (worker.Time > end) break;
            }
            return dict;
        }

        // Количество работ по дням, по месяцам
        class PeriodQuantity
        {
            public int Period { get; set; }
            public int Quantity { get; set; }
            public PeriodQuantity(int i)
            {
                Period = i;
                Quantity = 0;
            }
        }

        // годовая статистика
        private Dictionary<string, Dictionary<int, int>> InitDictionary()
        {
            Dictionary<string, Dictionary<int, int>> result = new Dictionary<string, Dictionary<int, int>>();
            foreach (var w in workers)
                result.Add(w.Key, new Dictionary<int, int>());

            result.Add("Total", new Dictionary<int, int>()); // Total
            return result;
        }
        private Dictionary<string, Dictionary<int, int>> YearsStatistic()
        {
            var result = InitDictionary();
            // По каждому работнику вывести количество

            // Инициализируем по месяцам каждого
            foreach (var r in result)
                for (int i = 0; i < 12; ++i)
                    r.Value.Add(i, 0); // добавляем записи словаря                          

            // По месяцам каждый раб
            foreach (var c in content)
                result[c.owner][c.Time.Month - 1]++;

            // Считаем общее
            foreach (var res in result)
            {
                if (res.Key != "Total")
                    foreach (var v in res.Value)
                        result["Total"][v.Key] += v.Value;
            }


            return result;
        }

        // Топ самых проблемных атм
        // Создать сортированный список из атм

        private Dictionary<string, int> GetAtmListWithQuantity()
        {
            Dictionary<string, int> result = new Dictionary<string, int>();

            foreach (var w in content)
            {
                if (!result.ContainsKey(w.Name))
                {
                    result.Add(w.Name, 0);
                }
                result[w.Name]++;
            }

            return null;
        }



        internal class AtmDateQuantities
        {
            public string Name { get; set; }
            Dictionary<DateTime, int> DayQuantities { get; set; }
        }

        SortedDictionary<string, SortedDictionary<DateTime, int>> xx(DateTime start, DateTime end)
        {
            // atm - Dictionary<date, quantities>
            SortedDictionary<string, SortedDictionary<DateTime, int>> atms = new SortedDictionary<string, SortedDictionary<DateTime, int>>();
            foreach (var item in content)
            {
                if (item.Time >= start && item.Time <= end)
                {
                    if (!atms.ContainsKey(item.Name))
                    {
                        atms.Add(item.Name, new SortedDictionary<DateTime, int>());
                    }
                    // Добавить атм и дату
                    if (!atms[item.Name].ContainsKey(item.Time))
                        atms[item.Name].Add((DateTime)item.Time, 0);

                    atms[item.Name][item.Time]++;
                }
                else if (item.Time > end)
                {
                    break;
                }
            }

            return atms;
        }
         private void SaveExcelFile()
        {
            List<string> months = new List<string>{ "jan", "feb", "mar","apr","may","jun","jul",
            "aug", "sep", "oct", "nov", "dec"};

            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            string tableRange = "A1:" + (char)('A' + months.Count) + (char)('1' + workers.Count); 

            ExcelPackage package = new ExcelPackage();
            ExcelWorksheet sheet = package.Workbook.Worksheets.Add("Sheet1");
            ExcelTable table = sheet.Tables.Add(sheet.Cells[tableRange], "Example");

            table.Columns[0].Name = "/";
            for (int i = 0, j =1; i < months.Count; ++i,++j)
                table.Columns[j].Name = months[i];

            // Количество строк workers +1
            int firstDataRowIndex = 2; // first row with data
            int c = firstDataRowIndex;
            foreach (var w in workers)
            {
                // берем каждого работника и по нему добавляем по столбцам.
                sheet.Cells["A" + c.ToString()].Value = w.Key;
                ++c;
            }

            sheet.Cells["A" + (2 + workers.Count).ToString()].Value = "Сумма";
            var stat = YearsStatistic();
            int number = firstDataRowIndex;
            foreach (var s in stat)
            {
                foreach (var w in s.Value)
                    sheet.Cells[(char)('B' + w.Key) + number.ToString()].Value = w.Value;
                number++;
            }

            // Chart
            OfficeOpenXml.Drawing.Chart.ExcelChart chart = sheet.Drawings.AddChart("example", eChartType.ColumnClustered);

            // Размеры окна, настройки осей
            chart.XAxis.Title.Text = "Месяц";
            chart.XAxis.Title.Font.Size = 10;
            chart.YAxis.Title.Text = "Количество";
            chart.YAxis.Title.Font.Size = 10;
            chart.SetSize(500, 300);
            chart.SetPosition(0, 0, 4, 0);

            string xValuesRange = "A1:M1"; //
            for (int i = 2; i <= workers.Count + 2; ++i)
            {

                var series = chart.Series.Add("B" + i.ToString() + ":M" + i.ToString(), xValuesRange);
                series.Header = sheet.Cells["A" + i.ToString()].Value.ToString(); // Series title
            }
            chart.Legend.Position = eLegendPosition.Right;

            //automatically adjust columns width to text

            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
            package.SaveAs(new FileInfo(Path.Combine(out_path, "example.xlsx")));
        }

        public class WorkerStat
        {
            public string Name { get; set; }

            public List<DateQuantities> Days { get; set; }
            public DateTime Date { get; set; }
            public int Quantity { get; set; }
        }
        public class DateQuantities
        {
            public DateTime Date { get; set; }
            public int Quantity { get; set; }
        }
        public class AtmStat
        {
            public string Name { get; set; }
            public int Quantity { get; set; }
        }
    }
}
