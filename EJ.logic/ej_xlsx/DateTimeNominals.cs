using EJ.logic.ej_xlsx.pre_ej;
using System;
using System.Collections.Generic;

namespace EJ.logic.ej_xlsx
{
    class DateTimeNominals
    {
        public DateTime Time { get; set; }
        public List<Nominal> Nominals = new List<Nominal>();
        public DateTimeNominals(DateTime time, List<Nominal> nom)
        {
            Time = time;
            Nominals = nom;
        }
    }
}
