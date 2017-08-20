using HtmlAgilityPack;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using System;
using System.Configuration;
using System.Linq;
using System.Net;

namespace Downloader
{
    public partial class Download : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ScrapingBrowser Browser = new ScrapingBrowser();
            Browser.AllowAutoRedirect = true;
            Browser.AllowMetaRedirect = true;

            string url = ConfigurationManager.AppSettings["url_to_scrap"];

            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);

            string file = string.Empty;
            FileDownloader downloader = new FileDownloader();
            HtmlNode TitleNode = doc.DocumentNode.CssSelect(".article-body").First();
            foreach (var anchor in TitleNode.ChildNodes.CssSelect("a"))
            {
                file = anchor.Attributes["href"].Value;
                if (file.EndsWith(".xlsx"))
                {
                    Response.Write(file + "<br />");
                    downloader.Download(file);
                }
            }
        }
    }

    public class FileDownloader
    {
        public FileDownloader()
        {
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["local_storage"]))
                throw new ConfigurationErrorsException("local_storage not found");
        }

        public void Download(string address)
        {
            WebClient client = new WebClient();
            string fileName = ConfigurationManager.AppSettings["local_storage"] + GetFileName(address);
            client.DownloadFile(address, fileName);
        }

        private string GetFileName(string file)
        {
            int startIndex = file.LastIndexOf("/");
            return file.Substring(startIndex);
        }
    }
}