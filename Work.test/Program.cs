using EJ.logic.ej_xlsx;
using Erl.logic.nominals;
using Stat.logic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
//using static Application.Program;

namespace Work.test
{
    internal class Program
    {
        static void Main(string[] args)
        {

            Console.OutputEncoding = Encoding.UTF8;

            string actual = @"c:\Users\sshch\OneDrive\VisualStudio\WorkApplication\";

            string path_in = actual + "ej.txt";
            string path_out = actual + "excel.xlsx";
            string epath_in = actual;
            string epath_out = actual + "erl.xlsx";

            string dep = actual + "depo.xlsx";
            string dis = actual + "disp.xlsx";

            //EJGenerateTest(path_in, path_out, dep, dis);

            //GenerateErlExcel exl = new GenerateErlExcel(epath_in, epath_out);

            //Process.Start("microsoft-edge:http://www.bing.com");

            string dirs = @"d:\temp\dirs";

            TasksBase tb = new TasksBase(dirs, dirs);




            tb.ShowTodayWork();
            


        }

        private static void EJGenerateTest(string path_in, string path_out, string dep, string dis)
        {
            if (File.Exists(dep) || File.Exists(dis))
            {
                GenerateEJExcel ex = new GenerateEJExcel(path_in, path_out, dis, dep);

                System.Console.WriteLine("OK");
            }
        }


    }
}
