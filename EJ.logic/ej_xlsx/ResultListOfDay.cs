using EJ.logic.ej_xlsx.pre_ej;
using EJ.logic.ej_xlsx.xlsx;
using System.Collections.Generic;
using System.Linq;

namespace EJ.logic.ej_xlsx
{
    class ResultListOfDay : List<ExcelResultString>
    {

        private readonly List<SummaryInformation> Summary = new List<SummaryInformation>();
        public ResultListOfDay(List<ClientLine> Ej, List<ExcelString> Exl, Part.PartType type, List<SummaryInformation> Summary = null)
        {
            this.Summary = Summary;
            // Так себе проверка

            Initialize(Ej, Exl, type);

        }
        public void InitializeOld(List<ClientLine> Ej, List<ExcelString> Exl, Part.PartType type)
        {
            int i = 0; int j = 0;

            while (i < Exl.Count && j < Ej.Count)
            {
                if (Exl[i].Number == Ej[j].Number)
                {
                    var sum = Ej[j].Sum; //(Ej[j].Sum != 0) ? Ej[j].Sum : Ej[j].GetSum();
                    ExcelResultString line = new ExcelResultString
                    {
                        Time = Exl[i].Time.ToString(),
                        Number = Exl[i].Number,
                        Card = Exl[i].Card,
                        Amount1 = Exl[i].Amount,
                        Equal = sum - Exl[i].Amount,
                        Amount2 = sum,
                        Counts = Ej[j].Counts,
                        Comment = Ej[j].Comments
                    };
                    this.Add(line);
                    ++i; ++j;
                }
                else if (Exl[i].Number > Ej[j].Number)
                {
                    while ((j < Ej.Count()) && (Exl[i].Number > Ej[j].Number))
                    {
                        var sum = Ej[j].Sum;//(Ej[j].Sum != 0) ? Ej[j].Sum : Ej[j].GetSum();
                        ExcelResultString line = new ExcelResultString
                        {
                            Time = Ej[j].Time.ToString(),
                            Number = -1 * Ej[j].Number, // Добавляем минус к номеру если есть
                            Card = Ej[j].Card,
                            Amount1 = 0,
                            Equal = sum,
                            Amount2 = sum,
                            Counts = Ej[j].Counts,
                            Comment = Ej[j].Comments
                        };
                        this.Add(line);
                        ++j;
                    }
                }
                else if (Exl[i].Number < Ej[j].Number)
                {
                    while ((i < Exl.Count()) && (Exl[i].Number < Ej[j].Number))
                    {
                        ExcelResultString line = new ExcelResultString
                        {
                            Time = Exl[i].Time.ToString(),
                            Number = Exl[i].Number,
                            Card = Exl[i].Card,
                            Amount1 = Exl[i].Amount,
                            Equal = Exl[i].Amount,
                            Amount2 = 0,
                            Counts = (type == Part.PartType.Dispense ? new List<int> { 0, 0, 0, 0 } : new List<int> { 0, 0, 0, 0, 0, 0, 0, 0 }),//NominalList().ToList(),
                            Comment = new List<string> { Exl[i].TrxType + " " + Exl[i].MsgType }
                        };
                        // TODO: Добавить комментерий из Excel
                        this.Add(line);//8                       
                        ++i;
                    }
                }
            }
            // Доходим конец списка.
            // TODO: Найти точку, когда остановить
            while (i < Exl.Count)
            {
                ExcelResultString line = new ExcelResultString
                {
                    Time = Exl[i].Time.ToString(),
                    Number = Exl[i].Number,
                    Card = Exl[i].Card,
                    Amount1 = Exl[i].Amount,
                    Equal = Exl[i].Amount,
                    Amount2 = 0,
                    Counts = (type == Part.PartType.Dispense ? new List<int> { 0, 0, 0, 0 } : new List<int> { 0, 0, 0, 0, 0, 0, 0, 0 }),
                    Comment = new List<string> { Exl[i].TrxType + " " + Exl[i].MsgType }
                };
                this.Add(line);
                ++i;
            }

            while (j < Ej.Count)
            {
                //if (Exl[i - 1].Number < Ej[j].Number || Ej[j].Number == 0)
                //{
                ExcelResultString line = new ExcelResultString
                {
                    Time = Ej[j].Time.ToString(),
                    Number = -1 * Ej[j].Number, // Добавляем минус к номеру если есть
                    Card = Ej[j].Card,
                    Amount1 = 0,
                    Equal = Ej[j].Sum, //.GetSum(),
                    Amount2 = Ej[j].Sum, //GetSum(),
                    Counts = Ej[j].Counts,//(type == Part.PartType.Dispense ? new List<int> { 0, 0, 0, 0 } : new List<int> {0, 0, 0, 0, 0, 0 }),
                    Comment = Ej[j].Comments
                };
                this.Add(line);
                //}
                ++j;
            }
        }

