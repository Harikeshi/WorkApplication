using EJ.logic.ej_xlsx.pre_ej;

namespace EJ.logic.ej_xlsx
{
    public class GenerateEJExcel
    {         
        public GenerateEJExcel(string path_in, string path_out, string disp, string depo )
        {
            // Формируем Журнал операций
            RawEJournal ej = new RawEJournal(path_in);


            StructureForExcel ejc = new StructureForExcel(ej, disp, depo);

            DepoDispEjournalForExcelLists lst = new DepoDispEjournalForExcelLists(ejc);

            ExcelGeneratorForEJournalClient eg = new ExcelGeneratorForEJournalClient(lst, path_out);
        }
    }
}
