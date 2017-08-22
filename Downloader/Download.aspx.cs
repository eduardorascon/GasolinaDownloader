using HtmlAgilityPack;
using Newtonsoft.Json;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Downloader
{
    public partial class Download : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //var x = new FirebaseClient();
            WebScrapper ws = new WebScrapper();
        }
    }

    public class FirebaseClient
    {
        public FirebaseClient()
        {
            string firebase_url = ConfigurationManager.AppSettings["firebase_url"];
            HttpWebRequest request = WebRequest.CreateHttp(firebase_url);
            request.Method = "POST";
            request.ContentType = "application/json";

            DateTime now = DateTime.Now;
            string json = JsonConvert.SerializeObject(new
            {
                Name = now.ToString("yyyyMMdd"),
                Value = now.ToString("HHmmss")
            });

            byte[] data = Encoding.UTF8.GetBytes(json);
            request.ContentLength = data.Length;
            request.GetRequestStream().Write(data, 0, data.Length);
            WebResponse response = request.GetResponse();
            json = new StreamReader(response.GetResponseStream()).ReadToEnd();
        }
    }

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
            FileDownloader downloader = new FileDownloader();
            HtmlNode TitleNode = doc.DocumentNode.CssSelect(".article-body").First();
            foreach (var anchor in TitleNode.ChildNodes.CssSelect("a"))
            {
                file = anchor.Attributes["href"].Value;
                if (file.EndsWith(".xlsx"))
                {
                    //Response.Write(file + "<br />");
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
            string fileName = ConfigurationManager.AppSettings["local_storage"] + GetFileName(address);
            //            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

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