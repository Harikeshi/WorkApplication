using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace EJ.logic.ej_xlsx.pre_ej
{
    class RawEJournal
    {
        // Сепараторы основного текста
        private readonly string start_op_session = @"С Е С С И Я   О П Е Р А Т О Р А";
        private readonly string start_client = @"К Л И Е Н Т     <   N D C >";
        private readonly string start_balance = @"Б А Л А Н С     Т Е Р М И Н А Л А";
        private readonly string start_sys_ndc = @"\*     < С И С Т Е М А     N D C >     \*";

        private readonly string pcode = @"КОД ОПЕРАТОРА:";

        // Фильтр комментов
        private readonly List<string> DispenseMsg = new List<string>() { "ДЕНЬГИ НЕ ВЗЯТЫ", "РЕТРАКТ НЕВЗЯТЫХ ДЕНЕГ", "ДЕНЬГИ НЕ ОБНАРУЖЕНЫ", "УСПЕШНЫЙ РЕТРАКТ", "НА МОМЕНТ РЕТРАКТА" };
        private readonly List<string> DepositMsg = new List<string>() { "Платеж наличными не завершен", "ПРЕВЫШЕНО ВРЕМЯ ОЖИДАНИЯ ИЗЪЯТИЯ КУПЮР", "Операция ПОПОЛНЕНИЕ не завершена", "СРЕДСТВА МОГЛИ БЫТЬ ДОСТУПНЫ КЛИЕНТУ", "ПРИНЯТО СЛИШКОМ МНОГО КУПЮР - ВОЗВРАТ!", "ТАЙМАУТ ОЖИДАНИЯ ВСТАВЛЕНИЯ КУПЮР!", "_827_DO NOT HONOR", "ОТКАЗ КЛИЕНТА ОТ  ОПЕРАЦИИ", "ДЕНЬГИ ВЗЯТЫ  КЛИЕНТОМ", "АППАРАТНЫЙ СБОЙ", "НЕТ  ОТВЕТА ОТ ХОСТА", "РЕТРАКТ КУПЮР ВЫПОЛНЕН УСПЕШНО" };

        private readonly string stars = @"\*{40}";
        private readonly string minuses = @"\-{40}";

        // Из Client
        private readonly string disp = @"\d+-00,\d+-00,\d+-00,\d+"; //Delivery < new> < 00 - 00,00 - 00,00 - 00,02 - 00 > Action: 00000000
        private readonly string bim = @"BIM\:"; //BIM: 0,0,0,0,0,1,0,0
        private readonly string dep = @"ПРИНЯТО НАЛИЧНЫМИ:"; // ПРИНЯТО НАЛИЧНЫМИ:          5 000,00 РУБ
        private readonly string old_disp = @"УСПЕШНЫЙ НАСЧЕТ:";

        private readonly string vpan = @"V\.PAN";
        private readonly string ncard = @"КАРТА";
        private readonly string pcard = @"\d{6}XXXXXX\d{4}";
        private readonly string ppart1 = @"^ \-{10}$";
        private readonly string ppart2 = @"^\-{10}$";
        private readonly string error_code = @"_[\d][\d][\d]_";
        private readonly string head_time = @"\d+\.\d+\.\d+[ ]+\d+:\d+:\d{2}";
        private readonly string begin = @"НАЧАЛО СЕССИИ";

        // Из Operator
        private readonly string separator = @"\-{40}";
        // Банкомат
        private readonly string nominal_old = @"\b\d{3}[ ]+\d+\.00 (643|810)[ ]+\d{3,4}[ ]+\d+[ ]+\d+\b";
        private readonly string pos = @"ЦИЯ:";
        private readonly string nominal_new = @"\b\d+\.00 (810|643)[ ]\d{4,}[ ]\d{5,}[ ]\d{5,}[ ]\d{4,}[ ]\d{4,}\b";
        private readonly string close_cdm = @"КАССЕТЫ ДИСПЕНСЕРА ЗАКР"; // Конец нформации
        private readonly string load = @"ЗАГРУЖЕНО[ ]{8}:";
        // Терминал
        private readonly string close_op = @"З А К Р Ы Т И Е   С Е С С И И     B I M";
        private readonly string session_number = @"СЕССИЯ: \d+";

        //private readonly List<string> lines = new List<string>();

        private readonly List<RawOperation> clients = new List<RawOperation>();
        private readonly List<RawOperation> balances = new List<RawOperation>();
        private readonly List<RawOperation> operators = new List<RawOperation>();
        private readonly List<RawOperation> ndcs = new List<RawOperation>();
        private readonly List<string> other = new List<string>();

        private readonly List<Client> Clients = new List<Client>();
        private readonly List<Operator> Operators = new List<Operator>();


        public List<Client> GetClients() { return Clients; }
        public List<Operator> GetOperators() { return Operators; }

        public List<RawOperation> Getclients() { return clients; }
        public List<RawOperation> Getoperators() { return operators; }
        public List<RawOperation> Getbalances() { return balances; }
        public List<RawOperation> Getndc() { return ndcs; }

        public RawEJournal(string path)
        {
            // Читаем все линии
            var strings = File.ReadAllLines(path).ToList();

            // Клиенты
            List<string> lines = MakeClients(strings); // Остаток после операций

            // Балансы
            lines = MakeBalanses(lines);

            // Операторы
            lines = MakeOperators(lines);
        }

        private List<string> MakeOperators(List<string> strings)
        {
            List<string> other = new List<string>();

            List<int> NoNominalsList = new List<int>();
            for (int j = 0; j < strings.Count; ++j)
            {
                if (Regex.IsMatch(strings[j], start_op_session))
                {
                    RawOperation op = new RawOperation();
                    op.Type = OperationType.Operator;
                    op.Add(strings[j]);
                    ++j;
                    while (j < strings.Count)
                    {
                        if (Regex.IsMatch(strings[j], start_op_session))
                        {
                            --j;
                            break;
                        }
                        op.Add(strings[j]);
                        ++j;
                    }
                    //if (op.Size() != 0)

                    // TODO: набирать op и запоминать в котором нет полного списка номиналов и когда список сформируется в каком либо, заполнить списки этих
                    operators.Add(op);
                    var oper = InitializeOperator(op.GetLines());

                    if (oper.Nominals.Count < 4)
                    {
                        NoNominalsList.Add(operators.Count - 1);
                        Console.WriteLine("Operators[" + (operators.Count - 1) + "].Nominal.Counts = " + oper.Nominals.Count + "(4)");
                    }
                    else
                    {
                        foreach (int i in NoNominalsList)
                        {
                            Operators[i].Nominals = oper.Nominals;
                            Operators[i].Type = OperatorType.Atm;
                        }
                    }
                    Operators.Add(oper);
                }
                else other.Add(strings[j]);
                if (NoNominalsList.Count > 0 && Operators[0].Type != OperatorType.Terminal)
                {
                    foreach (int i in NoNominalsList)
                    {
                        Operators[i].Nominals = Operators[0].Nominals;
                        Operators[i].Type = OperatorType.Atm;
                    }
                }
            }
            return other;
        }

        private List<string> MakeBalanses(List<string> lines)
        {
            List<string> strings = new List<string>();
            for (int j = 0; j < lines.Count; ++j)
            {
                if (Regex.IsMatch(lines[j], start_balance))
                {
                    RawOperation op = new RawOperation();
                    op.Type = OperationType.Balance;
                    op.Add(lines[j]);
                    ++j;
                    while (j < lines.Count)
                    {
                        if (Regex.IsMatch(lines[j], start_balance) || Regex.IsMatch(lines[j], start_op_session))
                        {
                            --j;
                            break;
                        }

                        op.Add(lines[j]);
                        ++j;
                    }
                    // TODO: Добавить обработку.
                    balances.Add(op);
                }
                if (j < lines.Count)
                    strings.Add(lines[j]);
            }

            return strings;
        }

        private List<string> MakeClients(List<string> strings)
        {
            List<string> lines = new List<string>(); // Остаток
            for (int j = 0; j < strings.Count; ++j)
            {
                if (Regex.IsMatch(strings[j], start_client))
                {
                    RawOperation op = new RawOperation();
                    op.Type = OperationType.Client;
                    ++j;
                    // Пока не нашли разделитель                    
                    while (!Regex.IsMatch(strings[j], minuses) && !Regex.IsMatch(strings[j], start_client) &&
                        !Regex.IsMatch(strings[j], start_op_session) && !Regex.IsMatch(strings[j], start_sys_ndc) &&
                        !Regex.IsMatch(strings[j], start_balance) && !Regex.IsMatch(strings[j], start_balance))
                    {
                        if (strings[j] != "")
                        {
                            op.Add(strings[j]);
                        }
                        ++j;

                        if (strings.Count <= j)
                        {
                            break;
                        }
                    }
                    if (strings.Count > j && Regex.IsMatch(strings[j], start_client)) --j;
                    clients.Add(op);
                    var cl = InitializeClient(op.GetLines());

                    // Если не нашли время
                    if (cl.Time == DateTime.MinValue)
                    {
                        Console.Write("[~" + j + "]Clients[" + (Clients.Count - 1) + "].Time = " + cl.Time);
                        if (cl.Parts.Count > 0)
                        {
                            cl.Time = cl.Parts.Last().Time;
                        }
                        else
                        {
                            try
                            {
                                cl.Time = Clients.Last().Time;
                            }
                            catch
                            {
                                cl.Time = DateTime.MinValue;
                            }
                        }
                        Console.WriteLine(", NewTime = " + cl.Time);
                    }
                    Clients.Add(cl);
                }

                // Добавляем в остаток
                if (strings.Count > j && strings[j] != "")
                    //if ((strings[j][0] >= 'А' && strings[j][0] <= 'Я') || strings[j][0] == ' ')
                    lines.Add(strings[j]);
            }

            return lines;
        }

        private string MsgFilter(List<string> msg, Part p)
        {
            string result = null;
            for (int i = 0; i < p.Comments.Count(); ++i)
            {
                foreach (string m in msg)
                {
                    if (Regex.IsMatch(p.Comments[i], m))
                    {
                        if (result == null) result = String.Empty;
                        result += m.Trim() + ", ";
                    }
                }
            }
            foreach (var item in p.Errors)
            {
                result += item;
            }
            return result;
        }

        private Client InitializeClient(List<string> strings)
        {
            Client client = new Client();

            int i = 0;

            var length = InitHead(ref i, strings, ref client);

            // В head может быть операция
            if (length >= 11) // Условная цифра выявлена императивным путем, корректируется
            {
                i = 0;
                Part p = new Part();
                InitPart(ref i, strings, ref client, ref p);

                string comm = MsgFilter(p.Type == Part.PartType.Dispense ? DispenseMsg : DepositMsg, p);
                p.Comments = new List<string>();
                if (comm != null) p.Comments.Add(comm);

                p.Time = client.Time;
                client.Parts.Add(p);
            }
            // TODO: До конца транзакции набираем список
            // Если находим разделитель обрабатываем часть
            while (i < strings.Count - 1)
            {
                Part p = new Part();
                MakeTimeAndNumber(ref i, ref p, strings);
                InitPart(ref i, strings, ref client, ref p);

                string comm = MsgFilter(p.Type == Part.PartType.Dispense ? DispenseMsg : DepositMsg, p);
                p.Comments = new List<string>();
                if (comm != null) p.Comments.Add(comm);

                client.Parts.Add(p);
                ++i;
            }
            return client;
        }
        private int InitHead(ref int i, List<string> Lines, ref Client client)
        {
            bool time_not_found = true;
            bool card_is_not_Found = true;
            int length = 0;

            // TODO: Набираем лист до разделителя
            // Если длина большая, то обрабатываем, как обычную, а потом как часть
            // то есть создаем голову и часть потом

            while ((i < Lines.Count) && !Regex.IsMatch(Lines[i], @"^\-{10}$") && !Regex.IsMatch(Lines[i], @"^ \-{10}$"))//1
            {
                if (time_not_found && Regex.IsMatch(Lines[i], begin))
                {
                    var x = Regex.Match(Lines[i], head_time);
                    var time = Regex.Replace(x.Value, @"\s+", String.Empty);

                    if (time.Length == 17)
                    {
                        try
                        {
                            client.Time = DateTime.ParseExact(time, "dd.MM.yyyyH:mm:ss", CultureInfo.CurrentCulture);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Строка приведения времени: [" + time + "]: " + e.Message);
                        }
                    }
                    else
                    {
                        try
                        {
                            client.Time = DateTime.ParseExact(time, "dd.MM.yyyyHH:mm:ss", CultureInfo.CurrentCulture);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Строка приведения времени: [" + time + "]: " + e.Message);
                        }
                    }

                }
                if (card_is_not_Found && Regex.IsMatch(Lines[i], vpan) || Regex.IsMatch(Lines[i], pcard)
                    || Regex.IsMatch(Lines[i], "DPAN"))
                {
                    var index = Lines[i].IndexOf(':');

                    string value = Lines[i].Substring(index + 1, Lines[i].Length - index - 1).Trim(' ');
                    client.Card = value.Trim(' ').Split(' ')[0].Trim(' ');
                    card_is_not_Found = true;
                }
                ++length;
                ++i;
            }
            if (i != Lines.Count)
            {
                while ((i < Lines.Count) && !Regex.IsMatch(Lines[i], @"^\-{10}$") && !Regex.IsMatch(Lines[i], @"^ \-{10}$"))
                {
                    ++i;
                    ++length;
                }
                if (i != Lines.Count)
                {
                    ++i;
                }
            }
            return length;
        }
        private void InitDepositePart(ref int i, List<string> Lines, ref Part p, ref Client client)
        {
            // Обработать строку с суммой
            //ПРИНЯТО НАЛИЧНЫМИ: 12 000,00 РУБ
            var num = @"\d+[ ]?\d+";
            var s = Regex.Replace(Regex.Match(Lines[i], num).Value, @"\s+", String.Empty);
            bool depo_complete = false;
            int index = 0;

            p.Sum = Convert.ToInt32(s);
            p.Type = Part.PartType.Deposite;

            client.IsHad = true;

            while ((i < Lines.Count) && !Regex.IsMatch(Lines[i], ppart1))
            {
                if (!depo_complete && Regex.IsMatch(Lines[i], bim))
                {
                    Lines[i] = Lines[i].Substring(Lines[i].IndexOf(':') + 1, Lines[i].Length - Lines[i].IndexOf(':') - 1);
                    var ns = Lines[i].Split(',');

                    for (int j = 0; j < ns.Length; ++j)
                    {
                        try
                        {
                            p.Counts.Add(Convert.ToInt32(ns[j]));
                        }
                        catch
                        {
                            p.Counts = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0 };
                            Console.WriteLine("Определение BIM из строки: [" + Lines[i] + "] "); break;
                        }
                    }
                    depo_complete = true;
                }
                else if ((index = Lines[i].IndexOf(':')) > -1)
                {
                    string key = Lines[i].Substring(0, index);
                    string value = Lines[i].Substring(index + 1, Lines[i].Length - index - 1).Trim(' ');
                    // TODO: Проверку на соответствие строк

                    if (!p.Infos.ContainsKey(key))
                        p.Infos.Add(key, value);
                }
                else if (Lines[i] == "") { }
                else { p.Comments.Add(Lines[i]); }
                ++i;
            }
            if (p.Counts.Count == 0)
            {
                p.Counts = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0 };
            }
        }
        private int InitDispensePart(ref int i, List<string> Lines, ref Part p, ref Client client, Match x)
        {
            var nums = x.Value.Split(',');

            for (int j = 0; j < nums.Length; ++j)
            {
                var num = Regex.Match(nums[j], @"\d+").Value;
                p.Counts.Add(Convert.ToInt32(num));
            }

            p.Type = Part.PartType.Dispense;
            client.IsHad = true;
            while ((i < Lines.Count - 1) && !Regex.IsMatch(Lines[i], ppart1))
            {
                p.Comments.Add(Lines[i]);
                ++i;
            }

            return i;
        }
        private void InitPart(ref int i, List<string> Lines, ref Client client, ref Part p)
        {
            //10/03/23 11:51:06 51
            //i = MakeTimeAndNumber(i, p, Lines);
            while ((i < Lines.Count - 1) && !Regex.IsMatch(Lines[i], ppart1) && !Regex.IsMatch(Lines[i], ppart2))
            {
                int ind = 0;
                Match x = null;
                Match y = null;

                if (Regex.IsMatch(Lines[i], dep))
                {
                    InitDepositePart(ref i, Lines, ref p, ref client);
                    break;
                }
                // Если нашли строку Delivery проходим до конца записывая все в комментарии
                else if ((x = Regex.Match(Lines[i], disp)).Success || (y = Regex.Match(Lines[i], old_disp)).Success)
                {
                    if (y != null && y.Success)
                    {
                        // Обрабатыва строку и идем дальше
                        //УСПЕШНЫЙ НАСЧЕТ:              500,00 РУБ
                        p.Sum = Convert.ToInt32(Regex.Match(Regex.Replace(Lines[i], @"\s+", String.Empty), @"\d+").Value);
                    }
                    else
                    {
                        //02-00,01-00,00-00,01
                        InitDispensePart(ref i, Lines, ref p, ref client, x);
                        break;
                    }
                    //Проходим все до конца и выходим Часть закончена
                }
                else if ((ind = Lines[i].IndexOf(':')) > -1)
                {
                    string key = Lines[i].Substring(0, ind);
                    string value = Lines[i].Substring(ind + 1, Lines[i].Length - ind - 1);

                    // TODO: Проверку на соответствие строк

                    if (!p.Infos.ContainsKey(key))
                        p.Infos.Add(key, value);
                }

                else
                {
                    if (Regex.IsMatch(Lines[i], error_code)) { p.Errors.Add(Lines[i]); }
                    p.Comments.Add(Lines[i]);
                }

                ++i;
            }
        }
        private void MakeTimeAndNumber(ref int i, ref Part p, List<string> Lines)
        {
            // 10/01/22 08:13:43 143 
            // @"\d{2}/\d{2}/\d{2}[ ]+\d{2}:\d{2}:\d{2}[ ]+\d+"
            while (!Regex.IsMatch(Lines[i], @"\d{2}/\d{2}/\d{2}[ ]+\d{2}:\d{2}:\d{2}[ ]+\d+")) ++i;

            var str = Lines[i].Split(' ');
            if (str.Length >= 3)
            {
                var time = Regex.Match(Lines[i], @"\d{2}/\d{2}/\d{2}[ ]+\d{2}:\d{2}:\d{2}");
                var num = Regex.Match(str[2], @"\d+");
                p.Number = Convert.ToInt32(num.Value);
                p.Time = DateTime.ParseExact(time.Value, "dd/MM/yy HH:mm:ss", CultureInfo.CurrentCulture);
            }

            ++i;
        }
        /// <summary>
        /// Operator Initialize
        /// </summary>
        /// <param name="operator"></param>
        private Operator InitializeOperator(List<string> lst)
        {
            int i = 0;
            Operator oper = new Operator();
            // Определяем Дату и время
            // ОТ:  16.11.2022 13:07:20
            i = MakeTime(lst, i, ref oper);

            // ОТ:  10.03.2023  9:20:1
            // Кассеты
            i = MakeDispenseNominals(lst, i, ref oper);

            return oper;
        }
        private int MakeTime(List<string> lines, int i, ref Operator op)
        {
            //TODO: Сделать совпадение с полной датой // ДАТА: 17.05.2023
            string time = @"ДАТА: \d{2}\.\d{2}\.\d{4}";
            string p1 = @"\d{2}\.\d{2}\.\d{4}";//10.03.2023
            string p2 = @"\d{1,2}:\d{2}:\d{2}";// 9:20:17

            while (i < lines.Count)
            {
                if (Regex.IsMatch(lines[i], time))
                {
                    var str = lines[i].Split(' ');
                    string t = String.Empty;
                    foreach (var line in str)
                    {
                        var d1 = Regex.Match(line, p1);
                        if (d1.Success)
                        {
                            t += d1.Value;
                        }
                        var d2 = Regex.Match(line, p2);
                        if (d2.Success)
                        {
                            if (d2.Value.Length == 7)
                                t += '0';
                            t += d2.Value;
                        }
                    }
                    op.Time = DateTime.ParseExact(t, "dd.MM.yyyyHH:mm:ss", CultureInfo.CurrentCulture);

                    break;
                }
                ++i;
            }

            return i;
        }
        private int MakeDispenseNominals(List<string> opLines, int i, ref Operator op)
        {
            SortedDictionary<int, Nominal> dict = new SortedDictionary<int, Nominal>();

            // TODO: Проходить до конца для определения сессии

            // bool fnumber = true;
            while (i < opLines.Count)
            {
                // СЕССИЯ: 063     ОТ:  22.12.2021 15:53:34 //@"СЕССИЯ: \d+[ ]+ОТ:"
                //if (fnumber)
                //{
                //    var ses = Regex.Match(opLines[i], @"СЕССИЯ: \d+[ ]+ОТ:");
                //    if (ses.Success)
                //    {
                //        //22.12.2021 15:53:34                       
                //        var num = Regex.Match(ses.Value, @"\d+");

                //        if (num.Success)
                //        {
                //            op.Number = Convert.ToInt32(num.Value);
                //            fnumber = false;
                //        }
                //    }
                //}
                if (dict.Count < 4)
                {
                    if (Regex.IsMatch(opLines[i], nominal_old))
                    {
                        op.Type = OperatorType.Atm;
                        i = MakeOldNominals(opLines, i, dict);
                    }
                    else if (Regex.IsMatch(opLines[i], pos))
                    {
                        op.Type = OperatorType.Atm;
                        int t = MakeNewNominals(opLines, ref i, dict);
                        if (t > 0) op.Number = t;
                    }
                }
                ++i;
            }
            foreach (var pair in dict.Values)
                op.Nominals.Add(pair);
            if (dict.Count != 0 && dict.Values.Last().Value < dict.Values.First().Value)
            {
                op.Nominals.Reverse();
            }

            //if(dict.Count<3)

            return i;
        }
        private int MakeNewNominals(List<string> opLines, ref int i, SortedDictionary<int, Nominal> dict)
        {
            int number = 0;
            int p = 0;
            while (dict.Count < 4 && i < opLines.Count)
            {
                //var ses = Regex.Match(opLines[i], session_number);
                //if (ses.Success)
                //{
                //    try
                //    {
                //        number = Convert.ToInt32(ses.Value.Split(' ')[1]);
                //    }
                //    catch { }
                //}
                if (Regex.IsMatch(opLines[i], pos))
                {
                    try
                    {
                        p = Convert.ToInt32(opLines[i].Split(' ')[1].Trim(' '));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Определение номинала из строки [{0}]: " + ex.Message, opLines[i]);
                    }
                    ++i;
                    if (dict.ContainsKey(p))// 5
                    {
                        continue;
                    }
                    else
                    {
                        while (dict.Count < 4 && i < opLines.Count)
                        {

                            if (Regex.IsMatch(opLines[i], pos))
                            {
                                try
                                {
                                    var temp = Convert.ToInt32(opLines[i].Split(' ')[1].Trim(' '));
                                    if (!dict.ContainsKey(temp))
                                        p = temp;
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Определение номинала из строки [{0}]:" + ex.Message, opLines[i]);
                                }
                            }
                            if (Regex.IsMatch(opLines[i], nominal_new))
                            {
                                opLines[i] = ClearString(opLines[i]);
                                var str = opLines[i].Split(' ');
                                try
                                {
                                    dict.Add(p, new Nominal(Convert.ToInt32(str[0].Split('.')[0]), Convert.ToInt32(str[2])));
                                    break;
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Добавление в словарь номинала [{0}]:" + ex.Message, p);
                                }
                            }
                            ++i;
                        }
                    }
                }
                ++i;
            }

            return number;
        }
        private int MakeOldNominals(List<string> opLines, int i, SortedDictionary<int, Nominal> dict)
        {
            while (dict.Count < 4 && i < opLines.Count)
            {
                //Определяем номер и номинал с количеством
                if (Regex.IsMatch(opLines[i], nominal_old))
                {
                    opLines[i] = ClearString(opLines[i]);
                    var p = opLines[i].Split(' ');

                    try
                    {
                        int key = Convert.ToInt32(p[0]);
                        if (!dict.ContainsKey(key))
                            dict.Add(key, new Nominal(Convert.ToInt32(p[1].Split('.')[0]), Convert.ToInt32(p[3])));
                    }
                    catch (Exception e) { Console.WriteLine("Добавление в словарь 1:" + e.Message); }
                }
                ++i;
            }


            return i;
        }
        private string ClearString(string s)
        {
            string res = String.Empty;
            for (int i = 1; i < s.Length; ++i)
            {
                if (s[i] == ' ')
                {
                    if (s[i - 1] != ' ') res += s[i];
                }
                else if (s[i] == '.' || Char.IsDigit(s[i]))
                {
                    res += s[i];
                }
            }
            return res;
        }
    }
}
