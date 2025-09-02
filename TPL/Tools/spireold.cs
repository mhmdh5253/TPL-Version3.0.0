//using Spire.Doc;
//using Spire.Doc.Documents;
//using Spire.Doc.Fields;
//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.IO;
//using System.Security.Policy;
//using System.Text.RegularExpressions;
//using NuGet.Packaging;
//using BorderStyle = Spire.Doc.Documents.BorderStyle;
//using Color = System.Drawing.Color;
//using Image = System.Drawing.Image;
//using Table = Spire.Doc.Table;
//using Spire.Pdf;
//using FileFormat = Spire.Doc.FileFormat;
//using Spire.Doc.Formatting;
//using Spire.License;
//using Spire.Pdf.Graphics;
//using Spire.Pdf.Graphics.Fonts;

//namespace TPLWeb.Tools
//{
//    public class LetterTemplateEngine
//    {
//        // متد اصلی برای تولید نامه با استفاده از الگو
//        public void GenerateLetterFromTemplate(
//            string templatePath,
//            string outputPath,
//            Dictionary<string, string>? textReplacements,
//            Dictionary<string, Image>? imageReplacements = null,
//            Dictionary<string, TableData>? tableReplacements = null,
//            Dictionary<string, Table>? complexTableReplacements = null)
//        {
//            // بارگذاری سند الگو
//            Document doc = new Document();
//            doc.LoadFromFile(templatePath);
//            string ss = "Idd65VXxKpEAgBvZ1nVhUN+w7vpItcbvurq9YsmKuDda+JAEE9qF2G4YzR3o0s96HLaSfKKXq8fmv/VifgjLP/ZHrAKRewKyimE+b1l5tI82tdsWa+W3TgkLfepngT3Ui+LuaUc8pxXYEPd/bacNeg6yvWi7xVPzxDsE/m3D+OyD1ifz4S4lkOhjUS4pJ9gIKv6eIx0aXzRyczi4c+55+yRRBjUsB3AUS5C4sGq4LaSbeVLRq52visiCeMQxIkO6G38uTOyJl3mplKPrB3tpSTpmDc0j1WLuce1KIA9GbtKqOgh5vJwnXnwR3qeVgEBY2Lgrt6Gu0RModahYN6N5ODyj526SSOsz50jUQsrjfnk2JYKq3D3GA+lshknDJsSyHHkqYNxXfha7GQ4e11FhxALPu81LBXLXez4l73XCV9n6cdvHnyOerI18clWh/g6lgfEG+N+ugko2oxET/WEeIVKoIvpEw9YMv5bQrD6oWlN5GthgiXawtPQ6kM41r0MKW75+6ojDqRbOqvyVwC4HNRf2MXjni/Bdo0KBG3SD119bQfa+4zBREiEz6X26Mv7Tc0n8YzGTcK7VZcRGqI06bp4RDiFvAMrn4Y83gJaVRX6MbSJqwpKXKugSrmf0ck6XzLmhQcjsznnLkToXxvBS2jh6Vy3JZXvt4l8JUF8zE9CPix+kpDcGedXA1MmN/dju6Ps4sgGGAnjrfl1YLHvbQR8kii+h9tKrUrjTT88xvjjwz5IXmC4MX2A6HjSqabQwLVm8wfwNF22Pp1nMuX5DVP2pyNMMYMHIewGlJRSQz3j/7gVbw264aeBJPGyVpxrZCRO7byu/Z8cKTk02S+vZTazhIjV4jmn8zLOsxH0wsbcEpDLw1XnrH4tUiIRDQxRO+EBtpPklyFx9Q8AYkIv91osUiQZ14MXfysJ8oHG8gqHa7uidcd+YgFc3FRlFlVXYqqQlABFg5/ZvUHUklZdiRLenTb2yfl3RffnzA1aevJcLy2sBoWUrTxZlAFu0u8D2+swu0V3juiLM8pO9VDB4gHtQh3n/cnvShuv8hls2fi0TTZvpxLdfBw==";
//            Spire.Doc.License.LicenseProvider.SetLicenseKey(ss);

//            // جایگذاری متن‌ها
//            ReplaceTextPlaceholders(doc, textReplacements);

//            // جایگذاری تصاویر (اگر وجود دارند)
//            if (imageReplacements != null && imageReplacements.Count > 0)
//            {
//                ReplaceImagePlaceholders(doc, imageReplacements);
//            }

//            // جایگذاری جداول ساده (اگر وجود دارند)
//            if (tableReplacements != null && tableReplacements.Count > 0)
//            {
//                ReplaceSimpleTablePlaceholders(doc, tableReplacements);
//            }

