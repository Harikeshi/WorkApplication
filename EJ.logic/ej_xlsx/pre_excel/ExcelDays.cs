using System;
using System.Collections.Generic;
using EJ.logic.ej_xlsx.xlsx;

namespace EJ.logic.ej_xlsx.pre_excel
{
    class ExcelOperations : List<ExcelString>
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public List<DateTime> Times = new List<DateTime>();
        public ExcelOperations(ExcelDay day)
        {
            Times.Add(day.Start);
            Times.Add(day.End);
        }
        public ExcelOperations(List<ExcelString> lines)
        {
            
            ExcelDay day = new ExcelDay();
            int i = 0;
            this.Add(lines[i]);
            Times.Add(lines[i].Time);
            ++i;

            for (; i < lines.Count; ++i)
            {
                if (lines[i].Number < lines[i - 1].Number)
                {
                    Times.Add(lines[i].Time);
                }
                this.Add(lines[i]);
            }
            Times.Add(lines[i - 1].Time);
        }
    }
}
