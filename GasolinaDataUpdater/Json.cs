using DownloaderLibrary.DTO;
using JsonDiffPatchDotNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace DownloaderLibrary
{
    public class Json
    {
        static List<string> jsonFiles = new List<string>();
        public static List<string> WriteJsonFiles(string jsonDirectory, List<string> excelFiles)
        {

            foreach (string file in excelFiles)
            {
                List<PriceDTO> precios = ExcelFileReader.Read(file);
                string baseJsonFilename = Path.GetFileNameWithoutExtension(file);
                GenerateJsonFiles(jsonDirectory, baseJsonFilename, precios);
            }

            return jsonFiles;
        }
        private static void GenerateJsonFiles(string jsonDirectory, string baseJsonFilename, List<PriceDTO> precios)
        {
            try
            {
                string fileName = Path.Combine(jsonDirectory, baseJsonFilename + "estados.json");
                jsonFiles.Add(fileName);
                string estadosJson = GenerateJsonEstados(precios);
                File.WriteAllText(fileName, estadosJson);

                fileName = Path.Combine(jsonDirectory, baseJsonFilename + "precios.json");
                jsonFiles.Add(fileName);
                string preciosJson = GenerateJsonPrecios(precios);
                File.WriteAllText(fileName, preciosJson);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static string GenerateJsonEstados(List<PriceDTO> precios)
        {
            HashSet<string> listToJson = new HashSet<string>();
            foreach (PriceDTO e in precios)
                listToJson.Add(e.entidad);

            Dictionary<string, bool> dict = listToJson.ToDictionary(h => h, h => true);
            return JsonConvert.SerializeObject(dict, Formatting.Indented);
        }

        private static string GenerateJsonPrecios(List<PriceDTO> precios)
        {
            HashSet<string> listToJson = new HashSet<string>();
            foreach (PriceDTO e in precios)
                listToJson.Add(e.entidad);

            Dictionary<string, Dictionary<string, string>> dict = listToJson.ToDictionary(h => h, h => new Dictionary<string, string>());
            foreach (PriceDTO e in precios)
            {
                try
                {
                    var d = dict[e.entidad];
                    d.Add(e.ciudad, string.Format("M:{0}|P:{1}|D:{2}", e.magna, e.premium, e.diesel));
                }
                catch
                {
                    string rutaLog = ConfigurationManager.AppSettings["ruta_log"];
                    File.AppendAllText(rutaLog, string.Format("Duplicado: {0}:{1}{2}", e.entidad, e.ciudad, Environment.NewLine));
                }
            }

            return JsonConvert.SerializeObject(dict, Formatting.Indented);
        }

        public void DiffyPatch(string file1, string file2)
        {
            JsonDiffPatch jsonDiff = new JsonDiffPatch();
            JToken x = JObject.Parse(File.ReadAllText(file1));
            JToken y = JObject.Parse(File.ReadAllText(file2));
            JToken diff = jsonDiff.Diff(x, y);
            JToken patch = jsonDiff.Patch(x, diff);

            try
            {
                File.WriteAllText(Path.Combine(Path.GetDirectoryName(file1), "diff.json"), JsonConvert.SerializeObject(diff, Formatting.Indented));
                File.WriteAllText(Path.Combine(Path.GetDirectoryName(file1), "patch.json"), JsonConvert.SerializeObject(patch, Formatting.Indented));
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}