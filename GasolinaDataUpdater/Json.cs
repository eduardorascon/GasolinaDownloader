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
        public static void GenerateJsonFiles(string jsonDirectory, string excelFile)
        {
            try
            {
                string fileName = Path.GetFileNameWithoutExtension(excelFile);
                string fileDestination = Path.Combine(jsonDirectory, fileName.Substring(0, 4), fileName);

                if (File.Exists(fileDestination + "precios.json"))
                    return;

                if (Directory.Exists(Path.GetDirectoryName(fileDestination)) == false)
                    Directory.CreateDirectory(Path.GetDirectoryName(fileDestination));

                List<PriceDTO> precios = ExcelFileReader.Read(excelFile);

                string estadosJsonString = GenerateJsonEstados(precios, Path.GetFileNameWithoutExtension(excelFile).Substring(0, 8));
                File.WriteAllText(fileDestination + "estados.json", estadosJsonString);
                FirebaseClient.UpdateEstados(estadosJsonString);

                string preciosJsonString = GenerateJsonPrecios(precios, Path.GetFileNameWithoutExtension(excelFile).Substring(0, 8));
                File.WriteAllText(fileDestination + "precios.json", preciosJsonString);
                FirebaseClient.UpdatePrecios(preciosJsonString);

                string configurationJsonString = GenerateJsonConfiguration(fileName);
                FirebaseClient.SetLastUpdate(configurationJsonString);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static string GenerateJsonConfiguration(string fileName)
        {
            string lastUpdate = fileName;
            string expirationDate = fileName;
            if (fileName.Contains("-"))
            {
                lastUpdate = fileName.Split('-')[0];
                expirationDate = fileName.Split('-')[1];
            }

            Dictionary<string, string> configurationDict = new Dictionary<string, string>();
            configurationDict.Add("last_update", lastUpdate);
            configurationDict.Add("expiration_date", expirationDate);

            return JsonConvert.SerializeObject(configurationDict);
        }

        public static string GetLastUpdate()
        {
            string firebaseValue = FirebaseClient.GetLastUpdate();
            object lastUpdate = JsonConvert.DeserializeObject(firebaseValue);
            return lastUpdate.ToString();
        }

        private static string GenerateJsonEstados(List<PriceDTO> precios, string fechaArchivo)
        {
            HashSet<string> listToJson = new HashSet<string>();
            foreach (PriceDTO e in precios)
                listToJson.Add(e.Entidad);

            Dictionary<string, string> dict = listToJson.ToDictionary(h => h, h => fechaArchivo);
            return JsonConvert.SerializeObject(dict, Formatting.Indented);
        }

        private static string GenerateJsonPrecios(List<PriceDTO> precios, string fechaArchivo)
        {
            HashSet<string> listToJson = new HashSet<string>();
            foreach (PriceDTO e in precios)
                listToJson.Add(e.Entidad);

            Dictionary<string, Dictionary<string, string>> dict = listToJson.ToDictionary(h => h, h => new Dictionary<string, string>());
            foreach (PriceDTO e in precios)
            {
                try
                {
                    var d = dict[e.Entidad];
                    d.Add(e.Ciudad, string.Format("M:{0}|P:{1}|D:{2}", e.Magna, e.Premium, e.Diesel));
                }
                catch
                {
                    string rutaLog = ConfigurationManager.AppSettings["ruta_log"];
                    File.AppendAllText(rutaLog, string.Format("Duplicado: {0}:{1}{2}", e.Entidad, e.Ciudad, Environment.NewLine));
                }
            }

            HashSet<string> c = new HashSet<string>();
            c.Add(fechaArchivo);
            var dict2 = c.ToDictionary(h => h, h => dict);

            return JsonConvert.SerializeObject(dict2, Formatting.Indented);
        }
    }
}