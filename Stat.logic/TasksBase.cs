using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Stat.logic
{
    public class TasksBase
    {
        string path = "";// @"A:\Акты технической экспертизы";
        List<string> ExceptDirs = new List<string>() { "Архив", "шаблоны" };

        // Создать список овнеров
        Dictionary<string, string> workers = new Dictionary<string, string>();
        List<TaskInfo> context = new List<TaskInfo>();

        public int Count { get; set; }

        public TasksBase(string path = @"A:\Акты технической экспертизы\")
        {
            this.path = path;
            Initialize();
            Count = context.Count;

            Console.WriteLine(Count);
            foreach (var x in workers)
            {
                Console.WriteLine(x);
            }
        }


        public void TodayWork()
        {
            var start = DateTime.Today;
            var end = start + TimeSpan.FromDays(1);
            WorkersStat(start, end);
        }
        public void WorkersStat(DateTime start, DateTime end)
        {
            var s = GetWorkersStat(start, end);

            foreach (var i in s)
            {
                Console.WriteLine(i.Key + " - " + i.Value);
            }
        }

        public void WorkersStatFor(int d1, int m1, int d2, int m2, int y1 = 2023, int y2 = 2023)
        {
            var date1 = new DateTime(y1, m1, d1, 0, 0, 0);
            var date2 = new DateTime(y2, m2, d2, 0, 0, 0);

            var s = GetWorkersStat(date1, date2);

            foreach (var i in s)
            {
                Console.WriteLine(i.Key + " - " + i.Value);
            }
        }

        public void ShowLastWeek()
        {
            //// TODO: сделать оопешно по списку работников
            //var ssh = StatWeekOfPerson(@"PLC\schegolihin");
            //var esv = StatWeekOfPerson(@"PLC\ershov");
            //var sa = StatWeekOfPerson(@"PLC\safronov");
            //// Dictionary
            //Dictionary<List<DateQuantities>, int> lst = new Dictionary<List<DateQuantities>, int>();

            //foreach (var item in workers)
            //{
            //    lst.Add(StatWeekOfPerson(@"PLC\" + item), 0);
            //}

            //// Header
            //Console.Write(new string(' ', 20));
            //foreach (var item in workers)
            //{
            //    Console.Write("{0, 3}", item);
            //}
            //Console.WriteLine();

            //for(int i = 0; i< lst[0].Count; ++i)
            //{
            //    foreach(var item in lst)
            //    {
            //        item[i].Date + ": " + "{0}"
            //    }
            //}

        }




        private void Initialize()
        {
            GetAllDirectories(path, context);

        }

        private void GetAllDirectories(string path, List<TaskInfo> context)
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
                    if (workers.ContainsKey(acc) == false)
                    {
                        workers.Add(acc, "");
                    }

                    context.Add(new TaskInfo(acc, di.CreationTime, di.FullName.Split('\\')[2]));
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

            foreach (var worker in context)
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

        public void xxx()
        {



        }

        // Топ самых проблемных атм
        // Создать сортированный список из атм

        internal class AtmDateQuantities
        {
            public string Name { get; set; }
            Dictionary<DateTime, int> DayQuantities { get; set; }
        }

        SortedDictionary<string, SortedDictionary<DateTime, int>> xx(DateTime start, DateTime end)
        {
            // atm - Dictionary<date, quantities>
            SortedDictionary<string, SortedDictionary<DateTime, int>> atms = new SortedDictionary<string, SortedDictionary<DateTime, int>>();
            foreach (var item in context)
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
