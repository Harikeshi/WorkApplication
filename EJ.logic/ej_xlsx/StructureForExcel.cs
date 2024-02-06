using DocumentFormat.OpenXml.Spreadsheet;
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
        private OperDaysTimes Times = new OperDaysTimes();
        private ExcelOperations Disps = null;
        private ExcelOperations Depos = null;

        private DateTime Start = DateTime.MinValue;
        private DateTime End = DateTime.MinValue;

        private List<Nominal> Nominals = new List<Nominal>();
        private List<OperDay> Days = new List<OperDay>();

        public List<Nominal> GetNominals() { return Nominals; }
        public List<OperDay> GetDays() { return Days; }
        public StructureForExcel(RawEJournal ej, string disp_path = null, string dep_path = null)
        {

            // Если atm, если terminal                
            OperatorType type = OperatorType.Atm;
            if (ej.GetOperators().Count > 0) type = ej.GetOperators().First().Type;

            Nominals = ej.GetOperators()[0].Nominals;

            // Находим времена деления по Диспенсеру и депозитору из excel
            InitDispAndDepos(disp_path, dep_path); // Инициализация Start и End

            // 1. Получаем времена из Операторов, с учетом того что в это время была смена нумерации.
            // 2. Получаем опердни с учетом этого времени

            // Список рабочих времен
            Times.Add(new DateTimeNominals(Start, Nominals));

            // Будем сохранять времена из Опердней
            Times.AddRange(MakeTimesFromEjournal(ej));
            Times.Add(new DateTimeNominals(End, Nominals));

            InitOperDays(ej);

        }

        private OperDaysTimes MakeTimesFromEjournal(RawEJournal ej)
        {
            OperDaysTimes temp = new OperDaysTimes();

            var clients = ej.GetClients();

            foreach (var item in ej.GetOperators())
            {

                if (item.Time <= Start || item.Time >= End) continue;

                // Взять предыдущее значение по времени
                // Получить номер предыдущей транзакции

                // Если первый меньше нуля, то можно не искать второй, я сразу добавлять
                var prevIndex = clients.IndexOf(clients.LastOrDefault(x => x.Time < item.Time));
                var nextIndex = clients.IndexOf(clients.FirstOrDefault(x => x.Time > item.Time));
                if (prevIndex < 0 || nextIndex < 0) { temp.Add(new DateTimeNominals(item.Time, Nominals)); continue; }

                // Нам надо найти номера
                // Ищем номер предыдущего
                int pnum = -1;
                int lnum = -1;
                for (int i = prevIndex; i >= 0; --i)
                {
                    if (clients[i].Parts.Count > 0)
                    {
                        var num = clients[i].Parts.LastOrDefault(x => x.Number > 0).Number;
                        if (num > 0) { pnum = num; break; }
                    }
                }
                // Ищем номер следующего
                for (int i = nextIndex; i < clients.Count; ++i)
                {
                    if (clients[i].Parts.Count > 0)
                    {
                        var num = clients[i].Parts.FirstOrDefault(x => x.Number > 0).Number;
                        if (num > 0) { lnum = num; break; }
                    }
                }
                if (pnum > lnum) temp.Add(new DateTimeNominals(item.Time, Nominals));
            }

            return temp;
        }

        private void InitDispAndDepos(string disp_path, string dep_path)
        {
            if (File.Exists(disp_path))
            {
                Disps = InitExcelOperations(disp_path);
                Start = Disps.Start;
                End = Disps.End;
            }
            if (File.Exists(dep_path))
            {
                Depos = InitExcelOperations(dep_path);

                if (Start == DateTime.MinValue)
                {
                    Start = Depos.Start;
                    End = Depos.End;
                }
            }
        }

        private void InitOperDays(RawEJournal ej)
        {

            var clients = ej.GetClients();

            ClientLines cl = InitClientLines(clients); // Формируем список клиентов для эксель depos и disps

            for (int i = 1; i < Times.Count; ++i)
            {
                var oDay = new OperDay();
                oDay.Begin = Times[i - 1].Time;
                oDay.End = Times[i].Time;
                oDay.Nominals = Times[i - 1].Nominals;

                if (Depos != null && Depos.Count != 0)
                {
                    oDay.exc_de = Depos.FindAll(x => x.Time >= Times[i - 1].Time && x.Time <= Times[i].Time);
                    if (cl.Depos != null)
                        oDay.ej_de = cl.Depos.FindAll(x => x.Time >= Times[i - 1].Time && x.Time <= Times[i].Time);
                }
                if (Disps != null && Disps.Count != 0)
                {
                    oDay.exc_di = Disps.FindAll(x => x.Time >= Times[i - 1].Time && x.Time <= Times[i].Time);
                    if (cl.Disps != null)
                        oDay.ej_di = cl.Disps.FindAll(x => x.Time >= Times[i - 1].Time && x.Time <= Times[i].Time);
                }

                Days.Add(oDay);
            }
        }

        private ClientLines InitClientLines(List<Client> clients)
        {
            ClientLines cl = new ClientLines();
            bool ds = true;
            bool de = true;
            // TODO: Список по времени Start и End
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].IsHad && (clients[i].Time >= Start && clients[i].Time <= End))
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

            exo.Start = reader.Start;
            exo.End = reader.End;

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
    }
}
