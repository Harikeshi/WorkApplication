using EJ.logic.ej_xlsx.pre_ej;
using EJ.logic.ej_xlsx.pre_excel;
using EJ.logic.ej_xlsx.xlsx;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EJ.logic.ej_xlsx
{
    class ClientLines
    {
        public List<ClientLine> Depos = null;
        public List<ClientLine> Disps = null;
    }
    class BeginEndTime
    {
        public DateTime Begin { get; set; }
        public DateTime End { get; set; }

        public BeginEndTime(DateTime begin, DateTime end)
        {
            Begin = begin;
            End = end;
        }
    }
    class StructureForExcel
    {
        public OperDaysTimes Times = new OperDaysTimes();
        public ExcelOperations Disps = null;
        public ExcelOperations Depos = null;

        //public EJournalDays EjDays;// = new EJournalDays();
        public List<OperDay> Days = new List<OperDay>();

        private readonly RawEJournal EJournal;

        public List<Nominal> GetNominals()
        {
            return EJournal.GetOperators()[0].Nominals;
        }
        public StructureForExcel(RawEJournal ej, string disp_path = null, string dep_path = null)// ExcelDays disps = null, ExcelDays depos = null)
        {
            EJournal = ej;
            // Если atm, если terminal                

            OperatorType type = OperatorType.Atm;

            if (ej.GetOperators().Count > 0) type = ej.GetOperators().First().Type;
            else { Console.WriteLine("Отсутствуют входы оператора."); return; }

            DateTime start = DateTime.MinValue;
            DateTime end = DateTime.MinValue;

            // Находим времена деления по Диспенсеру и депозитору.
            InitDispAndDepos(disp_path, dep_path, ref start, ref end);
            // Определение времени
            // Если оба есть, если их длины равны, то проходим и 
            List<DateTime> times = InitPreTimes();

            // times - время периодов из excel
            //InitTimes(ej.GetOperators(), times, start, end);

            Times.Add(new DateTimeNominals(start, ej.GetOperators()[0].Nominals));
            Times.Add(new DateTimeNominals(end, ej.GetOperators()[0].Nominals));

            // Начало всегда есть и конец

            // Если у нас один оперцикл в Эксель - то берем начало и конец
            //if()

            // Если не один то надо подобрать промежуточные времена

            //


            InitOperDays();

            //InitEjParts();
        }

        private void InitDispAndDepos(string disp_path, string dep_path, ref DateTime start, ref DateTime end)
        {
            if (File.Exists(disp_path))
            {
                Disps = InitExcelOperations(disp_path);
                start = Disps.Start;
                end = Disps.End;
            }
            if (File.Exists(dep_path))
            {
                Depos = InitExcelOperations(dep_path);
                start = Depos.Start;
                end = Depos.End;
            }
        }

        private List<DateTime> InitPreTimes()
        {
            List<DateTime> times = new List<DateTime>();


            if (Disps != null && Depos != null)
            {
                times.Add(Disps.Start);
                // 
                if (Disps.Times.Count == Depos.Times.Count)
                {
                    // Первую самую раннюю последнюю самую позднюю
                    for (int i = 1; i < Disps.Times.Count - 1; ++i)
                    {
                        var time = Disps.Times[i] >= Depos.Times[i] ? Disps.Times[i] : Depos.Times[i];
                        times.Add(time);
                    }
                }
                else if (Disps.Times.Count > Depos.Times.Count)
                {
                    for (int i = 1; i < Disps.Times.Count - 1; ++i)
                    {
                        times.Add(Disps.Times[i]);
                    }
                }
                else
                {
                    for (int i = 1; i < Depos.Times.Count - 1; ++i)
                    {
                        times.Add(Depos.Times[i]);
                    }
                }
                times.Add(Disps.End);
            }//*
            else if (Disps != null)
            {
                foreach (var item in Disps.Times) times.Add(item);
                times[0] = Disps.Start;
                times[times.Count - 1] = Disps.End;
            }
            else if (Depos != null)
            {
                foreach (var item in Depos.Times) times.Add(item);
                times[0] = Depos.Start;
                times[times.Count - 1] = Depos.End;
            }

            return times;
        }
        private void InitOperDays()
        {

            var clients = EJournal.GetClients();

            // Проходим по всем временам и набираем и из той и из другой группы
            // 
            // пока client.time меньше времени
            // Дошли до времени начала первого дня

            var cl = InitClientLines(clients);

            for (int i = 1; i < Times.Count; ++i)
            {
                var oDay = new OperDay();
                oDay.Begin = Times[i - 1].Time;
                oDay.End = Times[i].Time;
                oDay.Nominals = Times[i - 1].Nominals;

                if (Depos != null)
                {
                    oDay.exc_de = Depos.FindAll(x => x.Time >= Times[i - 1].Time && x.Time <= Times[i].Time);
                    oDay.ej_de = cl.Depos.FindAll(x => x.Time >= Times[i - 1].Time && x.Time <= Times[i].Time);
                }
                if (Disps != null)
                {
                    oDay.exc_di = Disps.FindAll(x => x.Time >= Times[i - 1].Time && x.Time <= Times[i].Time);
                    oDay.ej_di = cl.Disps.FindAll(x => x.Time >= Times[i - 1].Time && x.Time <= Times[i].Time);
                }
                Days.Add(oDay);
            }
        }

        private static ClientLines InitClientLines(List<Client> clients)
        {
            ClientLines cl = new ClientLines();
            bool ds = true;
            bool de = true;
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].IsHad)
                {
                    for (int j = 0; j < clients[i].Parts.Count; ++j)
                    {
                        if (clients[i].Parts[j].Type == Part.PartType.Dispense)
                        {
                            if (ds)
                            {
                                ds = false;
                                cl.Disps = new List<ClientLine>();
                            }
                            cl.Disps.Add(InitClientLine(clients[i], j));
                        }
                        else if (clients[i].Parts[j].Type == Part.PartType.Deposite)
                        {
                            if (de)
                            {
                                de = false;
                                cl.Depos = new List<ClientLine>();
                            }
                            cl.Depos.Add(InitClientLine(clients[i], j));
                        }
                    }
                }
            }
            return cl;
        }

        public ExcelOperations InitExcelOperations(string path)
        {
            ExcelReader reader = new ExcelReader();
            // Открываем
            var lines = reader.Read(path);

            ExcelOperations exo = new ExcelOperations(lines);

            exo.Start = reader.Start; exo.End = reader.End;

            return exo;
        }
        private static ClientLine InitClientLine(Client client, int p)
        {
            ClientLine cl = new ClientLine();
            cl.Card = client.Card;
            cl.Time = client.Time;
            cl.Type = client.Parts[p].Type;
            cl.Comments = client.Parts[p].Comments;
            cl.Sum = client.Parts[p].Sum;
            cl.Counts = client.Parts[p].Counts;

            if (client.Parts.Count > p + 1)
            {
                cl.Number = client.Parts[p + 1].Number;
                cl.Time = client.Parts[p + 1].Time;
                cl.Comments.AddRange(client.Parts[p + 1].Errors);
            }

            return cl;
        }
        private void InitTimes(List<Operator> operators, List<DateTime> times, DateTime start, DateTime end)
        {
            // TODO: Если время не находим для данного промежутка, то ищем первое подходящее время для данного              

            //  1-2 2-3 

            if (operators.Count <= 1)
            {
                // Берем время первого и последнего клиента
                var clients = EJournal.GetClients();

                // Первого берем: находим предыдущего клиента если есть
                // И до конца клиентов
                var nominals = new List<Nominal>();
                if (operators.Count == 1)
                {
                    nominals = operators[0].Nominals;
                }
                Times.Add(new DateTimeNominals(times[0], nominals));
                Times.Add(new DateTimeNominals(clients.Last(c => c.IsHad == true).Time, nominals));

                return;
            }

            for (int i = 0, j = 0; i < times.Count; ++i)
            {
                for (j = 0; j < operators.Count; ++j)
                {
                    //var time1 = times[i];
                    //var time2 = operators[j].Time;
                    if (times[i] < operators[j].Time)
                    {
                        if (i != times.Count - 1)
                        {
                            Times.Add(new DateTimeNominals(operators[j - 1].Time, operators[j - 1].Nominals));
                        }
                        else
                        {
                            Times.Add(new DateTimeNominals(operators[j].Time, operators[j].Nominals));
                        }
                        break;
                    }
                }
            }
            if (times.Count > Times.Count)
            {
                if (times[times.Count - 1] > operators[operators.Count - 1].Time)
                    Times.Add(new DateTimeNominals(times[times.Count - 1], operators[0].Nominals));
                else
                    Times.Add(new DateTimeNominals(operators[operators.Count - 1].Time, operators[operators.Count - 1].Nominals));

            }
        }
        private DateTimeNominals MakeFirstTime(List<Operator> operators, DateTime t, ref int i)
        {
            // time - time - time
            // ... op ... op ...
            var time = FindFirstTimeOfClient(EJournal.GetClients(), operators[0].Nominals);

            // TODO: Эта проверка может быть не верна
            if (operators.Count == 1)
            {
                return new DateTimeNominals(operators[0].Time, operators[0].Nominals);
            }

            for (; i < operators.Count; ++i)
            {
                if (operators[i].Time > t)
                {
                    if (i - 1 >= 0)
                    {
                        time = new DateTimeNominals(operators[i - 1].Time, operators[i - 1].Nominals);
                        return time;
                    }
                }
            }

            return time;
        }
        private DateTimeNominals FindFirstTimeOfClient(List<Client> clients, List<Nominal> lst)
        {
            for (int i = 0; i < clients.Count; ++i)
            {
                for (var j = 0; j < clients[i].Parts.Count; ++j)
                {
                    if (clients[i].Parts[j].Type != Part.PartType.Other)
                    {
                        return new DateTimeNominals(clients[i].Parts[j].Time, lst);
                    }
                }
            }

            return null;
        }
        private DateTimeNominals FindLastTimeOfClient(List<Client> clients, List<Nominal> lst)
        {
            for (int i = clients.Count - 1; i >= 0; --i)
            {
                for (var j = clients[i].Parts.Count - 1; j >= 0; --j)
                {
                    if (clients[i].Parts[j].Type != Part.PartType.Other)
                    {
                        return new DateTimeNominals(clients[i].Parts[j].Time, lst);
                    }
                }
            }
            return null;
        }
        private List<DateTimeNominals> MakeMiddleTimes(List<Operator> operators, List<DateTime> times, ref int i)
        {
            List<DateTimeNominals> lst = new List<DateTimeNominals>();

            for (int j = 1; j < times.Count; ++j)
            {
                for (; i < operators.Count; ++i)
                {
                    if (operators[i].Time > times[j - 1] && operators[i].Time < times[j])
                    {
                        while (i < operators.Count && operators[i].Time < times[j])
                        {
                            ++i;
                        }

                        // Большее время
                        lst.Add(new DateTimeNominals(operators[i - 1].Time, operators[i - 1].Nominals));
                        break;
                    }
                }
            }

            return lst;
        }
        private DateTimeNominals MakeLastTime(List<Operator> operators, DateTime time, int i)
        {
            for (; i < operators.Count; ++i)
                if (operators[i].Time > time)
                    return new DateTimeNominals(operators[i].Time, operators[i].Nominals);

            //TODO: надо проверять
            // 
            var clients = EJournal.GetClients();
            // Если не нашли то берем последнюю 
            var last = new DateTimeNominals(time, operators[0].Nominals);

            return last;
        }
    }
}
