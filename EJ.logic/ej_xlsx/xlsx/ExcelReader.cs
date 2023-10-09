using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace EJ.logic.ej_xlsx.xlsx
{
    class ExcelReader : List<ExcelString>
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        private List<List<string>> ReadXML(string path)
        {
            List<List<string>> lst = new List<List<string>>();
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (SpreadsheetDocument doc = SpreadsheetDocument.Open(fs, false))
                {
                    WorkbookPart workbookPart = doc.WorkbookPart;
                    SharedStringTablePart sstpart = workbookPart.GetPartsOfType<SharedStringTablePart>().First();
                    SharedStringTable sst = sstpart.SharedStringTable;

                    WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
                    DocumentFormat.OpenXml.Spreadsheet.Worksheet sheet = worksheetPart.Worksheet;

                    var cells = sheet.Descendants<DocumentFormat.OpenXml.Spreadsheet.Cell>();
                    var rows = sheet.Descendants<DocumentFormat.OpenXml.Spreadsheet.Row>();

                    foreach (Row row in rows)
                    {
                        // Создаем новый элемент строки
                        List<string> temp = new List<string>();
                        foreach (Cell c in row.Elements<Cell>())
                        {
                            if ((c.DataType != null) && (c.DataType == CellValues.SharedString))
                            {
                                int ssid = int.Parse(c.CellValue.Text);

                                string str = sst.ChildElements[ssid].InnerText;

                                temp.Add(str);
                            }
                            else if (c.CellValue != null)
                            {
                                temp.Add(c.CellValue.Text);
                            }
                        }
                        lst.Add(temp);
                    }
                }
            }
            return lst;
        }
        public List<ExcelString> Read(string path)
        {
            var lst = ReadXML(path);

            Start = DateTime.ParseExact(lst[2][0].Substring(lst[2][0].IndexOf(':') + 2), "dd/MM/yyyy HH:mm:ss", CultureInfo.CurrentCulture);
            End = DateTime.ParseExact(lst[3][0].Substring(lst[3][0].IndexOf(':') + 2), "dd/MM/yyyy HH:mm:ss", CultureInfo.CurrentCulture);

            int i = 7;
            try
            {
                for (; i < lst.Count; i++)
                {
                    // 01/09/2021 16:17:15
                    this.Add(new ExcelString
                    {
                        Time = DateTime.ParseExact(lst[i][0], "dd/MM/yyyy HH:mm:ss", CultureInfo.CurrentCulture),
                        Amount = Convert.ToInt32(lst[i][4]),
                        Card = lst[i][2],
                        MsgType = lst[i][7],
                        Number = Convert.ToInt32(lst[i][1]),
                        TrxType = lst[i][6]
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + path + " in line #" + i.ToString() + ".");
            }
            return this;
        }
    }
}
