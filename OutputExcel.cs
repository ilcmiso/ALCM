using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ClosedXML.Excel;

namespace ALCM
{
    /// <summary>
    /// 償還表を Excel 形式で出力するユーティリティクラス。
    /// </summary>
    public static class OutputExcel
    {
        /// <summary>
        /// 指定された償還表データを受け取り、指定パスに Excel ファイルとして保存します。
        /// </summary>
        /// <param name="items">償還表の行データ集合。</param>
        /// <param name="filePath">保存するファイルのフルパス。</param>
        public static Task SaveAmortizationAsync(IEnumerable<AmortizationItem> items, string filePath)
        {
            return Task.Run(() =>
            {
                // 新規ワークブック作成
                using var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("償還表");

                // ヘッダー行の書き込み
                ws.Cell(1, 1).Value = "回";
                ws.Cell(1, 2).Value = "振替日";
                ws.Cell(1, 3).Value = "返済金額";
                ws.Cell(1, 4).Value = "元金額";
                ws.Cell(1, 5).Value = "利息額";
                ws.Cell(1, 6).Value = "残高";

                int row = 2;
                foreach (var item in items)
                {
                    ws.Cell(row, 1).Value = item.回数;
                    ws.Cell(row, 2).Value = item.振替日;
                    ws.Cell(row, 3).Value = item.返済金額;
                    ws.Cell(row, 4).Value = item.元金額;
                    ws.Cell(row, 5).Value = item.利息額;
                    ws.Cell(row, 6).Value = item.残高;
                    row++;
                }

                // 列幅自動調整
                ws.Columns().AdjustToContents();

                // ディレクトリが存在しない場合作成
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // ファイル保存
                workbook.SaveAs(filePath);
            });
        }
    }
}