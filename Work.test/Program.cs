using EJ.logic.ej_xlsx;
using EJ.logic.ej_xlsx.pre_ej;
using EJ.logic;
using Erl.logic.nominals;
using Stat.logic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Xml.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
//using DocumentFormat.OpenXml.Spreadsheet;
using System.Runtime.InteropServices;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Windows.Interop;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Vml;
using Text = DocumentFormat.OpenXml.Wordprocessing.Text;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Office.CustomUI;
using System.Xml;
using DocumentFormat.OpenXml.ExtendedProperties;
//using DocumentFormat.OpenXml.Drawing;
//using static Application.Program;

namespace Work.test
{

    internal class Program
    {

        


        static void Main(string[] args)
        {

            Console.OutputEncoding = Encoding.UTF8;

            string actual = @"d:\temp\";

            string path_in = actual + "ej.txt";
            string path_out = actual + "excel.xlsx";
            string epath_in = actual;
            string epath_out = actual + "erl.xlsx";

            string dep = actual + "depo.xlsx";
            string dis = actual + "disp.xlsx";


            string file = actual + "depos1.docx";

            string txt = actual + "depos.docx";
            string blank = actual + "blank.docx";

            string x = actual + "x.xml";

            List<string> months = new List<string> { "января", "февраля", "марта", "апреля", "мая", "июня", "июля", "августа", "сментября", "октября", "ноября", "декабря" };
            List<string> Condition = new List<string> { "Излишек", "Недостача" }; // Если подаем +/-/null излишек/недостача/не выводим 

            using (WordprocessingDocument doc = WordprocessingDocument.Open(blank, true))
            {
                //var table = doc.MainDocumentPart.Document.Body.Elements<Table>().First();

                var body = doc.MainDocumentPart.Document.Body;

                string atmNum = "AM010012";
                string sum = "1000";
                string sumt = "много тысяч";
                string Card = "220038XXXXXX5959";
                string d1 = "10/12/2024";
                string d2 = "11/12/2024";

                //var paragraphs = doc.MainDocumentPart.Document.Body.Elements<Paragraph>();

                string outxml = File.ReadAllText(actual + "par3_card.xml");

                // Параграф 7

                // 1 paragraph
                // 2 paragraph
                // 3 paragraph


                //FullMethod(actual, months, Condition, ref body, atmNum, sum, Card, d1, d2);

            }

        }

        private static void FullMethod(string actual, List<string> months, List<string> Condition, ref Body body, string atmNum, string sum, string Card, string d1, string d2)
        {
            MakeHeader(months, ref body, actual);
            body.AppendChild(new Paragraph());
            body.AppendChild(new Paragraph());

            AddFirstParagraph(atmNum, d1, d2, ref body, actual);
            body.AppendChild(new Paragraph());

            AddSecondParagraph(atmNum, ref body, Condition[0], sum, actual);
            body.AppendChild(new Paragraph());

            AddThirdParagraph(ref body, Card, sum, actual);
            body.AppendChild(new Paragraph());

            AddPre(ref body, actual);

            body.AppendChild(new Paragraph());
            body.AppendChild(new Paragraph());
            body.AppendChild(new Paragraph());
            AddSign(actual, ref body);
            //return body;
        }

        private static string AddSign(string actual, ref Body body)
        {
            string outxml = File.ReadAllText(actual + "sig01.xml");
            var first = body.AppendChild(new Paragraph(outxml));

            outxml = File.ReadAllText(actual + "sig02.xml");
            var second = body.AppendChild(new Paragraph(outxml));
            return outxml;
        }

        private static void AddFirstParagraph(string atmNum, string d1, string d2, ref Body body, string path)
        {
            string outxml = File.ReadAllText(path + "par1_period.xml");

            // Параграф 5
            var paragraph = body.AppendChild(new Paragraph(outxml));
            var runs = paragraph.Elements<Run>();

            // TODO: Добавить время
            var text = runs.ElementAt(2).Elements<Text>().First();
            text.Text = atmNum;

            text = runs.ElementAt(7).Elements<Text>().First();
            text.Text = d1;

            text = runs.ElementAt(11).Elements<Text>().First();
            text.Text = d2;
        }

        private static void AddSecondParagraph(string atmNum, ref Body body, string cond, string sum, string path)
        {
            string outxml = File.ReadAllText(path + "par2_diff.xml");

            // Параграф 7
            var paragraph = body.AppendChild(new Paragraph(outxml));
            var runs = paragraph.Elements<Run>();

            var text = runs.ElementAt(0).Elements<Text>().First();
            text.Text = cond;

            text = runs.ElementAt(4).Elements<Text>().First();
            text.Text = atmNum;

            //TODO: Добавить сумму
            text = runs.ElementAt(8).Elements<Text>().First();
            text.Text = sum;

            //TODO: перевод по формуле из десятичных в текст
            text = runs.ElementAt(11).Elements<Text>().First();
            text.Text = "много тысяч";
        }
        private static void AddThirdParagraph(ref Body body, string card, string sum, string path)
        {
            string outxml = File.ReadAllText(path + "par3_card.xml");

            // Параграф 7
            var paragraph = body.AppendChild(new Paragraph(outxml));
            var runs = paragraph.Elements<Run>();

            var text = runs.ElementAt(2).Elements<Text>().First();
            text.Text = card;

            text = runs.ElementAt(6).Elements<Text>().First();
            text.Text = sum;

        }

