using System;
using System.Collections.Generic;

namespace EJ.logic.ej_xlsx.pre_ej
{
    class EJournalDay
    {
        public DateTime Begin { get; set; }
        public DateTime End { get; set; }
        public List<Nominal> Nominals = new List<Nominal>();
        public List<ClientLine> Disp = new List<ClientLine>();
        public List<ClientLine> Depo = new List<ClientLine>();
    }
}
