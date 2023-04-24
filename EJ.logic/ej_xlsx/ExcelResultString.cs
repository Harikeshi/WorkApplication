using System.Collections.Generic;

namespace EJ.logic.ej_xlsx
{
    class ExcelResultString
    {
        public string Time { get; set; }
        public int Number { get; set; }
        public string Card { get; set; }
        public int Amount1 { get; set; }
        public int Equal { get; set; }
        public int Amount2 { get; set; }
        public List<int> Counts { get; set; }
        public List<string> Comment { get; set; }
    }
}
