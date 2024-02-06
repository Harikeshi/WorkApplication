using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Erl.logic.nominals
{
    enum TransactionType { Client, Incass, Begin }
    class ListType
    {
        public List<string> lines = new List<string>();
        public TransactionType Type { get; set; }
        public ListType()
        {

        }
        public ListType(string line)
        {
            lines.Add(line);
        }
        public void Add(string line)
        {
            lines.Add(line);
        }
        public int Count { get { return lines.Count; } }
        public string Get(int i) { /*if (lines.Count < i)*/ return lines[i]; /*else return null;*/ }
    }

    class RawNdcTransactionsList : List<RawNdcTransaction>
    {
        readonly string bim_note = @"BIMNDCNOTE";
        readonly string cdm_note = @"CDMCASHUNIT\(NDC\)";
        readonly string cdm_unit = @"CDMNDCCASHUNIT";
        readonly string for_bim = @"data_ndc_get_cash_unit_info\(CuCount<";
        readonly string end_bim = @"bim_ndc_get_cash_unit_info$";

        readonly string pbim = @"BIMSESITEM\(SES\)";
        readonly string for_cdm = @"cdm_ndc_get_cash_unit_info\(";

        readonly string start_incass = @"bin_start_exchange\(\)";

        readonly string pcard = @"\d{6}X{6,9}\d{4}"; // card and dpan 
        readonly string ptime = @"\d{8}:\d{6}";

        // cmd_cdm_dispense_ext(bPresent<0> R<0> 0<   0> 1<   0> 2<   5> 3<   1> 4<   4> 5<   0> 6<   0>)
        // cmd_cdm_dispense(R<0> 0<   0> 1<   0> 2<   0> 3<   0> 4<  20> 5<   0> 6<   0>)
        readonly string pd = @"NDC Dispense <\d+,\d+,\d+,\d+,\d+,\d+,\d+> Action: 00000000";

        // количество оригинальных кассет
        int cass_number = 0;

        private SortedDictionary<string, UnknownNominal> UnKnowns = new SortedDictionary<string, UnknownNominal>();

        public RawNdcTransactionsList(string path)
        {
            var txt = new ErlReader().ReadErlFiles(path);

            this.InitTransactions(txt);
        }
        public SortedDictionary<string, UnknownNominal> GetUnKnowns()
        {
            return UnKnowns;
        }

        private void FindUnknown(List<string> lst)
        {
            foreach (var item in lst)
            {
                if (item.Length > 58 && Regex.IsMatch(item.Substring(58), pbim))
                {
                    //BIMSESITEM(SES){dwNoteCode<     8> Nominal<   10000 643> usAcceptedCount<    0> AccL3<    0> AccL2<    0> dwCashInCount<    8>
                    //usUnknownCount<    0> dwRetractCount<    0> dwCashOutCount<    7> CashInL3<   0> CashInL2<   0> fwStatus<0>}
                    var line = Regex.Matches(item, @"<.+?>");
                    if (line.Count >= 7)
                    {
                        // Длина 12
                        var time = DateTime.ParseExact(Regex.Match(item, ptime).Value, "yyyyMMdd:HHmmss", CultureInfo.CurrentCulture);
                        var code = line[0].Value.Trim('<', '>', ' ');
                        var nom = Convert.ToInt32(line[1].Value.Trim('<', '>', ' ').Split(' ')[0]) / 100;
                        var count = Convert.ToInt32(line[6].Value.Trim('<', '>', ' '));
                        if (count > 0)
                            if (UnKnowns.ContainsKey(code))
                            {
                                UnKnowns[code].Count = count;
                            }
                            else
                            {
                                UnKnowns.Add(code, new UnknownNominal(time, nom, count));
                            }
                    }
                }
            }
        }

        private void InitTransactions(List<string> txt)
        {
            // Проходим и ищем unknow
            FindUnknown(txt);

            // Добавить lstb lastc 
            NominalList lstc = null;
            SortedDictionary<int, int> lstb = null;

            ListType strings = new ListType();
            strings.Type = TransactionType.Begin;

            RawNdcTransaction ndc = null;

            // Проходим по строкам, если попадаем на pcard Значит новая транзакция
            foreach (var line in txt)
            {
                if (Regex.IsMatch(line, pcard))//Regex.IsMatch(line.Substring(58), pcard))
                {
                    ndc = CreateTransaction(ref lstc, ref lstb, strings);
                    if (ndc != null) this.Add(ndc);
                    strings = new ListType(line);
                    strings.Type = TransactionType.Client;
                }
                else if (Regex.IsMatch(line, start_incass))//Regex.IsMatch(line.Substring(58), start_incass))
                {
                    ndc = CreateTransaction(ref lstc, ref lstb, strings);
                    if (ndc != null) this.Add(ndc);

                    strings = new ListType(line);
                    strings.Type = TransactionType.Incass;
                }
                else
                {
                    strings.Add(line);
                }
            }
            ndc = CreateTransaction(ref lstc, ref lstb, strings);
            if (ndc != null) this.Add(ndc);
        }

        private RawNdcTransaction CreateTransaction(ref NominalList lstc, ref SortedDictionary<int, int> lstb, ListType strings)
        {
            var ndc = InitializeTransaction(strings, ref lstb, ref lstc);
            // TODO: Проход по телу транзакции

            if (ndc.cdms.Count > 0 || ndc.bims.Count > 0)
            {
                if (ndc.cdms.Count > 0)
                    InitDispense(strings, ref ndc);

                if (ndc.Type == TransactionType.Client)
                {
                    ndc.bims.Reverse();
                    ndc.cdms.Reverse();
                }
                return ndc;
            }

            // Если транзакция без выдачи и приема возвращаем null
            return null;
        }
        private RawNdcTransaction InitializeTransaction(ListType strings, ref SortedDictionary<int, int> lstb, ref NominalList lstc)
        {
            var time = DateTime.ParseExact(Regex.Match(strings.Get(0), ptime).Value, "yyyyMMdd:HHmmss", CultureInfo.CurrentCulture);

            RawNdcTransaction tran = new RawNdcTransaction(time, "", strings.Type);

            if (strings.Type == TransactionType.Client)
            {
                InitilizeClient(ref tran, strings, ref lstb, ref lstc);
            }
            else if (strings.Type == TransactionType.Incass)
            {
                InitializeIncass(ref tran, strings, ref lstb, ref lstc);
            }
            else
            {
                InitializeBegin(ref tran, strings, ref lstb, ref lstc);
            }
            return tran;
        }

        private void InitializeIncass(ref RawNdcTransaction ndc, ListType strings, ref SortedDictionary<int, int> lstb, ref NominalList lstc)
        {

            SortedDictionary<int, int> bim = new SortedDictionary<int, int>();
          
            for (int i = 0; i < strings.Count; ++i)
            {
                // BIMBALITEM(ClSe)< 1>{Nom<      1000 978>
                // BIMBALITEM\(ClSe\)<\d+>\{Nom<\s{6}\d+ 643>     
                if (Regex.IsMatch(strings.Get(i), @"BIMBALITEM\(ClSe\)<\d+>\{Nom<\s+\d+ 643>"))//Regex.IsMatch(strings.Get(i).Substring(58), @"BIMBALITEM\(ClSe\)<\d+>\{Nom<\s+\d+ 643>"))
                {
                    int temp = i;
                    var match = Regex.Matches(strings.Get(i), @"<.+?>");
                    if (match.Count >= 4)
                    {
                        var nom = Convert.ToInt32(match[1].Value.Trim('<', '>', ' ').Split(' ')[0]) / 100;
                        // Берем нулевой и заносим если больше нуля

                        if (!bim.ContainsKey(nom))
                            bim.Add(nom, 0);
                    }
                    i = temp;
                }
                // Для CDM 
                else if (Regex.IsMatch(strings.Get(i), for_cdm))//Regex.IsMatch(strings.Get(i).Substring(58), for_cdm))
                {
                    int temp = i;
                    var time = DateTime.ParseExact(Regex.Match(strings.Get(i), ptime).Value, "yyyyMMdd:HHmmss", CultureInfo.CurrentCulture);
                    NominalList cdm = CreateCdm(strings, ref i, time);


                    if (lstc == null || cdm.Count > 0 && cdm.IsNoEqual(lstc))
                    {
                        ndc.cdms.Add(cdm);
                        lstc = cdm;
                    }


                    i = temp;
                }

            }
            ndc.bims.Add(bim);
          
            lstb = new SortedDictionary<int, int>();
        }

        private void InitilizeClient(ref RawNdcTransaction ndc, ListType strings, ref SortedDictionary<int, int> lstb, ref NominalList lstc)
        {
            ndc.Card = Regex.Match(strings.Get(0), pcard).Value;

            // Требуется делать возврат на 
            for (int i = strings.Count - 1; i >= 0; --i)
            {
                if (Regex.IsMatch(strings.Get(i), for_bim))//Regex.IsMatch(strings.Get(i).Substring(58), for_bim))
                {
                    // Набираем bim
                    SortedDictionary<int, int> bim = CreateBim(strings, ref i);

                    // Решаем надо ли его добавлять в стек
                    if (lstb == null || IsNoEqualDictionary(ref bim, ref lstb))
                    {
                        ndc.bims.Add(bim);
                        lstb = bim;
                    }
                }
                // Для CDM 
                else if (Regex.IsMatch(strings.Get(i), for_cdm))//Regex.IsMatch(strings.Get(i).Substring(58), for_cdm))
                {
                    var time = DateTime.ParseExact(Regex.Match(strings.Get(i), ptime).Value, "yyyyMMdd:HHmmss", CultureInfo.CurrentCulture);
                    NominalList cdm = CreateCdm(strings, ref i, time);

                    if (cdm.Count > 0)
                    {
                        // Определяем количество денежных кассет
                        if (cass_number == 0)
                        {
                            foreach (var item in cdm)
                            {
                                if (item.Nominal > 5)
                                {
                                    cass_number++;
                                }
                            }
                        }

                        if (lstc == null || cdm.IsNoEqual(lstc))
                        {
                            ndc.cdms.Add(cdm);
                            lstc = cdm;
                        }
                    }
                }
            }
        }
        private void InitializeBegin(ref RawNdcTransaction ndc, ListType strings, ref SortedDictionary<int, int> lstb, ref NominalList lstc)
        {
            //Для Bim            
            SortedDictionary<int, int> depo = CreateDepoForBegin(strings);
            if (depo.Count > 0)
            {
                ndc.bims.Add(depo);
                lstb = depo;
            }
            // Для CDM 
            SortedDictionary<int, int> disp = CreateDispForBegin(strings);

            if (disp.Count > 0)
            {
                NominalList lst = new NominalList(ndc.Time);
                foreach (var item in disp)
                {
                    lst.Add(new NominalCount(item.Key, item.Value));
                }
                lst.Reverse();

                ndc.cdms.Add(lst);
                lstc = lst;
            }
        }
        private SortedDictionary<int, int> CreateDepoForBegin(ListType strings)
        {
            SortedDictionary<int, int> depo = new SortedDictionary<int, int>();
            SortedDictionary<int, int> t = new SortedDictionary<int, int>();

            for (int i = strings.Count - 1; i > 0; --i)
            {
                if (Regex.IsMatch(strings.Get(i), bim_note))//Regex.IsMatch(strings.Get(i).Substring(58), bim_note))
                {
                    //BIMNDCNOTE<  0>{nominal<     50000 643> count<  203> dw_note_code<       0>}
                    var match = Regex.Matches(strings.Get(i).Substring(58), @"<.+?>");
                    if (match.Count >= 4)
                    {
                        var nom = Convert.ToInt32(match[1].Value.Trim('<', '>', ' ').Split(' ')[0]) / 100;
                        var count = Convert.ToInt32(match[2].Value.Trim('<', '>'));
                        // Берем нулевой и заносим если больше нуля
                        if (Convert.ToInt32(match[0].Value.Trim('<', '>', ' ')) == 0)
                        {
                            // если содержится, то передаем то что содержится в 
                            if (t.ContainsKey(nom))
                            {
                                if (!depo.ContainsKey(nom))
                                    depo.Add(nom, t[nom]);
                            }
                            else t.Add(nom, count);

                            if (nom == 10 || nom == 50)
                            {
                                if (!depo.ContainsKey(nom))
                                    depo.Add(nom, count);
                            }
                        }
                        else
                        {
                            if (!depo.ContainsKey(nom))
                                depo.Add(nom, count);
                        }
                    }
                }
            }

            return depo;
        }
        private SortedDictionary<int, int> CreateDispForBegin(ListType strings)
        {
            SortedDictionary<int, int> disp = new SortedDictionary<int, int>();

            for (int i = 0; i < strings.Count; ++i)
                if (Regex.IsMatch(strings.Get(i), cdm_note))
                {
                    var match = Regex.Matches(strings.Get(i).Substring(58), @"<.+?>");
                    if (match.Count >= 9)
                    {
                        // CDMCASHUNIT(NDC)< 0>{wCuType<2,REJ> STATUS< 0> UNIT<90000> AMT<         0 000> POS<0> AliasToItem<     > Initial<   0> Remain<   9> CashOut<    0> CashIn<    0> Unknown<  0> Alarm< 350>}
                        var n = match[1].Value.Split(',')[1].TrimEnd('>');
                        int nom = 0;
                        if (n == "REJ") nom = 1;
                        if (n == "RET") nom = 2;
                        var count = Convert.ToInt32(match[8].Value.Trim('<', '>'));
                        if (!disp.ContainsKey(nom)) disp.Add(nom, count);
                    }
                }
                else if (Regex.IsMatch(strings.Get(i), cdm_unit))
                {
                    // CDMNDCCASHUNIT<0>{wCuType<4,RCL> sz_unit_id<001> fitness<     OK> supplies<    GOOD> Nominal<      100.00 643> initial< 500> delivered< 151> rejected<   0> cash_in<   4> remained< 353>}
                    var match = Regex.Matches(strings.Get(i), @"<.+?>");
                    if (match.Count >= 11)
                    {
                        // nominal - 5 , count - 10 
                        var nom = Convert.ToInt32(match[5].Value.Trim(' ').Split('.')[0].Split(' ').Last());
                        var count = Convert.ToInt32(match[10].Value.Trim('<', '>'));
                        if (!disp.ContainsKey(nom)) disp.Add(nom, count);
                    }
                }


            return disp;
        }
        private void InitDispense(ListType strings, ref RawNdcTransaction ndc)
        {
            // NDC Dispense <00,16,00,00,00,00,00> Action: 00000000
            for (int i = 0; i < strings.Count; ++i)
            {
                if (Regex.IsMatch(strings.Get(i), pd))
                {
                    var match = Regex.Match(strings.Get(i).Substring(58), @"<.+?>");

                    if (match.Success)
                    {
                        // CDMCASHUNIT(NDC)< 0>{wCuType<2,REJ> STATUS< 0> UNIT<90000> AMT<         0 000> POS<0> AliasToItem<     > Initial<   0> Remain<   9> CashOut<    0> CashIn<    0> Unknown<  0> Alarm< 350>}
                        NominalList lst = new NominalList(DateTime.ParseExact(Regex.Match(strings.Get(i), ptime).Value, "yyyyMMdd:HHmmss", CultureInfo.CurrentCulture));
                        var nums = match.Value.Trim('<', '>').Split(',');

                        for (int d = 0, j = 0; d < cass_number; ++d, ++j)
                        {
                            lst.Add(j, Convert.ToInt32(nums[d]));
                        }
                        ndc.disp = lst;
                    }
                    break;
                }
            }
        }
        private NominalList CreateCdm(ListType txt, ref int i, DateTime time)
        {
            --i;
            int temp = i;
            NominalList cdm = new NominalList(time);

            while (i > 0)
            {
                if (Regex.IsMatch(txt.Get(i), cdm_note))
                {
                    temp = i;
                    var match = Regex.Matches(txt.Get(i).Substring(58), @"<.+?>");
                    if (match.Count >= 9)
                    {
                        // CDMCASHUNIT(NDC)< 0>{wCuType<2,REJ> STATUS< 0> UNIT<90000> AMT<         0 000> POS<0> AliasToItem<     > Initial<   0> Remain<   9> CashOut<    0> CashIn<    0> Unknown<  0> Alarm< 350>}
                        var n = match[1].Value.Split(',')[1].TrimEnd('>');
                        int nom = 0;
                        if (n == "REJ") nom = 1;
                        if (n == "RET") nom = 2;
                        var count = Convert.ToInt32(match[8].Value.Trim('<', '>'));
                        cdm.Add(nom, count);
                    }
                }
                else if (Regex.IsMatch(txt.Get(i), cdm_unit))
                {
                    // CDMNDCCASHUNIT<0>{wCuType<4,RCL> sz_unit_id<001> fitness<     OK> supplies<    GOOD> Nominal<      100.00 643> initial< 500> delivered< 151> rejected<   0> cash_in<   4> remained< 353>}
                    temp = i;
                    var match = Regex.Matches(txt.Get(i), @"<.+?>");
                    if (match.Count >= 11)
                    {
                        // nominal - 5 , count - 10 
                        var nom = Convert.ToInt32(match[5].Value.Trim(' ').Split('.')[0].Split(' ').Last());
                        var count = Convert.ToInt32(match[10].Value.Trim('<', '>'));
                        cdm.Add(nom, count);
                    }
                }
                else if (Regex.IsMatch(txt.Get(i), for_cdm))
                {
                    break;
                }
                --i;
            }
            i = temp;

            return cdm;
        }
        private SortedDictionary<int, int> CreateBim(ListType txt, ref int i)
        {
            // TODO: Перед добавлением Добавить unknow, прибавлять к номиналу и под вопросом в cdm
            SortedDictionary<int, int> bim = new SortedDictionary<int, int>();// { { 0, 0 } };
            SortedDictionary<int, int> t = new SortedDictionary<int, int>();

            --i;
            int temp = i;
            // от конца текущей до конца предыдущей
            while (i > 0 && !Regex.IsMatch(txt.Get(i), for_bim))
            {
                if (Regex.IsMatch(txt.Get(i), bim_note))
                {
                    temp = i;
                    //BIMNDCNOTE<  0>{nominal<     50000 643> count<  203> dw_note_code<       0>}
                    var match = Regex.Matches(txt.Get(i).Substring(58), @"<.+?>");
                    if (match.Count >= 4)
                    {
                        var nom = Convert.ToInt32(match[1].Value.Trim('<', '>', ' ').Split(' ')[0]) / 100;
                        var count = Convert.ToInt32(match[2].Value.Trim('<', '>'));
                        var index = Convert.ToInt32(match[0].Value.Trim('<', '>', ' '));
                        // Берем нулевой и заносим если больше нуля
                        if (index == 0)
                        {
                            // если содержится, то передаем то что содержится в 
                            if (t.ContainsKey(nom))
                            {
                                if (bim.ContainsKey(nom)) bim[nom] += t[nom];
                                else bim.Add(nom, t[nom]);
                            }
                            else t.Add(nom, count);

                            if (nom == 10 || nom == 50)
                            {
                                if (bim.ContainsKey(nom)) bim[nom] += count;
                                else bim.Add(nom, count);
                            }
                        }
                        else
                        {
                            if (bim.ContainsKey(nom)) bim[nom] += count;
                            else bim.Add(nom, count);
                        }
                    }
                }
                --i;
            }

            i = temp;

            return bim;
        }
        private bool IsNoEqualDictionary(ref SortedDictionary<int, int> cur, ref SortedDictionary<int, int> last)
        {
            if (cur.Count > last.Count) return true;
            if (cur.Count < last.Count)
            {
                cur = last;
                return true;
            }

            foreach (var item in last)
            {
                // TODO:* добавлена проверка
                if (!cur.ContainsKey(item.Key) || cur[item.Key] > item.Value) return true;
            }

            return false;
        }
    }
}
