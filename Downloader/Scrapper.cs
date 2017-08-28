using HtmlAgilityPack;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
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

        public static void DownloadExcelFiles(string downloadDirectory, List<string> list)
        {
            if (list.Count == 0 || string.IsNullOrEmpty(downloadDirectory))
                return;

            string fileName = string.Empty;
            foreach (string file in list)
            {
                fileName = downloadDirectory + GetFileName(file);

                if (File.Exists(fileName))
                    continue;

                DownloadExcelFile(downloadDirectory, fileName);
            }
        }

        private static void DownloadExcelFile(string downdloadDirectory, string file)
        {
            string fileName = downdloadDirectory + GetFileName(file);
            WebClient client = new WebClient();
            try
            {
                client.DownloadFile(file, fileName);
            }
            catch (Exception)
            {
                file = file.Replace("https://", "http://");
                client.DownloadFile(file, fileName);
            }
        }

        private static string GetFileName(string file)
        {
            int startIndex = file.LastIndexOf("/");
            return file.Substring(startIndex);
        }
    }
}