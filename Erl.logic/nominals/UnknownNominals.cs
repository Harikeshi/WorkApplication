using System;

namespace Erl.logic.nominals
{
    class UnknownNominal
    {
        public DateTime Time { get; set; }
        public int Nominal { get; set; }
        public int Count { get; set; }
        public UnknownNominal(DateTime time, int nom, int count)
        {
            Time = time;
            Nominal = nom;
            Count = count;
        }
    }
}
