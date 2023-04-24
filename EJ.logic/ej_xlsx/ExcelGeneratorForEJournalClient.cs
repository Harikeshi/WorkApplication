using EJ.logic.ej_xlsx.xlsx;
using System.IO;

namespace EJ.logic.ej_xlsx
{
    class ExcelGeneratorForEJournalClient
    {
        public ExcelGeneratorForEJournalClient(DepoDispEjournalForExcelLists lst, string path)
        {
            if (lst.Disps != null && lst.Depos != null)
            {
                var data = new ExcelGeneratorForEJournal().Generate(lst.Disps, lst.Head.dispense, "Dispense");
                File.WriteAllBytes(path, data);
                new ExcelGeneratorForEJournal().Insert(path, lst.Depos, lst.Head.deposit, "Deposit");

            }
            else if (lst.Disps != null)
            {
                var data = new ExcelGeneratorForEJournal().Generate(lst.Disps, lst.Head.dispense, "Dispense");
                File.WriteAllBytes(path, data);
            }
            else
            {
                var data = new ExcelGeneratorForEJournal().Generate(lst.Depos, lst.Head.deposit, "Deposite");
                File.WriteAllBytes(path, data);
            }
        }
    }
}
