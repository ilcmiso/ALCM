using ALCM.Models;

#if WINDOWS || ANDROID || IOS || MACCATALYST
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Snippets.Font;  // FailsafeFontResolver 用
#endif

namespace ALCM
{
    public static class OutputPdf
    {
        public static Task SaveAmortizationAsync(IEnumerable<AmortizationItem> items, string filePath)
        {
            return Task.Run(() =>
            {
#if WINDOWS || ANDROID || IOS || MACCATALYST
                // 最初の XFont 作成前にフォント解決方法を設定する
                if (GlobalFontSettings.FontResolver == null)
                {
#if WINDOWS
                    // Windows ではシステムフォントを利用する
                    GlobalFontSettings.UseWindowsFontsUnderWindows = true; // Arial, Times New Roman 等が使える:contentReference[oaicite:2]{index=2}
#else
                    // それ以外のプラットフォームでは安全なフォントリゾルバを使う
                    GlobalFontSettings.FontResolver = new FailsafeFontResolver(); // Segoe WP で代替:contentReference[oaicite:3]{index=3}
#endif
                }

                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using var document = new PdfDocument();
                var page = document.AddPage();
                page.Size = PdfSharp.PageSize.A4;
                page.Orientation = PdfSharp.PageOrientation.Portrait;

                var gfx = XGraphics.FromPdfPage(page);

                // NotoSansJP が無いと例外になるので存在するフォント名に変更
                var fontHeader = new XFont("Arial", 12); // フォント名は環境に合わせて変更可。指定が無効でも上記リゾルバが代替
                var fontCell = new XFont("Arial", 12);

                double startX = 40;
                double startY = 40;
                double rowHeight = 20;
                double[] columnWidths = { 40, 80, 80, 80, 80, 80 };

                string[] headers = { "回", "振替日", "返済金額", "元金額", "利息額", "残高" };
                double currentX = startX;
                for (int i = 0; i < headers.Length; i++)
                {
                    gfx.DrawString(headers[i], fontHeader, XBrushes.Black,
                        new XRect(currentX, startY, columnWidths[i], rowHeight), XStringFormats.CenterLeft);
                    currentX += columnWidths[i];
                }

                double currentY = startY + rowHeight;
                foreach (var item in items)
                {
                    currentX = startX;
                    string[] cells = {
                        item.回数.ToString(),
                        item.振替日.ToString("yyyy/MM/dd"),
                        item.返済金額.ToString("N0"),
                        item.元金額.ToString("N0"),
                        item.利息額.ToString("N0"),
                        item.残高.ToString("N0")
                    };
                    for (int i = 0; i < cells.Length; i++)
                    {
                        gfx.DrawString(cells[i], fontCell, XBrushes.Black,
                            new XRect(currentX, currentY, columnWidths[i], rowHeight), XStringFormats.CenterLeft);
                        currentX += columnWidths[i];
                    }
                    currentY += rowHeight;
                    if (currentY + rowHeight > page.Height - 40)
                    {
                        page = document.AddPage();
                        page.Size = PdfSharp.PageSize.A4;
                        page.Orientation = PdfSharp.PageOrientation.Portrait;
                        gfx = XGraphics.FromPdfPage(page);
                        currentY = startY;
                    }
                }

                document.Save(filePath);
#else
                throw new PlatformNotSupportedException("このプラットフォームでは PDF 出力がサポートされていません。");
#endif
            });
        }
    }
}
