using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO.Compression;

namespace EJ.logic.ej_get
{
    public class EJDownloader
    {
        private readonly string _pathA = @"y:\B24_REP\atmrpt\journal\";
        //private string _pathA = @"d:\temp\work\log\";
        private readonly string _pathB = @"y:\B24_REP\atmrpt\journal_ipt\";
        private readonly string _targetPath;
        private readonly string _subPath = @"C:\el\";

        public string number { get; set; } // Короткий номер, только цифры
        public string fullNumber { get; set; } // Полный номер
        public string AtmType { get; set; }

        public DateTime Start { get; set; } // Дата начала цикла
        public DateTime End { get; set; } // Дата завершения цикла

        // Номер и две даты 
        public EJDownloader(string number = "", string atmType = "", DateTime start = new DateTime(), DateTime end = new DateTime(), string targetPath = "")
        {
            //TODO обработать исключение пустой или не подходящий ввод				
            try
            {
                this.number = number; // 4 или 6 чисел
                this.AtmType = atmType;
                this.Start = start;
                this.End = end;
                this._targetPath = targetPath;
                this.CreateFullName();
            }
            catch
            {
            }
        }

        // Составляем полное имя файла
        private void CreateFullName()
        {
            if (number.Length == 4)
            {
                if (AtmType == "M")
                {
                    this.fullNumber = "S1AM" + number;
                }
                else if (AtmType == "T")
                {
                    this.fullNumber = "S1AT" + number;
                }
            }
            else if (number.Length == 6)
            {
                if (AtmType == "M")
                {
                    this.fullNumber = "AM" + number;
                }
                else if (AtmType == "T")
                {
                    this.fullNumber = "AT" + number;
                }
            }
        }
        // Создаем пути к файлам .zip на сервере
        private List<string> CreatePathsToLogs()
        {
            List<string> paths = new List<string>();
            // Берем пути к файлам 
            if (this.AtmType == "M")
            {
                paths = Directory.GetFiles(this._pathA + this.number + @"\").ToList();
            }
            else
            {
                paths = Directory.GetFiles(this._pathB + this.number + @"\").ToList();
            }
            return paths;
        }
        // Создать Ej из архива на сервере
        public void CreateEjFile()
        {
            CopyFilesToSubPath();
            UnzipAllFilesAndSaveEJ();

            Process.Start(_targetPath); // Открыть Журнал
        }
        // Очистка путей и создание .doc
        private void PrepairingDirectories()
        {
            if (!Directory.Exists(this._subPath)) Directory.CreateDirectory(this._subPath);
            if (File.Exists(_targetPath))
                File.Delete(_targetPath);
            ClearDirectory(_subPath);

            var targetDir = this._targetPath.Substring(0, this._targetPath.LastIndexOf("\\") + 1);

            var word = targetDir + "Тех. заключение" + this.fullNumber + "от" + DateTime.Today.ToString("dd/MM/yyyy") + ".doc";
            if (!File.Exists(word))
                File.AppendAllText(targetDir + "Тех. заключение" + this.fullNumber + "от" + DateTime.Today.ToString("dd/MM/yyyy") + ".doc", "");

            if (!Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);
        }
        private void ClearDirectory(string path)
        {
            var g = Directory.GetFiles(path);
            foreach (var y in g) File.Delete(y);
        }
        // Копирование файлов .zip в папку c:\el
        private void CopyFilesToSubPath()
        {
            PrepairingDirectories();

            List<string> paths = CreatePathsToLogs();
            foreach (var path in paths)
            {
                string d = Regex.Match(path, @"[\d]{8}").ToString();

                if (d.Length == 8)
                {
                    var date = new DateTime(Int32.Parse(d.Substring(0, 4)), Int32.Parse(d.Substring(4, 2)), Int32.Parse(d.Substring(6, 2)), 0, 0, 0);

                    // Создать условие, чтобы доходил до конца и выходил из цикла
                    bool exit = (date.CompareTo(this.End) <= 0);
                    if (!exit) break;
                    if (date.CompareTo(this.Start) >= 0 && exit)
                    {
                        try
                        {
                            //Скачать						
                            File.Copy(Path.GetFullPath(path), Path.Combine(this._subPath, Regex.Match(path.ToString(), @".{8}_\d{8}.zip").ToString()));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            }
        }
        // Распаковка .zip
        private void UnzipAllFilesAndSaveEJ()
        {
            var files = Directory.GetFiles(_subPath);

            // TODO: Если создавать List<string> , а потом добавлять весь массив сразу в файл
            foreach (var file in files)
            {
                ZipFile.ExtractToDirectory(file, _subPath);
                File.Delete(file);
                int found = file.IndexOf(".zip");

                var e = file.Substring(0, found);

                var temp = File.ReadAllBytes(e);

                string txt = Encoding.GetEncoding(866).GetString(temp);//866

                var lines = txt.Split('\n');

                File.AppendAllLines(_targetPath, lines); //, Encoding.GetEncoding(1251)

                File.Delete(e);
            }
        }
        // prj -> full fej.txt
        public void CreateEjFromPrj()
        {
            // _targetPath
            // Взять все файлы в директории
            string path = _targetPath + @"\fej.txt";

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            var files = Directory.GetFiles(_targetPath);

            List<string> Lines = new List<string>();
            try
            {
                foreach (var f in files)
                {
                    if (Regex.IsMatch(f, @"\.PRJ"))
                    {
                        var lines = File.ReadAllLines(f, Encoding.GetEncoding(1251));

                        foreach (var l in lines)
                        {
                            if (!(Regex.IsMatch(l, @"\[\d{8}") || Regex.IsMatch(l, @"=")))
                                Lines.Add(l);
                        }
                    }
                }
                File.AppendAllLines(path, Lines);//1251

                if (File.Exists(path))
                {
                    Process.Start(path);
                }
            }
            catch { }
        }
    }
}
