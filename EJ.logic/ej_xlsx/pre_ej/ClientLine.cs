using System;
using System.Collections.Generic;

namespace EJ.logic.ej_xlsx
{
    class ClientLine
    {
        public DateTime Time { get; set; }
        public int Number { get; set; }
        public string Card { get; set; }
        public int Sum { get; set; }
        public List<int> Counts = new List<int>();
        public List<string> Comments = new List<string>();
        public Part.PartType Type { get; set; }
    }
}
