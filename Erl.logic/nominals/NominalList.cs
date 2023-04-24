using System;
using System.Collections.Generic;

namespace Erl.logic.nominals
{
    class NominalList : List<NominalCount>
    {
        public DateTime Time { get; set; }
        public NominalList(DateTime time)
        {
            Time = time;
        }

        public void Add(int nom, int count)
        {
            this.Add(new NominalCount(nom, count));
        }
        public NominalList DictionaryToList(SortedDictionary<int, int> dict)
        {
            foreach (var kvp in dict)
            {
                this.Add(new NominalCount(kvp.Key, kvp.Value));
            }
            return this;
        }
        public SortedDictionary<int, int> ListToDictionary()
        {
            SortedDictionary<int, int> dict = new SortedDictionary<int, int>();

            foreach (var kvp in this)
            {
                dict.Add(kvp.Nominal, kvp.Count);
            }
            return dict;
        }
        public bool IsNoEqual(NominalList lst)
        {
            if (this.Count != lst.Count) return true;
            for (int i = 0; i < this.Count; ++i)
            {
                if (lst[i].Count != this[i].Count) return true;
            }

            return false;
        }
        public bool IsPositive()
        {
            foreach (var item in this)
            {
                if (item.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }
        public bool IsNegative()
        {
            foreach (var item in this)
            {
                if (item.Count < 0)
                {
                    return true;
                }
            }
            return false;
        }
        public bool IsNull()
        {
            foreach (var item in this)
            {
                if (item.Count != 0)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
