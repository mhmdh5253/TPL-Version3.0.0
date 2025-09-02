using HtmlAgilityPack;
using NPOI.OpenXmlFormats.Dml.WordProcessing;
using NPOI.OpenXmlFormats.Wordprocessing;
using NPOI.XWPF.UserModel;
using Spire.Doc;
using Image = System.Drawing.Image;
using System.Runtime.Versioning;

namespace TPLWeb.Tools
{
    [SupportedOSPlatform("windows")]
    public class LetterTemplateEngine
    {
        public void GenerateLetterFromTemplate(
            string templatePath,
            string outputPath,
            Dictionary<string, string> textReplacements,
            Dictionary<string, Image> imageReplacements = null!,
            Dictionary<string, TableData> tableReplacements = null!)
        {
            using (var fs = new FileStream(templatePath, FileMode.Open, FileAccess.Read))
            {
                var doc = new XWPFDocument(fs);

                // جایگذاری متن‌ها
                ReplaceTextPlaceholders(doc, textReplacements);

                // جایگذاری تصاویر
                if (imageReplacements != null && imageReplacements.Count > 0)
                {
                    ReplaceImagePlaceholders(doc, imageReplacements);
                }

                // جایگذاری جداول
                if (tableReplacements != null && tableReplacements.Count > 0)
                {
                    ReplaceTablePlaceholders(doc, tableReplacements);
                }

                // اعمال تنظیمات فارسی
                ApplyPersianSettings(doc);

                // ذخیره سند
                using (var outFs = new FileStream(outputPath + ".docx", FileMode.Create))
                {
                    doc.Write(outFs);
                }
                Spire.Doc.Document document = new Spire.Doc.Document();
                document.LoadFromFile(outputPath + ".docx");
                document.SaveToFile(outputPath + ".pdf", FileFormat.PDF);





            }
        }

