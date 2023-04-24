using System;
using System.Collections.Generic;

namespace EJ.logic.ej_xlsx.pre_ej
{
    class Operator
    {
        public DateTime Time { get; set; }
        public OperatorType Type = OperatorType.Terminal;
        public int Number { get; set; }
        public List<Nominal> Nominals = new List<Nominal>();
        public Dictionary<string, int> GetInfornation { get; set; }
        public List<SummaryInformation> FullInfo = new List<SummaryInformation>();
    }
}