//            // جایگذاری جداول پیچیده (اگر وجود دارند)
//            if (complexTableReplacements != null && complexTableReplacements.Count > 0)
//            {
//                ReplaceComplexTablePlaceholders(doc, complexTableReplacements);
//            }
//            // تنظیم فونت برای تمام پاراگراف‌های سند
//            foreach (Section section in doc.Sections)
//            {
//                foreach (Paragraph paragraph in section.Paragraphs)
//                {
//                    foreach (DocumentObject obj in paragraph.ChildObjects)
//                    {
//                        if (obj is TextRange textRange)
//                        {
//                            textRange.CharacterFormat.FontName = "B Nazanin";
//                            textRange.CharacterFormat.FontSize = 13;
//                            textRange.CharacterFormat.Bidi = true;
//                        }
//                    }
//                }

//                // تنظیم فونت برای متن داخل جداول
//                foreach (Table table in section.Tables)
//                {
//                    foreach (TableRow row in table.Rows)
//                    {
//                        foreach (TableCell cell in row.Cells)
//                        {
//                            foreach (Paragraph para in cell.Paragraphs)
//                            {
//                                foreach (DocumentObject obj in para.ChildObjects)
//                                {
//                                    if (obj is TextRange textRange)
//                                    {
//                                        textRange.CharacterFormat.FontName = "B Nazanin";
//                                        textRange.CharacterFormat.FontSize = 13;
//                                        textRange.CharacterFormat.Bidi = true;
//                                    }
//                                }
//                            }
//                        }
//                    }
//                }
//            }

//            doc.EmbedFontsInFile = true;
//            // ذخیره سند نهایی با تنظیمات فونت
//            doc.SaveToFile(outputPath + ".docx", FileFormat.Docx);
//            doc.SaveToFile(outputPath + ".pdf", FileFormat.PDF);

//        }

//        // جایگذاری متن‌های ساده
//        private void ReplaceTextPlaceholders(Document doc, Dictionary<string, string> replacements)
//        {
//            foreach (Section section in doc.Sections)
//            {
//                foreach (Paragraph paragraph in section.Paragraphs)
//                {
//                    foreach (var replacement in replacements)
//                    {
//                        if (paragraph.Text.Contains(replacement.Key))
//                        {
//                            if (replacement.Key == "CONTENT")
//                            {
//                                paragraph.Format.IsBidi = true; // فعال‌سازی پشتیبانی از دوطرفه
//                                paragraph.Format.HorizontalAlignment =HorizontalAlignment.Right; // تراز به راست
//                                paragraph.Text = paragraph.Text.Replace(replacement.Key, "");
//                                paragraph.AppendHTML(AdjustHtml(replacement.Value)); // جایگذاری HTML فقط یک‌بار

//                            }
//                            else
//                            {
//                                paragraph.Text = paragraph.Text.Replace(replacement.Key, replacement.Value);

//                            }
//                        }
//                    }
//                }

//                // جایگزینی در جداول
//                foreach (Table table in section.Tables)
//                {
//                    foreach (TableRow row in table.Rows)
//                    {
//                        foreach (TableCell cell in row.Cells)
//                        {
//                            foreach (Paragraph para in cell.Paragraphs)
//                            {
//                                foreach (var replacement in replacements)
//                                {
//                                    if (para.Text.Contains(replacement.Key))
//                                    {
//                                        para.Text = para.Text.Replace(replacement.Key, replacement.Value);
//                                    }
//                                }
//                            }
//                        }
//                    }
//                }
//            }
//        }
//        private string AdjustHtml(string htmlContent)
//        {
//            // تنظیم عرض و وسط‌چین کردن تصاویر
//            string imagePattern = "<img([^>]*)>";
//            string imageReplacement = "<img$1 style='width:500px; display:block; margin:auto;'>";
//            htmlContent = Regex.Replace(htmlContent, imagePattern, imageReplacement, RegexOptions.IgnoreCase);

//            return htmlContent;
//        }



//        // جایگذاری تصاویر
//        private void ReplaceImagePlaceholders(Document doc, Dictionary<string, Image>? imageReplacements)
//        {
//            foreach (Section section in doc.Sections)
//            {
//                foreach (Paragraph paragraph in section.Paragraphs)
//                {
//                    foreach (var replacement in imageReplacements)
//                    {
//                        if (paragraph.Text.Contains(replacement.Key))
//                        {
//                            // حذف placeholder متن
//                            paragraph.Text = paragraph.Text.Replace(replacement.Key, "");

//                            // درج تصویر
//                            DocPicture picture = paragraph.AppendPicture(replacement.Value);

//                            // تنظیم اندازه تصویر (اختیاری)
//                            picture.Width = 100;
//                            picture.Height = 100;
//                            picture.HorizontalAlignment = ShapeHorizontalAlignment.Left;
//                            picture.TextWrappingStyle = TextWrappingStyle.Behind;
//                        }
//                    }
//                }
//            }
//        }

