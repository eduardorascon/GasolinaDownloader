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
            DateTime startDate = new DateTime(2017, 1, 1);
            DateTime endDate = DateTime.Today;

            //2. We need to update firebase from the startDate to the current date.
            while (startDate <= endDate)
            {
                //3. Lest list the excel file if its name matches with the startDate variable.
                string excelDir = Path.Combine(excelFilesDirectory, startDate.ToString("MM"));
                string[] startExcelFile = Directory.GetFiles(excelDir, startDate.ToString("yyyyMMdd") + "*");
                string excelFile = startExcelFile[0];
                //4. Updating firebase
                UpdatePrecios(excelFile);

                //5. startDate value is updated to the next available date.
                string excelFilename = Path.GetFileNameWithoutExtension(startExcelFile[0]);
                if (excelFilename.Contains("-"))
                    startDate = DateTime.ParseExact(excelFilename.Split('-')[1], "yyyyMMdd", CultureInfo.InvariantCulture);

                startDate = startDate.AddDays(1);
            }

            UpdateEstados(excelFile);
        }

        private void UpdatePrecios(string excelFile)
        {
            string jsonFilesDirectory = ConfigurationManager.AppSettings["json_storage"];

            List<string> jsonFiles = Json.GeneratePreciosJsonFiles(jsonFilesDirectory, excelFiles);
            List<string> jsonPatchFiles = new List<string>();
            string file1 = string.Empty;
            string file2 = string.Empty;
            foreach (string file in jsonFiles)
            {
                if (file1.Equals(string.Empty))
                {
                    file1 = file;
                    continue;
                }

                file2 = file;
                string diffFile = Json.DiffandPatch(file1, file2);
                if (File.Exists(diffFile))
                    jsonPatchFiles.Add(diffFile);

                file1 = file2;
            }

            //string baseFile = Path.Combine(jsonFilesDirectory, "precios.json");
            //if (File.Exists(baseFile) == false)
            //    File.Copy(jsonFiles[0], baseFile);

            foreach (string patchFile in jsonPatchFiles)
            {
                //Json.PatchFile(baseFile, patchFile);
            }

            FirebaseClient.UpdatePrecios(jsonPatchFiles);
        }

        private void UpdateEstados(List<string> excelFiles)
        {
            string jsonFilesDirectory = ConfigurationManager.AppSettings["json_storage"];

            List<string> jsonFiles = Json.GenerateEstadosJsonFiles(jsonFilesDirectory, excelFiles);
            List<string> jsonPatchFiles = new List<string>();
            string file1 = string.Empty;
            string file2 = string.Empty;
            foreach (string file in jsonFiles)
            {
                if (file1.Equals(string.Empty))
                {
                    jsonPatchFiles.Add(file);
                    file1 = file;
                    continue;
                }

                file2 = file;
                string diffFile = Json.DiffandPatch(file1, file2);
                if (File.Exists(diffFile))
                    jsonPatchFiles.Add(diffFile);

                file1 = file2;
            }

            //string baseFile = Path.Combine(jsonFilesDirectory, "estados.json");
            //if (File.Exists(baseFile) == false)
            //    File.Copy(jsonFiles[0], baseFile);

            foreach (string patchFile in jsonPatchFiles)
            {
                //Json.PatchFile(baseFile, patchFile);
            }

            //FirebaseClient.UpdateEstados(baseFile);
        }
    }
}
