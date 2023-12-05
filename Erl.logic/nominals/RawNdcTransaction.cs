using System;
using System.Collections.Generic;

namespace Erl.logic.nominals
{     
    class RawNdcTransaction
    {
        public string Card { get; set; }
        public DateTime Time { get; set; }
        public TransactionType Type { get; set; }

        public List<NominalList> cdms = new List<NominalList>();
        public List<SortedDictionary<int, int>> bims = new List<SortedDictionary<int, int>>();
        public NominalList disp = null;
        
        public RawNdcTransaction(DateTime time, string card, TransactionType type = TransactionType.Client)
        {
            Time = time;
            Card = card;
            Type = type;
        }
    }
}
