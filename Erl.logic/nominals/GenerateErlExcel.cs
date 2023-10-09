using System;
using System.IO;

namespace Erl.logic.nominals
{
    public class GenerateErlExcel
    {
        public GenerateErlExcel(string path_in, string path_out)
        {
            RawNdcTransactionsList ndc = new RawNdcTransactionsList(path_in);

            LineStructureList diff = new LineStructureList(ndc);//.Init(ndc);

            var lst = new DifferenceListForExcel(diff);


            var data = new ExcelGeneratorErl().Generate(lst);

            File.WriteAllBytes(path_out, data);
            
            ShowErl(lst);
        }

        private static void ShowErl(DifferenceListForExcel lst)
        {
            foreach (var item in lst.Header)
            {
                Console.Write(item + " ");
            }
            Console.WriteLine();
            foreach (var item in lst)
            {
                Console.Write(item.Time + " " + item.Card + " ");
                foreach (var c in item.Counts)
                {
                    Console.Write(c + ",");
                }
                Console.WriteLine();
            }
        }
    }
}
