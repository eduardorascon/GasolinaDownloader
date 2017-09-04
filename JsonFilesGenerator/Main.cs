using DownloaderLibrary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Web;
using System.Windows.Forms;

namespace JsonFilesGenerator
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private void btnGenerar_Click(object sender, EventArgs e)
        {
            btnGenerar.Enabled = false;
            string excelFilesDirectory = ConfigurationManager.AppSettings["excel_storage"];
            string jsonFilesDirectory = ConfigurationManager.AppSettings["json_storage"];
            DateTime startDate = new DateTime(2017, 1, 1);
            DateTime endDate = new DateTime(2017, 2, 4);

            List<string> excelFiles = new List<string>();
            while (startDate <= endDate)
            {
                string excelDir = Path.Combine(excelFilesDirectory, startDate.ToString("MM"));
                string[] startExcelFile = Directory.GetFiles(excelDir, startDate.ToString("yyyyMMdd") + "*");
                excelFiles.Add(startExcelFile[0]);

                string excelFilename = Path.GetFileNameWithoutExtension(startExcelFile[0]);
                if (excelFilename.Contains("-"))
                    startDate = DateTime.ParseExact(excelFilename.Split('-')[1], "yyyyMMdd", CultureInfo.InvariantCulture);

                startDate = startDate.AddDays(1);
            }

            List<string> jsonFiles = Json.WriteJsonFiles(jsonFilesDirectory, excelFiles);
        }
    }
}
