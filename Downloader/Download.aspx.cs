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
            HtmlNode TitleNode = doc.DocumentNode.CssSelect(".article-body").First();
            foreach (var a in TitleNode.ChildNodes.CssSelect("a"))
            {
                file = a.Attributes["href"].Value;
                if (file.EndsWith(".xlsx"))
                    Response.Write(file + "<br />");
            }

            //FileDownloader downloader = new FileDownloader();
            //downloader.Download();
        }
    }

    public class FileDownloader
    {
        public FileDownloader()
        {
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["base_url"]))
                throw new ConfigurationErrorsException("base_url not found");
        }

        public void Download()
        {
            WebClient client = new WebClient();

            string url = ConfigurationManager.AppSettings["base_url"];
            client.DownloadFile(new Uri(url), "C:/test.xlsx");
        }
    }
}