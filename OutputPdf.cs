using ALCM.Models;

#if WINDOWS || ANDROID || IOS || MACCATALYST
// PDFSharp を利用して簡単な表形式の PDF を出力するクラス。
// プロジェクトに PdfSharpCore または PdfSharp の参照が追加されていることが前提です。
using PdfSharp.Pdf;
using PdfSharp.Drawing;
#endif

namespace ALCM
{
    /// <summary>
    /// 償還表を PDF 形式で出力するユーティリティクラス。
    /// </summary>
    public static class OutputPdf
    {
        /// <summary>
        /// 指定された償還表データを受け取り、指定パスに PDF ファイルとして保存します。
        /// </summary>
        /// <param name="items">償還表の行データ集合。</param>
        /// <param name="filePath">保存するファイルのフルパス。</param>
        public static Task SaveAmortizationAsync(IEnumerable<AmortizationItem> items, string filePath)
        {
            return Task.Run(() =>
            {
#if WINDOWS || ANDROID || IOS || MACCATALYST
                // ディレクトリが存在しない場合は作成
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // PDF ドキュメントを作成
                using var document = new PdfDocument();
                var page = document.AddPage();
                // 用紙サイズやマージンを設定
                page.Size = PdfSharp.PageSize.A4;
                page.Orientation = PdfSharp.PageOrientation.Portrait;
                var gfx = XGraphics.FromPdfPage(page);
                var fontHeader = new XFont("NotoSansJP", 12);
                var fontCell = new XFont("NotoSansJP", 12);

                double startX = 40;
                double startY = 40;
                double rowHeight = 20;
                double[] columnWidths = { 40, 80, 80, 80, 80, 80 };

                // ヘッダーの描画
                string[] headers = { "回", "振替日", "返済金額", "元金額", "利息額", "残高" };
                double currentX = startX;
                for (int i = 0; i < headers.Length; i++)
                {
                    gfx.DrawString(headers[i], fontHeader, XBrushes.Black, new XRect(currentX, startY, columnWidths[i], rowHeight), XStringFormats.CenterLeft);
                    currentX += columnWidths[i];
                }

                double currentY = startY + rowHeight;
                // データ行の描画
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
                        gfx.DrawString(cells[i], fontCell, XBrushes.Black, new XRect(currentX, currentY, columnWidths[i], rowHeight), XStringFormats.CenterLeft);
                        currentX += columnWidths[i];
                    }
                    currentY += rowHeight;
                    // 次ページへの切り替え
                    if (currentY + rowHeight > page.Height - 40)
                    {
                        page = document.AddPage();
                        page.Size = PdfSharp.PageSize.A4;
                        page.Orientation = PdfSharp.PageOrientation.Portrait;
                        gfx = XGraphics.FromPdfPage(page);
                        currentY = startY;
                    }
                }

                // 保存
                document.Save(filePath);
#else
                // サポートされていないプラットフォームでは例外
                throw new PlatformNotSupportedException("このプラットフォームでは PDF 出力がサポートされていません。");
#endif
            });
        }
    }
}