using EJ.logic.ej_xlsx.pre_ej;
using System.Collections.Generic;

namespace EJ.logic.ej_xlsx
{
    class OperDaysTimes : List<DateTimeNominals> { }
    class EJournalDays : List<EJournalDay> { }
    class Pair
    {
        public int Key { get; set; }
        public int Value { get; set; }
        public Pair(int k, int v)
        {
            Key = k;
            Value = v;
        }
    }
    class NominalList : List<Pair>
    {
        public NominalList()
        {
            Add(new Pair(10, 0));
            Add(new Pair(50, 0));
            Add(new Pair(100, 0));
            Add(new Pair(200, 0));
            Add(new Pair(500, 0));
            Add(new Pair(1000, 0));
            Add(new Pair(2000, 0));
            Add(new Pair(5000, 0));
        }
        public List<int> ToList()
        {
            List<int> list = new List<int>();
            foreach (var item in this)
            {
                list.Add(item.Value);
            }
            return list;
        }
    }

}
