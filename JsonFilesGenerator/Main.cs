using DownloaderLibrary;
using System;
using System.Configuration;
using System.Globalization;
using System.IO;
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
            //1. First les get things setup.
            btnGenerar.Enabled = false;
            string excelFilesDirectory = ConfigurationManager.AppSettings["excel_storage"];
            string jsonFilesDirectory = ConfigurationManager.AppSettings["json_storage"];
            DateTime startDate = DateTime.ParseExact(Json.GetLastUpdate(), "yyyyMMdd", CultureInfo.InvariantCulture);
            DateTime expirationDate = DateTime.ParseExact(Json.GetExpirationDate(), "yyyyMMdd", CultureInfo.InvariantCulture);

            if (startDate < expirationDate)
                startDate = expirationDate.AddDays(1);

            //2. We need to update firebase from the startDate to the current date.
            while (startDate <= DateTime.Today.AddDays(1))
            {
                //3. List the excel file if its name matches with the startDate variable.
                string excelDir = Path.Combine(excelFilesDirectory, startDate.ToString("yyyy"));
                string excelFile = Directory.GetFiles(excelDir, startDate.ToString("yyyyMMdd") + "*")[0];

                //4. Creating the json file.
                Json.GenerateJsonFiles(jsonFilesDirectory, excelFile);

                //5. startDate value is updated to the next available date.
                string excelFilename = Path.GetFileNameWithoutExtension(excelFile);
                if (excelFilename.Contains("-"))
                    startDate = DateTime.ParseExact(excelFilename.Split('-')[1], "yyyyMMdd", CultureInfo.InvariantCulture);

                startDate = startDate.AddDays(1);
            }
        }
    }
}
