using DownloaderLibrary;
using System.Collections.Generic;
using System.Configuration;
using System.Web;

namespace DownloadApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string url = ConfigurationManager.AppSettings["url_to_scrap"];
            List<string> excelFiles = WebScrapper.LookupForExcelFiles(url);
            string excelFilesDirectory = HttpRuntime.AppDomainAppPath + ConfigurationManager.AppSettings["excel_storage"];
            excelFiles = WebScrapper.DownloadExcelFiles(excelFilesDirectory, excelFiles);

        }
    }
}
