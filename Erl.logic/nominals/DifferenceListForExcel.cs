using System.Collections.Generic;

namespace Erl.logic.nominals
{
    class DifferenceListForExcel : List<LineForExcel>
    {
        public int cdm_length { get; set; }
        public int bim_length { get; set; }
        public int p_length { get; set; }
        public SortedDictionary<string, UnknownNominal> UnKnowns;
        public ExcelHeader Header = new ExcelHeader();

        public DifferenceListForExcel(LineStructureList lst)
        {
            UnKnowns = lst.UnKnowns;
            // Находим эталонную длину депозитора
            var etalon = DesignEtalons(lst);

            var depo = new SortedDictionary<int, int>();
            foreach (var item in etalon)
            {
                depo.Add(item.Key, item.Value);
            }
            // Находим эталонную длину по приему
            //SortedDictionary<int, int> present = new SortedDictionary<int, int>();
            List<NominalCount> disp = new List<NominalCount>();
            bool first = true;
            // Диспенсер
            foreach (var item in lst)
            {
                if (item.cdm != null)
                {
                    foreach (var cdm in item.cdm)
                    {
                        if (first)
                        {
                            disp.Add(cdm);
                        }
                    }
                    first = false;
                    break;
                }
            }
            foreach (var item in lst)
            {
                if (item.disp != null)
                {
                    p_length = item.disp.Count;
                    break;
                }
            }
            cdm_length = disp.Count;
            bim_length = etalon.Count;

            // Проходим по всем линиям         
            if (cdm_length > 0 & bim_length > 0)
            {
                foreach (var item in lst)
                {
                    LineForExcel line = new LineForExcel();
                    line.Time = item.Time; line.Card = item.Card;

                    if (item.cdm != null)
                    {
                        foreach (var c in item.cdm)
                        {
                            line.Counts.Add(c.Count);
                        }
                    }
                    else
                    {
                        foreach (var c in disp)
                        {
                            line.Counts.Add(0);
                        }
                    }

                    if (item.disp != null)
                    {
                        foreach (var c in item.disp)
                        {
                            line.Counts.Add(c.Count);
                        }
                    }
                    else
                    {
                        for (int l = 0; l < p_length; l++)
                        {
                            line.Counts.Add(0);
                        }
                    }

                    if (item.bim != null)
                    {
                        var sorted = new SortedDictionary<int, int>();
                        foreach (var d in etalon)
                        {
                            if (item.bim.ContainsKey(d.Key))
                            {
                                sorted.Add(d.Key, item.bim[d.Key]);
                            }
                            else
                            {
                                sorted.Add(d.Key, d.Value);
                            }
                        }

                        foreach (var b in sorted)
                        {
                            line.Counts.Add(b.Value);
                        }
                    }
                    else
                    {
                        foreach (var b in etalon)
                        {
                            line.Counts.Add(0);
                        }
                    }

                    this.Add(line);
                }
            }
            else if (cdm_length > 0)
            {
                foreach (var item in lst)
                {
                    LineForExcel line = new LineForExcel();
                    line.Time = item.Time; line.Card = item.Card;

                    if (item.cdm != null)
                    {
                        foreach (var c in item.cdm)
                        {
                            line.Counts.Add(c.Count);
                        }
                    }
                    else
                    {
                        foreach (var c in disp)
                        {
                            line.Counts.Add(0);
                        }
                    }

                    if (item.disp != null)
                    {
                        foreach (var c in item.disp)
                        {
                            line.Counts.Add(c.Count);
                        }
                    }
                    else
                    {
                        for (int l = 0; l < p_length; l++)
                        {
                            line.Counts.Add(0);
                        }
                    }
                    this.Add(line);
                }
            }
            else
            {
                foreach (var item in lst)
                {
                    LineForExcel line = new LineForExcel();
                    line.Time = item.Time; line.Card = item.Card;

                    var sorted = new SortedDictionary<int, int>();
                    foreach (var d in etalon)
                    {
                        if (item.bim.ContainsKey(d.Key))
                        {
                            sorted.Add(d.Key, item.bim[d.Key]);
                        }
                        else
                        {
                            sorted.Add(d.Key, d.Value);
                        }
                    }

                    foreach (var b in sorted)
                    {
                        line.Counts.Add(b.Value);
                    }

                    this.Add(line);
                }
            }

            InitHeader(etalon, depo, disp);
        }

        private void InitHeader(SortedDictionary<int, int> etalon, SortedDictionary<int, int> depo, List<NominalCount> disp)
        {
            foreach (var d in disp)
            {
                if (d.Nominal == 0) { Header.Add("UNK"); }
                else if (d.Nominal == 1) { Header.Add("REJ"); }
                else if (d.Nominal == 2) { Header.Add("RET"); }
                else { Header.Add(d.Nominal.ToString()); }
            }

            if (disp.Count > 0)
            {
                for (int i = 0; i < disp.Count; ++i)
                {
                    if (disp[i].Nominal > 5)
                    {
                        Header.Add(disp[i].Nominal.ToString());

                    }
                }
            }
            if (etalon.Count > 0)
            {
                foreach (var d in depo)
                {
                    Header.Add(d.Key.ToString());
                }
            }
        }

        private SortedDictionary<int, int> DesignEtalons(LineStructureList erls)
        {
            SortedDictionary<int, int> dict = new SortedDictionary<int, int>();

            foreach (var item in erls)
            {
                if (item.bim != null)
                {
                    foreach (var b in item.bim)
                    {
                        if ((!dict.ContainsKey(b.Key)) && b.Value > 0)
                        {
                            dict.Add(b.Key, 0);
                        }
                    }
                }
            }

            return dict;
        }
    }
}
