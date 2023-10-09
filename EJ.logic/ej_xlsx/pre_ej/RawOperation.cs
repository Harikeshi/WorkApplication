using System.Collections.Generic;

namespace EJ.logic.ej_xlsx.pre_ej
{
    class RawOperation
    {
        public OperationType Type { get; set; }
        private readonly List<string> lines = new List<string>();

        public void Add(string line) { lines.Add(line); }
        public int Size() { return lines.Count; }
        public void AddRange(List<string> lst) { lines.AddRange(lst); }
        public List<string> GetLines() { return lines; }
        public void Clear() { lines.Clear(); }
    }
}
