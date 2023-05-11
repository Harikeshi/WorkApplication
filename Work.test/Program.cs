using EJ.logic.ej_xlsx;
using Erl.logic.nominals;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Work.test
{
    internal class Program
    {
        static void Main(string[] args)
        {

            Console.OutputEncoding = Encoding.UTF8;

            string path_in = @"d:\Projects\work\actual\ej.txt";
            string path_out = @"d:\Projects\work\actual\excel.xlsx";
            string epath_in = @"d:\Projects\work\actual\";
            string epath_out = @"d:\Projects\work\actual\erl.xlsx";

            string dep = @"d:\Projects\work\actual\depo.xlsx";
            string dis = @"d:\Projects\work\actual\disp.xlsx";

            EJGenerateTest(path_in, path_out, dep, dis);

            //GenerateErlExcel exl = new GenerateErlExcel(epath_in, epath_out);

            Console.WriteLine();
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
