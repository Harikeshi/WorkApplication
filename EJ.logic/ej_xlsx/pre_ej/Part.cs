using System;
using System.Collections.Generic;

namespace EJ.logic.ej_xlsx
{
    class Part
    {
        public DateTime Time { get; set; }
        public int Number { get; set; }
        public Dictionary<string, string> Infos = new Dictionary<string, string>();
        public List<int> Counts = new List<int>(); // Купюры одинаково для выдачи и приема, только разной длины массивы
        public int Sum { get; set; }
        public List<string> Comments = new List<string>();
        public List<string> Errors = new List<string>();

        public PartType Type = PartType.Other;

        public enum PartType
        {
            Dispense,
            Deposite,
            Other
        }
    }
}
