using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;

namespace Downloader
{
    public partial class Download : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string url = ConfigurationManager.AppSettings["url_to_scrap"];
            List<string> files = WebScrapper.LookupForExcelFiles(url);

            string excelFilesDirectory = HttpRuntime.AppDomainAppPath + ConfigurationManager.AppSettings["excel_storage"];
            WebScrapper.DownloadExcelFiles(excelFilesDirectory, files);

            //ExcelFileReader efl = new ExcelFileReader();
            //efl.DiffyPatch();
            //FirebaseClient x = new FirebaseClient();
        }
    }
}