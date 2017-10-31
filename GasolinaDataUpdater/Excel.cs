using DownloaderLibrary.DTO;
using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DownloaderLibrary
{
    public class ExcelFileReader
    {
        public static List<PriceDTO> Read(string fileName)
        {
            ExcelPackage excelFile = new ExcelPackage(new FileInfo(fileName));
            ExcelWorksheet worksheet = excelFile.Workbook.Worksheets[1];
            ExcelWorksheet.MergeCellsCollection mergedCells = worksheet.MergedCells;

            //Return variable.
            List<PriceDTO> precios = new List<PriceDTO>();
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
                    precios.Add(new PriceDTO()
                    {
                        Entidad = m.First().Value.ToString(),
                        Ciudad = worksheet.Cells[cell].Value.ToString(),
                        Magna = worksheet.Cells[cell.Replace("D", "E")].Value.ToString(),
                        Premium = worksheet.Cells[cell.Replace("D", "F")].Value.ToString(),
                        Diesel = worksheet.Cells[cell.Replace("D", "G")].Value.ToString()
                    });

                    startrow++;
                }
            }

            return precios;
        }
    }
}