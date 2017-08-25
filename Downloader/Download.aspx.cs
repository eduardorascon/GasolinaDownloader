using HtmlAgilityPack;
using Newtonsoft.Json;
using OfficeOpenXml;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
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
        private string GenerateJsonFiles(List<Entity> entidades)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException();

            //GenerateJsonEstados(entidades);
            GenerateJsonPrecios(entidades);

            return string.Empty;
        }

        private void GenerateJsonEstados(List<Entity> entidades)
        {
            HashSet<string> listToJson = new HashSet<string>();
            foreach (Entity e in entidades)
                listToJson.Add(e.entidad);

            Dictionary<string, bool> dict = listToJson.ToDictionary(h => h, h => true);
            string estadosJson = JsonConvert.SerializeObject(new { estados = dict });

            try
            {
                File.WriteAllText(@"D:/estados.json", estadosJson);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void GenerateJsonPrecios(List<Entity> entidades)
        {
            HashSet<string> listToJson = new HashSet<string>();
            foreach (Entity e in entidades)
                listToJson.Add(e.entidad);

            Dictionary<string, Dictionary<string, string>> dict = listToJson.ToDictionary(h => h, h => new Dictionary<string, string>());
            foreach (Entity e in entidades)
            {
                var d = dict[e.entidad];
                d.Add(e.ciudad, string.Format("M:{0}|P:{1}|D:{2}", e.magna, e.premium, e.diesel));
            }

            string preciosJson = JsonConvert.SerializeObject(new { precios = dict });

            try
            {
                File.WriteAllText(@"D:/precios.json", preciosJson);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string Read()
        {
            ExcelPackage excelFile = new ExcelPackage(new FileInfo(fileName));
            ExcelWorksheet worksheet = excelFile.Workbook.Worksheets[1];
            ExcelWorksheet.MergeCellsCollection mergedCells = worksheet.MergedCells;

            List<Entity> entidades = new List<Entity>();
            foreach (string mc in mergedCells)
            {
                if (mc.StartsWith("C") == false)
                    continue;

                ExcelRange m = worksheet.Cells[mc.ToString()];
                int startrow = m.Start.Row;

                if (startrow <= 4)
                    continue;

                int endrow = m.End.Row;

                while (startrow <= endrow)
                {
                    string cell = "D" + startrow;
                    entidades.Add(new Entity()
                    {
                        entidad = m.First().Value.ToString(),
                        ciudad = worksheet.Cells[cell].Value.ToString(),
                        magna = worksheet.Cells[cell.Replace("D", "E")].Value.ToString(),
                        premium = worksheet.Cells[cell.Replace("D", "F")].Value.ToString(),
                        diesel = worksheet.Cells[cell.Replace("D", "G")].Value.ToString()
                    });

                    startrow++;
                }
            }

            return GenerateJsonFiles(entidades);
        }

        public class Entity
        {
            public string entidad { get; set; }
            public string ciudad { get; set; }
            public string magna { get; set; }
            public string premium { get; set; }
            public string diesel { get; set; }
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