using HtmlAgilityPack;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;

namespace DownloaderLibrary
{
    public class WebScrapper
    {
        public static List<string> LookupForExcelFiles(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("url");

            ScrapingBrowser browser = new ScrapingBrowser();
            browser.AllowAutoRedirect = true;
            browser.AllowMetaRedirect = true;

            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            HtmlNode TitleNode = doc.DocumentNode.CssSelect(".article-body").First();

            //Return variable.
            List<string> files = new List<string>();

            string file = string.Empty;
            foreach (var anchor in TitleNode.ChildNodes.CssSelect("a"))
            {
                file = anchor.Attributes["href"].Value;
                if (file.EndsWith(".xlsx") == false)
                    continue;

                files.Add(file);
            }

            return files;
        }

        public static List<string> DownloadExcelFiles(string downloadDirectory, List<string> files)
        {
            if (files.Count == 0 || string.IsNullOrEmpty(downloadDirectory))
                throw new ArgumentNullException();

            //Return variable.
            List<string> newExcelFiles = new List<string>();
            foreach (string fileToDownload in files)
            {
                string fileName = GetFileName(fileToDownload);
                string fileDestination = Path.Combine(downloadDirectory, fileName.Substring(0, 4));
                fileDestination = Path.Combine(fileDestination, fileName);
                
                if (File.Exists(fileDestination))
                    continue;

                Directory.CreateDirectory(Path.GetDirectoryName(fileDestination));
                DownloadExcelFile(fileToDownload, fileDestination);
                newExcelFiles.Add(fileDestination);
            }

            return newExcelFiles;
        }

        private static void DownloadExcelFile(string fileToDownload, string newFile)
        {
            WebClient client = new WebClient();
            try
            {
                client.DownloadFile(fileToDownload, newFile);
            }
            catch (Exception)
            {
                fileToDownload = fileToDownload.Replace("https://", "http://");
                client.DownloadFile(fileToDownload, newFile);
            }
        }

        private static string GetFileName(string file)
        {
            string startIndexString = file.Contains("PMAX") ? "PMAX" : (file.Contains("norteal") ? "norteal" : "nortedel");
            int startIndex = file.LastIndexOf(startIndexString) + startIndexString.Length;
            file = file.Substring(startIndex).Replace("de", "").Replace(".xlsx", "").Replace("al", "-");

            string[] dayArray = file.Split(new string[] { "-" }, StringSplitOptions.None);
            string newFileName = string.Empty;
            if (dayArray.Length == 2)
            {
                string temp = DateTime.ParseExact(dayArray[1], "ddMMMMyyyy", new CultureInfo("es-MX")).ToString("yyyyMMdd");
                newFileName = temp.Substring(0, temp.Length - 2) + dayArray[0] + "-" + temp;
            }
            else
            {
                DateTime tempDate;
                if (DateTime.TryParseExact(dayArray[0], "ddMMMMyyyy", new CultureInfo("es-MX"), DateTimeStyles.None, out tempDate))
                    newFileName = tempDate.ToString("yyyyMMdd");
                else
                    newFileName = DateTime.ParseExact(dayArray[0], "dMMMMyyyy", new CultureInfo("es-MX")).ToString("yyyyMMdd");
            }


            return newFileName + ".xlsx";
        }
    }
}