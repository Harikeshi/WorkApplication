using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Collections.Generic;

namespace EJ.logic.ej_xlsx.xlsx
{
    class ExcelGeneratorForEJournal
    {      
        public void Insert(string path, List<ResultListOfDay> lst, List<string> head, string name)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var package = new ExcelPackage(path);

            var row = 2;
            var column = 1;

            var sheet = InitSheet(package, row, name);

            int c = 7;
            foreach (var result in head)
            {
                sheet.Cells[row, column + c++].Value = result;
            }

            row++;


            foreach (var result in lst)
            {
                row = AddResultToForm(row, column, sheet, result);
                row += 2;
            }

            package.Save();
        }

        public byte[] Generate(List<ResultListOfDay> lst, List<string> head, string name)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var package = new ExcelPackage();

            var row = 2;
            var column = 1;

            ExcelWorksheet sheet = InitSheet(package, row, name);
            int c = 7;
            foreach (var result in head)
            {
                sheet.Cells[row, column + c++].Value = result;
            }

            row++;
            foreach (var result in lst)
            {
                row = AddResultToForm(row, column, sheet, result);
                row += 2;
            }

            return package.GetAsByteArray();
        }

        private static ExcelWorksheet InitSheet(ExcelPackage package, int row, string name)
        {
            var sheet = package.Workbook.Worksheets.Add(name);
            sheet.Column(1).Width = 20;
            sheet.Column(2).Width = 8;
            sheet.Column(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            sheet.Column(3).Width = 20;

            sheet.Columns[8, 15].Width = 5;
            return sheet;
        }

        private static int AddResultToForm(int row, int column, ExcelWorksheet sheet, ResultListOfDay result)
        {
            foreach (var item in result)
            {
                sheet.Cells[row, column].Value = item.Time;
                sheet.Cells[row, column + 1].Value = item.Number;
                sheet.Cells[row, column + 2].Value = item.Card;
                sheet.Cells[row, column + 3].Value = item.Amount1;
                sheet.Cells[row, column + 4].Value = item.Equal;
                sheet.Cells[row, column + 5].Value = item.Amount2;
                int c = 7; // Empty Column
                foreach (var i in item.Counts)
                {
                    sheet.Cells[row, column + c++].Value = i;
                }
                for (int i = 0; i < item.Comment.Count; ++i)
                {
                    sheet.Cells[row, column + c + i].Value = item.Comment[i];
                }
                row++;
            }

            return row;
        }
    }
}
