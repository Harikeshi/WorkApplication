using System;
using System.Collections.Generic;

namespace Erl.logic.nominals
{
    class LineStructure
    {
        public NominalList cdm = null;
        public SortedDictionary<int, int> bim = null;
        public NominalList disp = null;
        public DateTime Time { get; set; }
        public string Card { get; set; }
        public bool IsIncass { get; set; }
        public LineStructure()
        {
            IsIncass = false;
        }
    }
}
