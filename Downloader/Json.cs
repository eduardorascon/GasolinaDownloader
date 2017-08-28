﻿using Downloader.DTO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;

namespace Downloader
{
    public class Json
    {
        public static void WriteJsonFiles(string jsonDirectory, List<string> excelFiles)
        {
            foreach (string file in excelFiles)
            {
                string baseJsonFilename = TryAndParseFileName(file);
                List<PriceDTO> precios = ExcelFileReader.Read(file);
                GenerateJsonFiles(jsonDirectory, baseJsonFilename, precios);
            }
        }
        private static void GenerateJsonFiles(string jsonDirectory, string baseJsonFilename, List<PriceDTO> precios)
        {
            try
            {
                string fileName = Path.Combine(jsonDirectory, baseJsonFilename + "estados.json");
                string estadosJson = GenerateJsonEstados(precios);
                File.WriteAllText(fileName, estadosJson);

                fileName = Path.Combine(jsonDirectory, baseJsonFilename + "precios.json");
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
            return JsonConvert.SerializeObject(new { estados = dict }, Formatting.Indented);
        }

        private static string GenerateJsonPrecios(List<PriceDTO> precios)
        {
            HashSet<string> listToJson = new HashSet<string>();
            foreach (PriceDTO e in precios)
                listToJson.Add(e.entidad);

            Dictionary<string, Dictionary<string, string>> dict = listToJson.ToDictionary(h => h, h => new Dictionary<string, string>());
            foreach (PriceDTO e in precios)
            {
                var d = dict[e.entidad];
                d.Add(e.ciudad, string.Format("M:{0}|P:{1}|D:{2}", e.magna, e.premium, e.diesel));
            }

            return JsonConvert.SerializeObject(new { precios = dict }, Formatting.Indented);
        }

        //public void DiffyPatch()
        //{
        //    JsonDiffPatch jsonDiff = new JsonDiffPatch();
        //    JToken x = JObject.Parse(File.ReadAllText(@"D:\20170824precios.json"));
        //    JToken y = JObject.Parse(File.ReadAllText(@"D:\20170825precios.json"));
        //    JToken diff = jsonDiff.Diff(x, y);
        //    JToken patch = jsonDiff.Patch(x, diff);

        //    try
        //    {
        //        File.WriteAllText(jsonFilesDirectory + "diff.json", JsonConvert.SerializeObject(diff, Formatting.Indented));
        //        File.WriteAllText(jsonFilesDirectory + "patch.json", JsonConvert.SerializeObject(patch, Formatting.Indented));
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}
        private static string TryAndParseFileName(string fileName)
        {
            int startIndex = fileName.LastIndexOf("\\") + 1;
            fileName = fileName.Substring(startIndex);
            fileName = fileName.Replace("Acuerdodepublicaciondepreciosmaximosdeloscombustiblesyestimulodelafronteranortedel", string.Empty);
            fileName = fileName.Replace("de", string.Empty);

            try
            {
                return DateTime.ParseExact(fileName, "ddMMMMyyyy", new CultureInfo("es-MX")).ToString("yyyyMMdd");
            }
            catch (Exception)
            {
                startIndex = fileName.LastIndexOf("al") + 2;
                fileName = fileName.Substring(startIndex).Replace(".xlsx", string.Empty);
                return DateTime.ParseExact(fileName, "ddMMMMyyyy", new CultureInfo("es-MX")).ToString("yyyyMMdd");
            }
        }
    }
}