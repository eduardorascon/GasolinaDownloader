using HtmlAgilityPack;
using Newtonsoft.Json;
using OfficeOpenXml;
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
            ExcelFileReader efl = new ExcelFileReader(@"C:/Acuerdodepublicaciondepreciosmaximosdeloscombustiblesyestimulodelafronteranortedel22deAgostode2017.xlsx");
            efl.Read();
            //FirebaseClient x = new FirebaseClient();
            //WebScrapper ws = new WebScrapper();
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

            //TODO
            //To be replaced with string json = File.ReadAll(filename);
            //Filename should be a .json file
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

    public class ExcelFileReader
    {
        private string fileName = string.Empty;
        public ExcelFileReader(string fileName)
        {
            if (fileName.EndsWith(".xlsx") == false)
                throw new FileFormatException(fileName);

            if (File.Exists(fileName) == false)
                throw new FileNotFoundException();

            this.fileName = fileName;
        }
        private void GenerateJson()
        {
            if (string.IsNullOrEmpty(fileName))
                return;

            //sample
            //using (StreamWriter file = File.CreateText(@"c:\videogames.json")
        }
        public string Read()
        {
            ExcelPackage excelFile = new ExcelPackage(new FileInfo(fileName));
            ExcelWorksheet worksheet = excelFile.Workbook.Worksheets[1];
            ExcelWorksheet.MergeCellsCollection mergedCells = worksheet.MergedCells;
            foreach (ExcelRange m in mergedCells)
            {
                if (m.Address.StartsWith("C") == false)
                    continue;

                int startrow = m.Start.Row;
                int endrow = m.End.Row;

                while (startrow != endrow)
                {
                    Entity e = new Entity(m.Value.ToString(), worksheet.Cells["D" + startrow].Value.ToString());
                    startrow++;
                }
            }

            return string.Empty;
        }

        class Entity
        {
            private string entidad { get; }
            private string ciudad { get; }
            private string magna { get; }
            private string premium { get; }
            private string diesel { get; }
            public Entity(string entidad, string ciudad)
            {

            }
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