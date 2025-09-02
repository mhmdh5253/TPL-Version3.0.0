//using System.ComponentModel.DataAnnotations;
//using System.IO.Compression;
//using BE.hejrat;
//using NPOI.SS.UserModel;
//using NPOI.SS.Util;
//using NPOI.XSSF.UserModel;

//namespace TPLWeb.Tools
//{
//    public class ExcelExport
//    {
//        private readonly string _basePath = "wwwroot/excelfiles/";

//        public ExcelExport()
//        {
//            if (!Directory.Exists(_basePath))
//            {
//                Directory.CreateDirectory(_basePath);
//            }
//        }

//        public async Task<string> GenerateExcelFileAsync(List<ExcelViewModel> data)
//        {
//            IWorkbook workbook = new XSSFWorkbook();
//            AddDataToSheet(workbook, data, "لیست مهاجرین");

//            var fileName = Path.Combine(_basePath, $"output_{Guid.NewGuid().ToString().Replace("_", "").Replace("-", "")}.xlsx");

//            using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
//            {
//                workbook.Write(fileStream);
//            }

//            var zipFileName = Path.Combine(_basePath, $"output_{Guid.NewGuid()}.zip");
//            using (var zipStream = new FileStream(zipFileName, FileMode.Create))
//            {
//                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
//                {
//                    archive.CreateEntryFromFile(fileName, Path.GetFileName(fileName));
//                }
//            }

//            File.Delete(fileName);

//            return zipFileName.Replace(@"wwwroot/excelfiles/", "");
//        }

//        private void AddDataToSheet(IWorkbook workbook, List<ExcelViewModel> data, string sheetName)
//        {
//            ISheet sheet = workbook.CreateSheet(sheetName);
//            var fieldsToIgnore = new List<string> { "Timestamp", "FingersPicture", "MachedFaceid" };

//            var properties = typeof(ExcelViewModel).GetProperties().Where(p => !fieldsToIgnore.Contains(p.Name)).ToArray();
//            IRow headerRow = sheet.CreateRow(0);
//            ICellStyle headerStyle = workbook.CreateCellStyle();
//            IFont headerFont = workbook.CreateFont();
//            headerFont.FontName = "B Nazanin";
//            headerFont.FontHeightInPoints = 12;
//            headerFont.IsBold = true;
//            headerStyle.SetFont(headerFont);
//            headerStyle.FillForegroundColor = IndexedColors.Aqua.Index;
//            headerStyle.FillPattern = FillPattern.SolidForeground;

//            int columnIndex = 0;
//            foreach (var prop in properties)
//            {
//                var displayNameAttribute = prop.GetCustomAttributes(typeof(DisplayAttribute), true).FirstOrDefault() as DisplayAttribute;
//                var headerName = displayNameAttribute?.Name ?? prop.Name;

//                var cell = headerRow.CreateCell(columnIndex++);
//                cell.SetCellValue(headerName);
//                cell.CellStyle = headerStyle;
//            }

//            ICellStyle defaultStyle = workbook.CreateCellStyle();
//            IFont defaultFont = workbook.CreateFont();
//            defaultFont.FontName = "B Nazanin";
//            defaultFont.FontHeightInPoints = 12;
//            defaultStyle.SetFont(defaultFont);

//            for (int i = 0; i < data.Count; i++)
//            {
//                IRow row = sheet.CreateRow(i + 1);
//                var rowData = data[i];
//                columnIndex = 0;

//                foreach (var prop in properties)
//                {
//                    var cellValue = prop.GetValue(rowData)?.ToString() ?? "فاقد اطلاعات";
//                    if (cellValue.Length > 32767)
//                    {
//                        cellValue = cellValue.Substring(0, 32767);
//                    }

//                    var cell = row.CreateCell(columnIndex++);
//                    cell.SetCellValue(cellValue);
//                    cell.CellStyle = defaultStyle;
//                }
//            }

//            sheet.SetAutoFilter(new CellRangeAddress(0, data.Count, 0, columnIndex - 1));
//        }
//    }
//}
