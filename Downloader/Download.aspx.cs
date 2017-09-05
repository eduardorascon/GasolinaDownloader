using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using DownloaderLibrary;

namespace Downloader
{
    public partial class Download : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string url = ConfigurationManager.AppSettings["url_to_scrap"];
            List<string> excelFiles = WebScrapper.LookupForExcelFiles(url);
            string excelFilesDirectory = HttpRuntime.AppDomainAppPath + ConfigurationManager.AppSettings["excel_storage"];
            excelFiles = WebScrapper.DownloadExcelFiles(excelFilesDirectory, excelFiles);

            string jsonFilesDirectory = HttpRuntime.AppDomainAppPath + ConfigurationManager.AppSettings["json_storage"];
            List<string> estadosJsonFiles = Json.GenerateEstadosJsonFiles(jsonFilesDirectory, excelFiles);
            List<string> preciosJsonFiles = Json.GeneratePreciosJsonFiles(jsonFilesDirectory, excelFiles);

            //FirebaseClient.Update(jsonFiles);
        }
    }
}