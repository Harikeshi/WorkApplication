using EJ.logic.ej_xlsx.pre_ej;
using EJ.logic.ej_xlsx.xlsx;
using System;
using System.Collections.Generic;

namespace EJ.logic.ej_xlsx
{
    class OperDay
    {
        public DateTime Begin { get; set; }
        public DateTime End { get; set; }
        public List<Nominal> Nominals { get; set; }
        // excel - ejournal        
        public List<ClientLine> ej_di = new List<ClientLine>();
        public List<ClientLine> ej_de = new List<ClientLine>();
        public List<ExcelString> exc_di = new List<ExcelString>();
        public List<ExcelString> exc_de = new List<ExcelString>();
    }
}
