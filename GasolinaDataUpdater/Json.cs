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
        public static List<string> GenerateEstadosJsonFiles(string jsonDirectory, List<string> excelFiles)
        {
            List<string> jsonFiles = new List<string>();
            foreach (string file in excelFiles)
            {
                try
                {
                    List<PriceDTO> precios = ExcelFileReader.Read(file);
                    string fileName = Path.Combine(jsonDirectory, Path.GetFileNameWithoutExtension(file) + "estados.json");
                    jsonFiles.Add(fileName);
                    string estadosJson = GenerateJsonEstados(precios);
                    File.WriteAllText(fileName, estadosJson);
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return jsonFiles;
        }
        public static List<string> GeneratePreciosJsonFiles(string jsonDirectory, List<string> excelFiles)
        {
            List<string> jsonFiles = new List<string>();
            foreach (string file in excelFiles)
            {
                try
                {
                    List<PriceDTO> precios = ExcelFileReader.Read(file);
                    string fileName = Path.Combine(jsonDirectory, Path.GetFileNameWithoutExtension(file) + "precios.json");
                    jsonFiles.Add(fileName);
                    string preciosJson = GenerateJsonPrecios(precios, Path.GetFileNameWithoutExtension(file).Substring(0, 8));
                    File.WriteAllText(fileName, preciosJson);
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return jsonFiles;
        }

        private static string GenerateJsonEstados(List<PriceDTO> precios)
        {
            HashSet<string> listToJson = new HashSet<string>();
            foreach (PriceDTO e in precios)
                listToJson.Add(e.entidad);

            Dictionary<string, bool> dict = listToJson.ToDictionary(h => h, h => true);
            return JsonConvert.SerializeObject(dict, Formatting.Indented);
        }

        private static string GenerateJsonPrecios(List<PriceDTO> precios, string fechaArchivo)
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
                    d.Add(e.ciudad, string.Format("M:{0}|P:{1}|D:{2}|F:{3}", e.magna, e.premium, e.diesel, fechaArchivo));
                }
                catch
                {
                    string rutaLog = ConfigurationManager.AppSettings["ruta_log"];
                    File.AppendAllText(rutaLog, string.Format("Duplicado: {0}:{1}{2}", e.entidad, e.ciudad, Environment.NewLine));
                }
            }

            return JsonConvert.SerializeObject(dict, Formatting.Indented);
        }

        public static string DiffandPatch(string file1, string file2)
        {
            JsonDiffPatch jsonDiff = new JsonDiffPatch();
            JToken x = JObject.Parse(File.ReadAllText(file1));
            JToken y = JObject.Parse(File.ReadAllText(file2));

            JToken diff = jsonDiff.Diff(x, y);
            if (diff == null)
                return string.Empty;

            JToken patch = jsonDiff.Patch(x, diff);

            try
            {
                string diffFile = Path.Combine(Path.GetDirectoryName(file1), Path.GetFileNameWithoutExtension(file1) + "_diff.json");
                string patchFile = Path.Combine(Path.GetDirectoryName(file1), Path.GetFileNameWithoutExtension(file1) + "_patch.json");
                File.WriteAllText(diffFile, JsonConvert.SerializeObject(diff, Formatting.Indented));
                File.WriteAllText(patchFile, JsonConvert.SerializeObject(patch, Formatting.Indented));
                return patchFile;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void PatchFile(string file1, string file2)
        {
            JsonDiffPatch jsonDiff = new JsonDiffPatch();
            JToken x = JObject.Parse(File.ReadAllText(file1));
            JToken y = JObject.Parse(File.ReadAllText(file2));

            JToken diff = jsonDiff.Diff(x, y);
            diff = RemoveDeleteNodesFromJson(diff);
            //RemoveDeleteNodesFromJson(diff);

            if (diff == null)
                return;

            JToken patch = jsonDiff.Patch(x, diff);

            File.WriteAllText(file1, JsonConvert.SerializeObject(patch, Formatting.Indented));
        }

        private static JToken RemoveDeleteNodesFromJson(JToken diff)
        {
            if (diff == null)
                return null;

            JToken newDiff = diff.DeepClone();
            foreach (JProperty prop in diff.Children<JProperty>())
            {
                JArray t = (JArray)newDiff[prop.Name];
                JArray a = t;
                int arrayLength = a.Count;

                if (a[arrayLength - 1].Value<int>() == 0 && a[arrayLength - 2].Value<int>() == 0)
                    t.Parent.Remove();
            }

            if (newDiff.FirstOrDefault() == null)
                return null;

            return newDiff;
        }
    }
}