using HtmlAgilityPack;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace Downloader
{
    public class WebScrapper
    {
        public WebScrapper()
        {
            ScrapingBrowser Browser = new ScrapingBrowser();
            Browser.AllowAutoRedirect = true;
            Browser.AllowMetaRedirect = true;

            string url = ConfigurationManager.AppSettings["url_to_scrap"];

            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);

            string file = string.Empty;
            HtmlNode TitleNode = doc.DocumentNode.CssSelect(".article-body").First();
            foreach (var anchor in TitleNode.ChildNodes.CssSelect("a"))
            {
                file = anchor.Attributes["href"].Value;
                if (file.EndsWith(".xlsx"))
                {
                    //Response.Write(file + "<br />");
                    DownloadExcelFile(file);
                }
            }
        }
        private void DownloadExcelFile(string address)
        {
            string excelFilesDirectory = HttpRuntime.AppDomainAppPath + ConfigurationManager.AppSettings["excel_storage"];
            string fileName = excelFilesDirectory + GetFileName(address);

            if (File.Exists(fileName))
                return;

            WebClient client = new WebClient();
            try
            {
                client.DownloadFile(address, fileName);
            }
            catch (Exception)
            {
                address = address.Replace("https://", "http://");
                client.DownloadFile(address, fileName);
            }
        }

        private string GetFileName(string file)
        {
            int startIndex = file.LastIndexOf("/");
            return file.Substring(startIndex);
        }
    }
}