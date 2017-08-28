using System;

namespace Downloader
{
    public partial class Download : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            WebScrapper ws = new WebScrapper();
            ExcelFileReader efl = new ExcelFileReader();
            //efl.DiffyPatch();
            FirebaseClient x = new FirebaseClient();
        }
    }
}