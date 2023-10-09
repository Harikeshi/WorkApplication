using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;

namespace Erl.logic.nominals.xlsx
{
    internal class ExcelReader : List<ExcelString>
    {
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

                    //Console.WriteLine("Row count = {0}", rows.LongCount());
                    //Console.WriteLine("Cell count = {0}", cells.LongCount());

                    // Or... via each row
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
                MessageBox.Show(ex.Message + path + " in line #" + i.ToString() + ".");
            }
            return this;
        }
    }
}
