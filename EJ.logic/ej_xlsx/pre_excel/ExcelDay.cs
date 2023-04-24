using EJ.logic.ej_xlsx.xlsx;
using System;
using System.Collections.Generic;

namespace EJ.logic.ej_xlsx.pre_excel
{
    class ExcelDay
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public List<ExcelString> Clients = new List<ExcelString>();

        public void Add(ExcelString line)
        {
            Clients.Add(line);
        }
    }
}
