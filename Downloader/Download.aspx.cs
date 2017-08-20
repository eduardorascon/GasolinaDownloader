using System;
using System.Configuration;
using System.Net;

namespace Downloader
{
    public partial class Download : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            FileDownloader downloader = new FileDownloader();
            downloader.Download();
        }
    }

    public class FileDownloader
    {
        public FileDownloader()
        {
        }

        public void Download()
        {
            WebClient client = new WebClient();

            string url = ConfigurationManager.AppSettings["base_url"];
            client.DownloadFile(new Uri(url), "C:/test.xlsx");
        }
    }
}