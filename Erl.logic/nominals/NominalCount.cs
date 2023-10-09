using System;

namespace Erl.logic.nominals
{
    class NominalCount : IEquatable<NominalCount>, IComparable<NominalCount>
    {
        public int Nominal { get; set; }
        public int Count { get; set; }
        public NominalCount(int nominal, int count)
        {
            Nominal = nominal;
            Count = count;
        }

        public bool Equals(NominalCount other)
        {
            if (other == null) return false;
            return (this.Nominal.Equals(other.Nominal));
        }

        public int CompareTo(NominalCount other)
        {
            if (other == null)
                return 1;

            else
                return this.Nominal.CompareTo(other.Nominal);
        }
    }
}