        private static void AddPre(ref Body body, string path)
        {
            string outxml = File.ReadAllText(path + "par4_pre.xml");
            var paragraph = body.AppendChild(new Paragraph(outxml));
        }

        private static void MakeHeader(List<string> months, ref Body body, string path)
        {
            string outxml = File.ReadAllText(path + "tbl_header.xml");

            DateTime now = DateTime.Now;
            var table = body.AppendChild(new Table(outxml));

            TableRow row = table.Elements<TableRow>().ElementAt(0);
            TableCell cell = row.Elements<TableCell>().ElementAt(0);

            var paragraphs = cell.Elements<Paragraph>();
            var runs1 = paragraphs.ElementAt(0).Elements<Run>();// Первая строка 
            var text0 = runs1.ElementAt(0).Elements<Text>().First();
            text0.Text = now.Day.ToString();
            var text1 = runs1.ElementAt(2).Elements<Text>().First();
            text1.Text = months[now.Month - 1] + " ";

            var runs2 = paragraphs.ElementAt(2).Elements<Run>();
            var text2 = runs2.ElementAt(4).Elements<Text>().First();
            text2.Text = now.Day.ToString();
            var text3 = runs2.ElementAt(6).Elements<Text>().First();
            text3.Text = now.Month.ToString();
        }

        //static void AddTable(string fileName, string json)
        //{
        //    // read the data from the json file
        //    var data = System.Text.Json.JsonSerializer.Deserialize<string[][]>(json);

        //    if (data != null)
        //    {
        //        using (var document = WordprocessingDocument.Open(fileName, true))
        //        {
        //            if (document.MainDocumentPart is null || document.MainDocumentPart.Document.Body is null)
        //            {
        //                throw new ArgumentNullException("MainDocumentPart and/or Body is null.");
        //            }

        //            var doc = document.MainDocumentPart.Document;

        //            Table table = new Table();

        //            TableProperties props = new TableProperties(
        //                new TableBorders(
        //                new TopBorder
        //                {
        //                    Val = new EnumValue<BorderValues>(BorderValues.Single),
        //                    Size = 12
        //                },
        //                new BottomBorder
        //                {
        //                    Val = new EnumValue<BorderValues>(BorderValues.Single),
        //                    Size = 12
        //                },
        //                new LeftBorder
        //                {
        //                    Val = new EnumValue<BorderValues>(BorderValues.Single),
        //                    Size = 12
        //                },
        //                new RightBorder
        //                {
        //                    Val = new EnumValue<BorderValues>(BorderValues.Single),
        //                    Size = 12
        //                },
        //                new InsideHorizontalBorder
        //                {
        //                    Val = new EnumValue<BorderValues>(BorderValues.Single),
        //                    Size = 12
        //                },
        //                new InsideVerticalBorder
        //                {
        //                    Val = new EnumValue<BorderValues>(BorderValues.Single),
        //                    Size = 12
        //                }));

        //            table.AppendChild<TableProperties>(props);

        //            for (var i = 0; i < data.Length; i++)
        //            {
        //                var tr = new TableRow();
        //                for (var j = 0; j < data[i].Length; j++)
        //                {
        //                    var tc = new TableCell();
        //                    tc.Append(new Paragraph(new Run(new Text(data[i][j]))));

        //                    // Assume you want columns that are automatically sized.
        //                    tc.Append(new TableCellProperties(
        //                        new TableCellWidth { Type = TableWidthUnitValues.Auto }));

        //                    tr.Append(tc);
        //                }
        //                table.Append(tr);
        //            }
        //            doc.Body.Append(table);
        //            doc.Save();
        //        }
        //    }

        private static void CreateHeader(Body body, Justification paragraphJustification)
        {


        }

        private static RunProperties CreateRunProp()
        {
            var runProp = new RunProperties();
            var font = new RunFonts { Ascii = "Times New Roman" };
            var size = new FontSize { Val = new StringValue("20") };

            runProp.Append(font);
            runProp.Append(size);


            return runProp;
        }

        private static Run CreateRun(string txt, RunProperties properties)
        {
            Run run = new Run(CreateRunProp());

            // String msg contains the text, "Hello, Word!"
            run.AppendChild(new Text(txt));

            return run;
        }

        private static void AddParagraph(Body body, string txt, Justification paragraphJustification)
        {

            var properties = new ParagraphProperties();

            properties.Append(paragraphJustification);

            Paragraph para = body.AppendChild(new Paragraph(properties));

            var runProp = new RunProperties();
            var font = new RunFonts { Ascii = "Times New Roman" };
            var size = new FontSize { Val = new StringValue("20") };
            runProp.Append(font);
            runProp.Append(size);

            Run run = para.AppendChild(new Run(runProp));

            // String msg contains the text, "Hello, Word!"
            run.AppendChild(new Text(txt));
        }

        private static void EJGenerateTest(string path_in, string path_out, string dep, string dis)
        {
            if (File.Exists(dep) || File.Exists(dis))
            {
                GenerateEJExcel ex = new GenerateEJExcel(path_in, path_out, dis, dep);

                System.Console.WriteLine("OK");
            }
        }


    }
}
