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

            string path_in = @"d:\temp\ej.txt";
            string path_out = @"d:\temp\excel.xlsx";
            string epath_in = @"d:\temp\";
            string epath_out = @"d:\temp\erl.xlsx";

            string dep = @"d:\temp\depo.xlsx";
            string dis = @"d:\temp\disp.xlsx";

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
