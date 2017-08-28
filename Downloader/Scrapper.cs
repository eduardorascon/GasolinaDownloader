using HtmlAgilityPack;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;

namespace Downloader
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
            string newfile = string.Empty;
            foreach (string fileToDownload in files)
            {
                newfile = Path.Combine(downloadDirectory, GetFileName(fileToDownload));

                if (File.Exists(newfile))
                    continue;

                DownloadExcelFile(fileToDownload, newfile);
                newExcelFiles.Add(newfile);
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
            int startIndex = file.LastIndexOf("/") + 1;
            return file.Substring(startIndex);
        }

        private static string TryAndParseFileName(string fileName)
        {
            int startIndex = fileName.LastIndexOf("/") + 1;
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