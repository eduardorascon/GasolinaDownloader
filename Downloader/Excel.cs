using JsonDiffPatchDotNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace Downloader
{
    public class ExcelFileReader
    {
        private string fileName = string.Empty;
        private string jsonFile = string.Empty;
        private string excelFilesDirectory = HttpRuntime.AppDomainAppPath + ConfigurationManager.AppSettings["excel_storage"];
        private string jsonFilesDirectory = HttpRuntime.AppDomainAppPath + ConfigurationManager.AppSettings["json_storage"];

        public ExcelFileReader()
        {
            string[] excelFiles = Directory.GetFiles(excelFilesDirectory);

            foreach (string s in excelFiles)
            {
                ExcelFileReader excelFileReader = new ExcelFileReader(s);
                excelFileReader.Read();
            }
        }

        public ExcelFileReader(string fileName)
        {
            if (fileName.EndsWith(".xlsx") == false)
                throw new FileFormatException(fileName);

            if (File.Exists(fileName) == false)
                throw new FileNotFoundException();

            this.fileName = fileName;
        }

        private void GenerateJsonFiles(List<Entity> entidades)
        {
            GenerateJsonEstados(entidades);
            GenerateJsonPrecios(entidades);
        }

        private void GenerateJsonEstados(List<Entity> entidades)
        {
            HashSet<string> listToJson = new HashSet<string>();
            foreach (Entity e in entidades)
                listToJson.Add(e.entidad);

            Dictionary<string, bool> dict = listToJson.ToDictionary(h => h, h => true);
            string estadosJson = JsonConvert.SerializeObject(new { estados = dict }, Formatting.Indented);

            try
            {
                string fileName = jsonFile.Replace(".json", "estados.json");
                File.WriteAllText(fileName, estadosJson);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void GenerateJsonPrecios(List<Entity> entidades)
        {
            HashSet<string> listToJson = new HashSet<string>();
            foreach (Entity e in entidades)
                listToJson.Add(e.entidad);

            Dictionary<string, Dictionary<string, string>> dict = listToJson.ToDictionary(h => h, h => new Dictionary<string, string>());
            foreach (Entity e in entidades)
            {
                var d = dict[e.entidad];
                d.Add(e.ciudad, string.Format("M:{0}|P:{1}|D:{2}", e.magna, e.premium, e.diesel));
            }

            string preciosJson = JsonConvert.SerializeObject(new { precios = dict }, Formatting.Indented);

            try
            {
                string fileName = jsonFile.Replace(".json", "precios.json");
                File.WriteAllText(fileName, preciosJson);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void Read()
        {
            jsonFile = jsonFilesDirectory + "/" + Path.GetFileName(fileName).Substring(82).Replace(".xlsx", ".json");

            ExcelPackage excelFile = new ExcelPackage(new FileInfo(fileName));
            ExcelWorksheet worksheet = excelFile.Workbook.Worksheets[1];
            ExcelWorksheet.MergeCellsCollection mergedCells = worksheet.MergedCells;

            List<Entity> entidades = new List<Entity>();
            foreach (string mc in mergedCells)
            {
                if (mc.StartsWith("C") == false)
                    continue;

                ExcelRange m = worksheet.Cells[mc.ToString()];
                int startrow = m.Start.Row;

                if (startrow <= 4)
                    continue;

                int endrow = m.End.Row;

                while (startrow <= endrow)
                {
                    string cell = "D" + startrow;
                    entidades.Add(new Entity()
                    {
                        entidad = m.First().Value.ToString(),
                        ciudad = worksheet.Cells[cell].Value.ToString(),
                        magna = worksheet.Cells[cell.Replace("D", "E")].Value.ToString(),
                        premium = worksheet.Cells[cell.Replace("D", "F")].Value.ToString(),
                        diesel = worksheet.Cells[cell.Replace("D", "G")].Value.ToString()
                    });

                    startrow++;
                }
            }

            GenerateJsonFiles(entidades);
        }

        public void DiffyPatch()
        {
            JsonDiffPatch jsonDiff = new JsonDiffPatch();
            JToken x = JObject.Parse(File.ReadAllText(@"D:\20170824precios.json"));
            JToken y = JObject.Parse(File.ReadAllText(@"D:\20170825precios.json"));
            JToken diff = jsonDiff.Diff(x, y);
            JToken patch = jsonDiff.Patch(x, diff);

            try
            {
                File.WriteAllText(jsonFilesDirectory + "diff.json", JsonConvert.SerializeObject(diff, Formatting.Indented));
                File.WriteAllText(jsonFilesDirectory + "patch.json", JsonConvert.SerializeObject(patch, Formatting.Indented));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public class Entity
        {
            public string entidad { get; set; }
            public string ciudad { get; set; }
            public string magna { get; set; }
            public string premium { get; set; }
            public string diesel { get; set; }
        }
    }
}