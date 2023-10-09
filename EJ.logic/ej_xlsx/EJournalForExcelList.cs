using System.Collections.Generic;

namespace EJ.logic.ej_xlsx
{
    class DepoDispEjournalForExcelLists
    {
        public Header Head = new Header();
        public List<ResultListOfDay> Depos = null;
        public List<ResultListOfDay> Disps = null;
        public DepoDispEjournalForExcelLists(StructureForExcel client)
        {

            bool disp = true;
            bool depo = true;
            foreach (var item in client.Days)
            {
                if (item.ej_di.Count > 0 || item.exc_di.Count > 0)
                {
                    if (disp)
                    {
                        this.Disps = new List<ResultListOfDay>();
                        disp = false;
                    }
                    this.Disps.Add(new ResultListOfDay(item.ej_di, item.exc_di, Part.PartType.Dispense));
                }

                if (item.ej_de.Count > 0 || item.exc_de.Count > 0)
                {
                    if (depo)
                    {
                        this.Depos = new List<ResultListOfDay>();
                        depo = false;
                    }

                    this.Depos.Add(new ResultListOfDay(item.ej_de, item.exc_de, Part.PartType.Deposite));
                }
            }

            InitHeader(client);
        }
        private void InitHeader(StructureForExcel client)
        {
            Head.info = new List<string> { "Term time", "Number", "Card", "Amount1", "Equal", "Amount2", "" };


            if (Depos != null)
            {
                var nom = new NominalList();
                foreach (var s in nom)
                {
                    Head.deposit.Add(s.Key.ToString());
                }
            }
            if (Disps != null)
            {
                foreach (var s in client.GetNominals())
                {
                    Head.dispense.Add(s.Value.ToString());
                }
            }
        }
    }
}
