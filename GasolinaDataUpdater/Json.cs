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
        public static string GenerateEstadosJsonFiles(string jsonDirectory, string excelFile)
        {
            try
            {
                string fileName = Path.GetFileNameWithoutExtension(excelFile) + "estados.json";
                string fileDestination = Path.Combine(jsonDirectory, fileName.Substring(4, 2), fileName);

                if (File.Exists(fileDestination))
                    return string.Empty;

                if (Directory.Exists(Path.GetDirectoryName(fileDestination)) == false)
                    Directory.CreateDirectory(Path.GetDirectoryName(fileDestination));

                List<PriceDTO> precios = ExcelFileReader.Read(excelFile);
                string estadosJson = GenerateJsonEstados(precios, Path.GetFileNameWithoutExtension(excelFile).Substring(0, 8));
                File.WriteAllText(fileDestination, estadosJson);

                return fileDestination;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static string GeneratePreciosJsonFiles(string jsonDirectory, string excelFile)
        {
            try
            {
                string fileName = Path.GetFileNameWithoutExtension(excelFile) + "precios.json";
                string fileDestination = Path.Combine(jsonDirectory, fileName.Substring(0, 4), fileName);

                if (File.Exists(fileDestination))
                    return string.Empty;

                if (Directory.Exists(Path.GetDirectoryName(fileDestination)) == false)
                    Directory.CreateDirectory(Path.GetDirectoryName(fileDestination));

                List<PriceDTO> precios = ExcelFileReader.Read(excelFile);
                string preciosJson = GenerateJsonPrecios(precios, Path.GetFileNameWithoutExtension(excelFile).Substring(0, 8));
                File.WriteAllText(fileDestination, preciosJson);

                return fileDestination;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static string GenerateJsonEstados(List<PriceDTO> precios, string fechaArchivo)
        {
            HashSet<string> listToJson = new HashSet<string>();
            foreach (PriceDTO e in precios)
                listToJson.Add(e.entidad);

            Dictionary<string, string> dict = listToJson.ToDictionary(h => h, h => fechaArchivo);
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
            //diff = RemoveDeleteNodesFromJson(diff);

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
                JArray a = (JArray)newDiff[prop.Name];
                if (a[a.Count - 1].Value<int>() == 0 && a[a.Count - 2].Value<int>() == 0)
                    a.Parent.Remove();
            }

            if (newDiff.FirstOrDefault() == null)
                return null;

            return newDiff;
        }
    }
}