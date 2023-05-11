using OfficeOpenXml.Style;
using OfficeOpenXml;

namespace Erl.logic.nominals
{
    internal class ExcelGeneratorErl
    {
        public byte[] Generate(DifferenceListForExcel erl)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var package = new ExcelPackage();

            var sheet = package.Workbook.Worksheets.Add("Erl");

            // Первая вкладка с исходными данными
            InitSheetForErlCounts(erl, ref sheet);

            return package.GetAsByteArray();
        }
        public void Insert(DifferenceListForExcel erl, string path)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var package = new ExcelPackage(path);

            var sheet = package.Workbook.Worksheets.Add("Difference");

            InitSheetForErlCounts(erl, ref sheet);

            package.Save();
        }

        private static void InitSheetForErlCounts(DifferenceListForExcel erl, ref ExcelWorksheet sheet)
        {
            var row = 2;
            var column = 1;

            // Header
            for (int c = 0; c < erl.Header.Count; c++)
            {
                sheet.Cells[1, column + c + 2].Value = erl.Header[c];
            }

            sheet.Cells[1, column + 2, 1, erl.cdm_length + erl.bim_length * 2 + 3 + erl.p_length].Style.Font.Bold = true;
            sheet.Columns[row, erl.cdm_length + 2 + erl.bim_length + erl.p_length + 1].Width = 5;

            sheet.Cells[row, column, erl.Count + 1, column + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin); // 1

            if (erl.cdm_length > 0)
            {
                sheet.Cells[row, column, erl.Count + 1, erl.cdm_length + column + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin); // 2
                sheet.Cells[row, column, erl.Count + 1, column + 1 + erl.cdm_length + erl.p_length].Style.Border.BorderAround(ExcelBorderStyle.Thin);// 3
            }
            if (erl.bim_length > 0)
                sheet.Cells[row, column, erl.Count + 1, column + 1 + erl.cdm_length + erl.p_length + erl.bim_length].Style.Border.BorderAround(ExcelBorderStyle.Thin); // 4
            //sheet.Cells[row, column + 2, row, erl.Upper.Count].LoadFromArrays(new object[][] { erl.Upper.ToArray() });

            //row++;

            // Body
            for (int j = 0; j < erl.Count; ++j)
            {
                if (erl[j].Card != "")
                {
                    sheet.Cells[row, column].Value = erl[j].Time.ToString();
                    sheet.Cells[row, column + 1].Value = erl[j].Card;

                    for (int i = 0; i < erl[j].Counts.Count; i++)
                    {
                        sheet.Cells[row, column + 2 + i].Value = erl[j].Counts[i];
                    }
                    row++;
                }
            }

            row += 5;

            // Unknown
            if (erl.UnKnowns.Count > 0)
            {
                foreach (var item in erl.UnKnowns)
                {
                    sheet.Cells[row, column].Value = item.Value.Time.ToString();
                    sheet.Cells[row, column + 1].Value = item.Value.Nominal;
                    sheet.Cells[row, column + 2].Value = item.Value.Count;
                    row++;
                }
            }
            // TODO: Привязать к относительной сетке
            sheet.Column(column).Width = 20;
            sheet.Column(column + 1).Width = 20;
            sheet.Protection.IsProtected = false;
        }
    }
}
