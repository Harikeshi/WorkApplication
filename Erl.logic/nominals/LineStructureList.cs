using System.Collections.Generic;
using System.Linq;

namespace Erl.logic.nominals
{

    class LineStructureList : List<LineStructure>
    {
        public SortedDictionary<string, UnknownNominal> UnKnowns;// = new SortedDictionary<string, UnknownNominal>();
        private NominalList DifferenceOfNominalLists(NominalList cur, NominalList last)
        {
            if (cur.Count != last.Count)
            {
                return cur.Count > last.Count ? cur : last;
            }
            NominalList lst = new NominalList(cur.Time);

            for (int i = 0; i < last.Count; ++i)
            {
                lst.Add(new NominalCount(last[i].Nominal, cur[i].Count - last[i].Count));
            }

            return lst;
        }
        private SortedDictionary<int, int> DifferenceOfDictionaries(SortedDictionary<int, int> cur, SortedDictionary<int, int> last)
        {
            SortedDictionary<int, int> dict = new SortedDictionary<int, int>();
            foreach (var item in cur)
            {
                dict.Add(item.Key, item.Value);
            }

            foreach (var item in last)
            {
                if (dict.ContainsKey(item.Key))
                {
                    dict[item.Key] -= item.Value;
                }
                else
                {
                    dict.Add(item.Key, item.Value);
                }
            }

            return dict;
        }
        private bool IsLineNoNull(LineStructure line)
        {
            if (line.cdm != null)
                foreach (var item in line.cdm)
                {
                    if (item.Count != 0) return true;
                }
            if (line.bim != null)
                foreach (var item in line.bim)
                {
                    if (item.Value != 0) return true;
                }
            return false;
        }
        private SortedDictionary<int, int> FirstBim(RawNdcTransactionsList erls)
        {
            foreach (var item in erls)
            {
                if (item.bims.Count > 0)
                {
                    return item.bims[0];
                }
            }
            return null;
        }

        private NominalList FirstCdm(RawNdcTransactionsList erls)
        {
            foreach (var item in erls)
            {
                if (item.cdms.Count > 0)
                {
                    return item.cdms[0];
                }
            }
            return null;
        }

        public LineStructureList(RawNdcTransactionsList erls)
        {
            UnKnowns = erls.GetUnKnowns();

            NominalList lastc = FirstCdm(erls);
            SortedDictionary<int, int> lastb = FirstBim(erls);

            int start = 0;
            if (erls[0].Type == TransactionType.Begin) start = 1;

            // TODO: Приводить к эталону после расчетов!
            for (int i = start; i < erls.Count; ++i)
            {
                // Если инкассация Просто создаем строку
                if (erls[i].Type == TransactionType.Incass)
                {
                    LineStructure line = new LineStructure();

                    if (erls[i].cdms.Count > 0)
                    {
                        lastc = erls[i].cdms.First();
                        NominalList cdm = new NominalList(line.Time);
                        foreach (var item in lastc) cdm.Add(item);
                        cdm.Sort();
                        line.cdm = cdm;
                    }
                    if (erls[i].bims.Count > 0)
                    {
                        lastb = erls[i].bims[0];
                        line.bim = erls[i].bims[0];
                    }
                    line.Time = erls[i].Time;
                    line.Card = "Инкассация";
                    line.IsIncass = true;
                    this.Add(line);
                    continue;
                }

                int c = 0; // Счетчик по cash
                int b = 0; // Счетчик по bim
                while (b < erls[i].bims.Count && c < erls[i].cdms.Count) // До конца одного из cash и bim
                {
                    // Создаем новую запись              
                    LineStructure line = new LineStructure();

                    // Находим разность Между двумя номиналами  
                    var cdm = DifferenceOfNominalLists(erls[i].cdms[c], lastc);
                    cdm.Sort();
                    lastc = erls[i].cdms[c];

                    if (cdm.IsPositive() && b < erls[i].bims.Count) // Если bim закончились то только дописываем
                    {
                        // Записываем cdm
                        line.cdm = cdm;

                        var bim = DifferenceOfDictionaries(erls[i].bims[b], lastb);

                        lastb = erls[i].bims[b];
                        line.bim = bim;

                        line.Time = erls[i].cdms[c].Time;
                        line.Card = erls[i].Card;
                        this.Add(line);
                        ++c;
                        ++b;
                    }
                    else
                    {
                        // 2.
                        // Записываем разницу и оставляем остальные пустыми, при выводе проверить и забить нулями                            
                        line.cdm = cdm;
                        if (erls[i].disp != null)
                        {
                            erls[i].disp.Sort();
                            line.disp = erls[i].disp;
                        }
                        if (IsLineNoNull(line))
                        {
                            line.Time = erls[i].cdms[c].Time;
                            line.Card = erls[i].Card;
                            this.Add(line);
                        }
                        ++c;
                    }
                }
                // Дописываем оставшиеся
                if (c < erls[i].cdms.Count)
                {

                    for (; c < erls[i].cdms.Count; ++c)
                    {
                        LineStructure line = new LineStructure();
                        var cdm = DifferenceOfNominalLists(erls[i].cdms[c], lastc);
                        cdm.Sort();
                        line.cdm = cdm;
                        if (erls[i].disp != null)
                        {
                            erls[i].disp.Sort();
                            line.disp = erls[i].disp;
                        }
                        lastc = erls[i].cdms[c];
                        if (IsLineNoNull(line))
                        {
                            line.Time = erls[i].cdms[c].Time;
                            line.Card = erls[i].Card;
                            this.Add(line);
                        }
                    }
                }
                if (b < erls[i].bims.Count)
                {
                    for (; b < erls[i].bims.Count; ++b)
                    {
                        LineStructure line = new LineStructure();
                        if (erls[0].cdms.Count > 0)
                        {
                            NominalList cdm = new NominalList(line.Time);
                            foreach (var item in erls[0].cdms[0]) cdm.Add(item.Nominal, 0);
                            cdm.Sort();
                            line.cdm = cdm;
                        }

                        var bim = DifferenceOfDictionaries(erls[i].bims[b], lastb);
                        line.bim = bim;

                        lastb = erls[i].bims[b];
                        if (IsLineNoNull(line))
                        {
                            line.Time = erls[i].Time; // TODO: Время 
                            line.Card = erls[i].Card;
                            this.Add(line);
                        }
                    }
                }
            }
        }
    }
}