//        // جایگذاری جداول ساده
//        private void ReplaceSimpleTablePlaceholders(Document doc, Dictionary<string, TableData> tableReplacements)
//        {
//            foreach (Section section in doc.Sections)
//            {
//                foreach (Paragraph paragraph in section.Paragraphs)
//                {
//                    foreach (var replacement in tableReplacements)
//                    {
//                        if (paragraph.Text.Contains(replacement.Key))
//                        {
//                            // حذف placeholder متن
//                            paragraph.Text = paragraph.Text.Replace(replacement.Key, "");

//                            // ایجاد جدول جدید
//                            Table table = section.AddTable(true);
//                            table.ResetCells(replacement.Value.RowCount, replacement.Value.ColumnCount);

//                            // اعمال استایل به جدول
//                            ApplyDefaultTableStyle(table);

//                            // پر کردن جدول با داده‌ها
//                            FillTableData(table, replacement.Value.Data);

//                            // قرار دادن جدول بعد از پاراگراف حاوی placeholder
//                            section.Body.ChildObjects.Insert(section.Body.ChildObjects.IndexOf(paragraph) + 1, table);
//                        }
//                    }
//                }
//            }
//        }

//        // جایگذاری جداول پیچیده
//        private void ReplaceComplexTablePlaceholders(Document doc, Dictionary<string, Table> complexTableReplacements)
//        {
//            foreach (Section section in doc.Sections)
//            {
//                foreach (Paragraph paragraph in section.Paragraphs)
//                {
//                    foreach (var replacement in complexTableReplacements)
//                    {
//                        if (paragraph.Text.Contains(replacement.Key))
//                        {
//                            // حذف placeholder متن
//                            paragraph.Text = paragraph.Text.Replace(replacement.Key, "");

//                            // کپی جدول از الگو
//                            Table newTable = replacement.Value.Clone();

//                            // قرار دادن جدول بعد از پاراگراف
//                            section.Body.ChildObjects.Insert(section.Body.ChildObjects.IndexOf(paragraph) + 1, newTable);
//                        }
//                    }
//                }
//            }
//        }

//        // اعمال استایل پیش‌فرض به جدول
//        [Obsolete]
//        private void ApplyDefaultTableStyle(Table table)
//        {
//            table.TableFormat.Borders.BorderType = BorderStyle.Single;
//            table.TableFormat.Borders.LineWidth = 1f;
//            table.TableFormat.Borders.Color = Color.Black;
//            table.TableFormat.Paddings.All = 5f;
//            table.TableFormat.HorizontalAlignment = RowAlignment.Center;
//        }

//        // پر کردن جدول با داده‌ها
//        private void FillTableData(Table table, string[,] data)
//        {
//            for (int i = 0; i < data.GetLength(0); i++)
//            {
//                for (int j = 0; j < data.GetLength(1); j++)
//                {
//                    Paragraph para = table[i, j].AddParagraph();
//                    TextRange text = para.AppendText(data[i, j]);

//                    // اعمال فرمت متن
//                    text.CharacterFormat.FontName = "B Nazanin";
//                    text.CharacterFormat.FontSize = 12;
//                    para.Format.HorizontalAlignment = HorizontalAlignment.Center;
//                }
//            }
//        }

//        // متد کمکی برای تولید سند از بایت‌ها
//        public byte[] GenerateLetterAsBytes(
//            string templatePath,
//            Dictionary<string, string> textReplacements,
//            Dictionary<string, Image>? imageReplacements = null,
//            Dictionary<string, TableData> tableReplacements = null)
//        {
//            using (var memoryStream = new MemoryStream())
//            {
//                Document doc = new Document();
//                doc.LoadFromFile(templatePath);

//                // انجام جایگذاری‌ها
//                ReplaceTextPlaceholders(doc, textReplacements);

//                if (imageReplacements != null)
//                    ReplaceImagePlaceholders(doc, imageReplacements);

//                if (tableReplacements != null)
//                    ReplaceSimpleTablePlaceholders(doc, tableReplacements);

//                doc.SaveToStream(memoryStream, FileFormat.Docx);
//                return memoryStream.ToArray();
//            }
//        }
//    }

//    // کلاس کمکی برای داده‌های جدول
//    public class TableData
//    {
//        public int RowCount { get; set; }
//        public int ColumnCount { get; set; }
//        public string[,] Data { get; set; }

//        public TableData(int rows, int cols)
//        {
//            RowCount = rows;
//            ColumnCount = cols;
//            Data = new string[rows, cols];
//        }

//        // متد کمکی برای پر کردن داده‌ها
//        public void FillData(string[,] data)
//        {
//            if (data.GetLength(0) != RowCount || data.GetLength(1) != ColumnCount)
//                throw new ArgumentException("ابعاد داده با ابعاد جدول مطابقت ندارد");

//            Data = data;
//        }
//    }
//}