using System;
using System.Collections.Generic;

namespace Erl.logic.nominals
{
    class LineForExcel
    {
        public DateTime Time { get; set; }
        public string Card { get; set; }
        public List<int> Counts = new List<int>();

        public List<int> cdm = new List<int>();
    }
}
