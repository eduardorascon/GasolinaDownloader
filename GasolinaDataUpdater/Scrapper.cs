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

            if (Directory.Exists(downloadDirectory) == false)
                Directory.CreateDirectory(downloadDirectory);

            //Return variable.
            List<string> newExcelFiles = new List<string>();
            foreach (string fileToDownload in files)
            {
                string fileName = GetFileName(fileToDownload);
                string fileDestination = Path.Combine(downloadDirectory, fileName.Substring(4, 2), fileName);

                if (File.Exists(fileDestination))
                    continue;

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
                newFile = newFile.Replace("https://", "http://");
                client.DownloadFile(fileToDownload, newFile);
            }
        }

        private static string GetFileName(string file)
        {
            string startIndexString = file.Contains("norteal") ? "norteal" : "nortedel";
            int startIndex = file.LastIndexOf(startIndexString) + startIndexString.Length;
            string[] dateArray = file.Substring(startIndex).Replace(".xlsx", "").Split(new string[] { "de" }, StringSplitOptions.None);
            string[] dayArray = dateArray[0].Split(new string[] { "al" }, StringSplitOptions.None);

            string newFileName = string.Empty;
            foreach (string day in dayArray)
            {
                if (string.IsNullOrEmpty(newFileName))
                {
                    newFileName = dateArray[2];
                    newFileName += DateTime.ParseExact(dateArray[1], "MMMM", new CultureInfo("es-MX")).ToString("MM");
                    newFileName += ("0" + day).Substring(("0" + day).Length - 2);
                    continue;
                }

                if (newFileName.Length == 8)
                {
                    newFileName += "-" + dateArray[2];
                    newFileName += DateTime.ParseExact(dateArray[1], "MMMM", new CultureInfo("es-MX")).ToString("MM");
                    newFileName += ("0" + day).Substring(("0" + day).Length - 2);
                }
            }

            return newFileName + ".xlsx";
        }
    }
}