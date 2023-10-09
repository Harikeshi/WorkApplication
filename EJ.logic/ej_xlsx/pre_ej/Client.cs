using System;
using System.Collections.Generic;

namespace EJ.logic.ej_xlsx
{
    class Client
    {
        public string Card { get; set; }
        public List<Part> Parts = new List<Part>();
        public DateTime Time { get; set; } // Время начала транзакции
        public bool IsHad = false;
        public Dictionary<string, string> dict = new Dictionary<string, string>();// По хорошему словарь 
        public List<string> Other = new List<string>(); // Строки которые не вошли.
    }
}
