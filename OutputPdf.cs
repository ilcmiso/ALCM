using ALCM.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using IContainer = QuestPDF.Infrastructure.IContainer;

namespace ALCM
{
    public static class OutputPdf
    {
        public static Task SaveAmortizationAsync(IEnumerable<AmortizationItem> items, string filePath)
        {
            return Task.Run(() =>
            {
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Content().Column(col =>
                        {
                            col.Item().Element(e => e.PaddingBottom(10)).Text("償還表").FontSize(16).Bold();

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1); // 回
                                    columns.RelativeColumn(2); // 振替日
                                    columns.RelativeColumn(3); // 返済金額
                                    columns.RelativeColumn(4); // 元金額
                                    columns.RelativeColumn(5); // 利息額
                                    columns.RelativeColumn(6); // 残高
                                });

                                // ヘッダー行
                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("回").Bold();
                                    header.Cell().Element(CellStyle).Text("振替日").Bold();
                                    header.Cell().Element(CellStyle).Text("返済金額").Bold();
                                    header.Cell().Element(CellStyle).Text("元金額").Bold();
                                    header.Cell().Element(CellStyle).Text("利息額").Bold();
                                    header.Cell().Element(CellStyle).Text("残高").Bold();
                                });

                                // 明細行
                                foreach (var item in items)
                                {
                                    table.Cell().Element(CellStyle).Text(item.回数.ToString());
                                    table.Cell().Element(CellStyle).Text(item.振替日.ToString("yyyy/MM/dd"));
                                    table.Cell().Element(CellStyle).Text(item.返済金額.ToString("N0"));
                                    table.Cell().Element(CellStyle).Text(item.元金額.ToString("N0"));
                                    table.Cell().Element(CellStyle).Text(item.利息額.ToString("N0"));
                                    table.Cell().Element(CellStyle).Text(item.残高.ToString("N0"));
                                }
                            });
                        });
                    });
                });

                document.GeneratePdf(filePath);
            });
        }

        // 🔧 テーブルセルの共通スタイル
        private static IContainer CellStyle(IContainer container) =>
            container.PaddingVertical(4).BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2);
    }
}