        public void Initialize(List<ClientLine> Ej, List<ExcelString> Exl, Part.PartType type)
        {
            int i = 0; int j = 0;

            while (i < Exl.Count && j < Ej.Count)
            {
                if (Exl[i].Number == Ej[j].Number)
                {
                    var sum = Ej[j].Sum; //(Ej[j].Sum != 0) ? Ej[j].Sum : Ej[j].GetSum();
                    ExcelResultString line = new ExcelResultString
                    {
                        Time = Exl[i].Time.ToString(),
                        Number = Exl[i].Number,
                        Card = Exl[i].Card,
                        Amount1 = Exl[i].Amount,
                        Equal = sum - Exl[i].Amount,
                        Amount2 = sum,
                        Counts = Ej[j].Counts,
                        Comment = Ej[j].Comments
                    };
                    this.Add(line);
                    ++i; ++j;
                }
                else if (Exl[i].Time > Ej[j].Time)
                {
                    var sum = Ej[j].Sum;//(Ej[j].Sum != 0) ? Ej[j].Sum : Ej[j].GetSum();
                    ExcelResultString line = new ExcelResultString
                    {
                        Time = Ej[j].Time.ToString(),
                        Number = -1 * Ej[j].Number, // Добавляем минус к номеру если есть
                        Card = Ej[j].Card,
                        Amount1 = 0,
                        Equal = sum,
                        Amount2 = sum,
                        Counts = Ej[j].Counts,
                        Comment = Ej[j].Comments
                    };
                    this.Add(line);
                    ++j;
                }
                else if (Exl[i].Time < Ej[j].Time)
                {
                    ExcelResultString line = new ExcelResultString
                    {
                        Time = Exl[i].Time.ToString(),
                        Number = Exl[i].Number,
                        Card = Exl[i].Card,
                        Amount1 = Exl[i].Amount,
                        Equal = Exl[i].Amount,
                        Amount2 = 0,
                        Counts = (type == Part.PartType.Dispense ? new List<int> { 0, 0, 0, 0 } : new List<int> { 0, 0, 0, 0, 0, 0, 0, 0 }),//NominalList().ToList(),
                        Comment = new List<string> { Exl[i].TrxType + " " + Exl[i].MsgType }
                    };
                    // TODO: Добавить комментерий из Excel
                    this.Add(line);//8                       
                    ++i;
                }
            }

            // Так как выход по окончанию одного из списков, требуется проход оставшегося
            // TODO: Найти точку, когда остановить
            while (i < Exl.Count)
            {
                ExcelResultString line = new ExcelResultString
                {
                    Time = Exl[i].Time.ToString(),
                    Number = Exl[i].Number,
                    Card = Exl[i].Card,
                    Amount1 = Exl[i].Amount,
                    Equal = Exl[i].Amount,
                    Amount2 = 0,
                    Counts = (type == Part.PartType.Dispense ? new List<int> { 0, 0, 0, 0 } : new List<int> { 0, 0, 0, 0, 0, 0, 0, 0 }),
                    Comment = new List<string> { Exl[i].TrxType + " " + Exl[i].MsgType }
                };
                this.Add(line);
                ++i;
            }

            while (j < Ej.Count)
            {
                //if (Exl[i - 1].Number < Ej[j].Number || Ej[j].Number == 0)
                //{
                ExcelResultString line = new ExcelResultString
                {
                    Time = Ej[j].Time.ToString(),
                    Number = -1 * Ej[j].Number, // Добавляем минус к номеру если есть
                    Card = Ej[j].Card,
                    Amount1 = 0,
                    Equal = Ej[j].Sum, //.GetSum(),
                    Amount2 = Ej[j].Sum, //GetSum(),
                    Counts = Ej[j].Counts,//(type == Part.PartType.Dispense ? new List<int> { 0, 0, 0, 0 } : new List<int> {0, 0, 0, 0, 0, 0 }),
                    Comment = Ej[j].Comments
                };
                this.Add(line);
                //}
                ++j;
            }
        }
    }
}
