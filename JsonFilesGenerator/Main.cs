using DownloaderLibrary;
using System;
using System.Collections.Generic;
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
            DateTime startDate = new DateTime(2017, 1, 1);

            //2. We need to update firebase from the startDate to the current date.
            while (startDate <= DateTime.Today)
            {
                //3. List the excel file if its name matches with the startDate variable.
                string excelDir = Path.Combine(excelFilesDirectory, startDate.ToString("MM"));
                string excelFile = Directory.GetFiles(excelDir, startDate.ToString("yyyyMMdd") + "*")[0];

                //4. Creating the json file.
                string jsonFile = Json.GeneratePreciosJsonFiles(jsonFilesDirectory, excelFile);
                
                //5. Updating firebase
                if (string.IsNullOrEmpty(jsonFile) == false)
                    FirebaseClient.UpdatePrecios(jsonFile);
                    //UpdateEstados(excelFile);

                //6. startDate value is updated to the next available date.
                string excelFilename = Path.GetFileNameWithoutExtension(excelFile);
                if (excelFilename.Contains("-"))
                    startDate = DateTime.ParseExact(excelFilename.Split('-')[1], "yyyyMMdd", CultureInfo.InvariantCulture);

                startDate = startDate.AddDays(1);
            }
        }

        private void UpdateEstados(string excelFile)
        {
            string jsonFilesDirectory = ConfigurationManager.AppSettings["json_storage"];
            string jsonFile = Json.GenerateEstadosJsonFiles(jsonFilesDirectory, excelFile);

            FirebaseClient.UpdateEstados(jsonFile);
        }
    }
}