        private void ReplaceTextPlaceholders(XWPFDocument doc, Dictionary<string, string> replacements)
        {
            if (replacements == null) return;

            // جایگزینی در پاراگراف‌ها
            foreach (var para in doc.Paragraphs)
            {
                foreach (var replacement in replacements)
                {
                    if (para.Text.Contains(replacement.Key))
                    {
                        if (replacement.Key == "CONTENT")
                        {
                            para.ReplaceText(replacement.Key, AdjustHtml(replacement.Value));

                        }
                        else
                        {
                            para.ReplaceText(replacement.Key, replacement.Value);

                        }

                    }
                }
            }

            // جایگزینی در جداول
            foreach (var table in doc.Tables)
            {
                foreach (var row in table.Rows)
                {
                    foreach (var cell in row.GetTableCells())
                    {
                        foreach (var para in cell.Paragraphs)
                        {
                            foreach (var replacement in replacements)
                            {
                                if (para.Text.Contains(replacement.Key))
                                {
                                    para.ReplaceText(replacement.Key, replacement.Value);
                                }
                            }
                        }
                    }
                }
            }
        }
        public string AdjustHtml(string htmlContent)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);
            return doc.DocumentNode.InnerText; // Extracts text without HTML tags
        }
        private void ReplaceImagePlaceholders(XWPFDocument doc, Dictionary<string, Image> imageReplacements)
        {
            if (imageReplacements == null || imageReplacements.Count == 0) return;

            foreach (var para in doc.Paragraphs.ToList()) // استفاده از ToList برای جلوگیری از مشکلات تغییر در حین پیمایش
            {
                foreach (var replacement in imageReplacements)
                {
                    if (para.Text.Contains(replacement.Key))
                    {
                        // حذف تمام Runهای این پاراگراف
                        while (para.Runs.Count > 0)
                        {
                            para.RemoveRun(0);
                        }

                        // ایجاد یک Run جدید برای تصویر
                        var run = para.CreateRun();
                        run.SetText(""); // متن خالی

                        // محاسبه اندازه تصویر با حفظ نسبت ابعاد
                        int desiredWidthPx = 100; // عرض دلخواه به پیکسل
                        int desiredHeightPx = (int)(replacement.Value.Height *
                                                (desiredWidthPx / (double)replacement.Value.Width));

                        // تبدیل به واحد EMU که NPOI استفاده می‌کند
                        int widthEmu = (int)(desiredWidthPx * 9525);
                        int heightEmu = (int)(desiredHeightPx * 9525);

                        using (var ms = new MemoryStream())
                        {
                            replacement.Value.Save(ms, global::System.Drawing.Imaging.ImageFormat.Png);
                            ms.Position = 0;

                            // درج تصویر
                            var picture = run.AddPicture(
                                ms,
                                (int)PictureType.PNG,
                                "image.png",
                                widthEmu,
                                heightEmu
                            );
                            var drawing = run.GetCTR().GetDrawingList()?.FirstOrDefault();
                            if (drawing != null)
                            {
                                var anchor = drawing.anchor?.FirstOrDefault();
                                if (anchor != null)
                                {
                                    anchor.simplePos1 = false; // تنظیم موقعیت ساده
                                    anchor.relativeHeight = 0; // تنظیم ارتفاع نسبی
                                    anchor.behindDoc = true; // قرار دادن تصویر پشت متن
                                    anchor.wrapThrough.wrapText = ST_WrapText.bothSides;

                                    // تنظیم موقعیت افقی و عمودی
                                    anchor.positionH = new CT_PosH() { relativeFrom = ST_RelFromH.column, posOffset = 0 };
                                    anchor.positionV = new CT_PosV() { relativeFrom = ST_RelFromV.paragraph, posOffset = 0 };

                                    // تنظیم wrap برای جلوگیری از قرارگیری متن روی تصویر
                                    anchor.wrapNone = new CT_WrapNone(); // تعیین اینکه هیچ حالت wrapping وجود نداشته باشد
                                }
                            }


                        }

                        // تنظیمات پاراگراف برای RTL
                        var pPr = para.GetCTP().pPr ?? para.GetCTP().AddNewPPr();
                        pPr.bidi = new CT_OnOff() { val = true };
                        pPr.textAlignment = new CT_TextAlignment() { val = ST_TextAlignment.auto };
                    }
                }
            }
        }
        private void ReplaceTablePlaceholders(XWPFDocument doc, Dictionary<string, TableData> tableReplacements)
        {
            foreach (var para in doc.Paragraphs)
            {
                foreach (var replacement in tableReplacements)
                {
                    if (para.Text.Contains(replacement.Key))
                    {
                        // حذف placeholder
                        para.ReplaceText(replacement.Key, "");

                        // ایجاد جدول جدید
                        var table = doc.CreateTable(replacement.Value.RowCount, replacement.Value.ColumnCount);

                        // پر کردن جدول با داده‌ها
                        for (int i = 0; i < replacement.Value.Data.GetLength(0); i++)
                        {
                            for (int j = 0; j < replacement.Value.Data.GetLength(1); j++)
                            {
                                var cell = table.GetRow(i).GetCell(j) ?? table.GetRow(i).CreateCell();
                                var cellPara = cell.Paragraphs.Count > 0 ? cell.Paragraphs[0] : cell.AddParagraph();
                                var run = cellPara.CreateRun();
                                run.SetText(replacement.Value.Data[i, j]);
                            }
                        }

                        // قرار دادن جدول بعد از پاراگراف فعلی
                        var pos = doc.GetPosOfParagraph(para);
                        doc.InsertTable(pos + 1, table);
                    }
                }
            }
        }

        private void ApplyPersianSettings(XWPFDocument doc)
        {
            // تنظیمات برای پاراگراف‌ها
            foreach (var para in doc.Paragraphs)
            {
                var pPr = para.GetCTP().pPr ?? para.GetCTP().AddNewPPr();
                pPr.bidi = new CT_OnOff() { val = true }; // فعالسازی RTL
                pPr.textAlignment = new CT_TextAlignment() { val = ST_TextAlignment.auto }; // راست‌چین

                foreach (var run in para.Runs)
                {
                    run.FontFamily = "B Nazanin"; // تنظیم فونت
                    run.FontSize = 12; // اندازه فونت
                }
            }

            // تنظیمات برای جداول
            foreach (var table in doc.Tables)
            {
                foreach (var row in table.Rows)
                {
                    foreach (var cell in row.GetTableCells())
                    {
                        foreach (var para in cell.Paragraphs)
                        {
                            var pPr = para.GetCTP().pPr ?? para.GetCTP().AddNewPPr();
                            pPr.bidi = new CT_OnOff() { val = true };
                            pPr.textAlignment = new CT_TextAlignment() { val = ST_TextAlignment.auto };

                            foreach (var run in para.Runs)
                            {
                                run.FontFamily = "B Nazanin";
                                run.FontSize = 12;
                            }
                        }
                    }
                }
            }
        }

        public byte[] GenerateLetterAsBytes(
            string templatePath,
            Dictionary<string, string> textReplacements,
            Dictionary<string, Image> imageReplacements = null!,
            Dictionary<string, TableData> tableReplacements = null!)
        {
            using (var ms = new MemoryStream())
            {
                using (var fs = new FileStream(templatePath, FileMode.Open, FileAccess.Read))
                {
                    var doc = new XWPFDocument(fs);

                    ReplaceTextPlaceholders(doc, textReplacements);

                    if (imageReplacements != null)
                        ReplaceImagePlaceholders(doc, imageReplacements);

                    if (tableReplacements != null)
                        ReplaceTablePlaceholders(doc, tableReplacements);

                    // اعمال تنظیمات فارسی
                    ApplyPersianSettings(doc);

                    doc.Write(ms);
                    return ms.ToArray();
                }
            }
        }
    }

    public class TableData
    {
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public string[,] Data { get; set; }

        public TableData(int rows, int cols)
        {
            RowCount = rows;
            ColumnCount = cols;
            Data = new string[rows, cols];
        }

        public void FillData(string[,] data)
        {
            if (data.GetLength(0) != RowCount || data.GetLength(1) != ColumnCount)
                throw new ArgumentException("ابعاد داده با ابعاد جدول مطابقت ندارد");

            Data = data;
        }
    }
}